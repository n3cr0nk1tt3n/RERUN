using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(CapsuleCollider2D))]
public class PlayerMovement2D : MonoBehaviour
{
    // references
    Rigidbody2D rb;
    Animator animator;
    CapsuleCollider2D col;

    // optional debugging
    [Header("Debug")]
    public bool logAnimatorValidation = false;

    // cache which animator params exist
    bool hasSpeedParam, hasIsCrouchParam, hasIsJumpingParam, hasYVelParam;

    // input
    float inputX;
    bool inputJumpPressed;
    bool inputJumpHeld;
    bool inputCrouchHeld;

    // surfaces (ground and walls share the same mask)
    [Header("Surface Checks")]
    [Tooltip("Layer mask that includes all solid surfaces, both floors and walls.")]
    public LayerMask surfaceLayer;

    [Header("Ground Check")]
    public Transform groundCheck;               // auto-created if null
    public float groundCheckRadius = 0.1f;
    public float groundEpsilon = 0.02f;
    bool isGrounded;

    // movement
    [Header("Movement")]
    public float maxSpeed = 5f;
    public float moveSpeed = 10f;
    public float acceleration = 15f;
    public float deceleration = 20f;

    // jump
    [Header("Jump")]
    public float jumpForce = 12f;
    public int maxJumpCount = 2;
    [Range(0f, 1f)] public float jumpCutMultiplier = 0.5f;
    public float coyoteTime = 0.12f;
    public float jumpBufferTime = 0.12f;

    float coyoteTimer;
    float jumpBufferTimer;
    int jumpsUsed;

    // crouch collider morph
    [Header("Collider Crouch")]
    public float crouchHeightFactor = 0.5f;   // 0.5 = half height
    public float colliderLerpSpeed = 20f;
    Vector2 standSize, standOffset;
    Vector2 crouchSize, crouchOffset;

    // wall stick prevention using normals
    [Header("Wall Classification")]
    [Tooltip("Cast distance to probe for walls in the input direction.")]
    public float wallCheckDistance = 0.05f;
    [Tooltip("Minimum absolute X of a normal to count as a wall. 0.8 means near vertical.")]
    [Range(0.0f, 1.0f)]
    public float wallNormalMinAbsX = 0.8f;
    [Tooltip("If a contact has normal.y >= this, treat it as floor/slope, not a wall.")]
    [Range(0.0f, 1.0f)]
    public float floorNormalMinY = 0.4f;

    // animator parameter hashes
    static readonly int HashSpeed     = Animator.StringToHash("Speed");
    static readonly int HashIsCrouch  = Animator.StringToHash("IsCrouching");
    static readonly int HashIsJumping = Animator.StringToHash("IsJumping");
    static readonly int HashYVel      = Animator.StringToHash("YVel");

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        col = GetComponent<CapsuleCollider2D>();

        // Rigidbody defaults
        rb.interpolation = RigidbodyInterpolation2D.Interpolate;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        rb.freezeRotation = true;

        EnsureGroundCheck();
        CacheColliderStates();

        DetectAnimatorParams();
        if (logAnimatorValidation) LogAnimatorValidation();
    }

    void Reset()
    {
        var rb2 = GetComponent<Rigidbody2D>();
        if (rb2 != null)
        {
            rb2.gravityScale = 3f;
            rb2.interpolation = RigidbodyInterpolation2D.Interpolate;
            rb2.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            rb2.freezeRotation = true;
        }

        var c = GetComponent<CapsuleCollider2D>();
        if (c != null)
        {
            c.direction = CapsuleDirection2D.Vertical;
            c.size = new Vector2(0.6f, 1.6f);
            c.offset = Vector2.zero;
        }

        EnsureGroundCheck();
        CacheColliderStates();
    }

    void OnValidate()
    {
        crouchHeightFactor = Mathf.Clamp(crouchHeightFactor, 0.2f, 1f);
        colliderLerpSpeed = Mathf.Max(0f, colliderLerpSpeed);
        wallNormalMinAbsX = Mathf.Clamp01(wallNormalMinAbsX);
        floorNormalMinY = Mathf.Clamp01(floorNormalMinY);

        if (col == null) col = GetComponent<CapsuleCollider2D>();
        if (col != null) AlignGroundCheckToFeet();
    }

    void EnsureGroundCheck()
    {
        if (groundCheck == null)
        {
            var t = new GameObject("groundCheck").transform;
            t.SetParent(transform);
            t.localPosition = Vector3.zero;
            groundCheck = t;
        }
        AlignGroundCheckToFeet();
    }

    void CacheColliderStates()
    {
        if (col == null) return;

        standSize = col.size;
        standOffset = col.offset;

        float bottomY = standOffset.y - standSize.y * 0.5f;
        crouchSize = new Vector2(standSize.x, Mathf.Max(0.1f, standSize.y * crouchHeightFactor));
        float crouchOffsetY = bottomY + crouchSize.y * 0.5f;
        crouchOffset = new Vector2(standOffset.x, crouchOffsetY);
    }

    void DetectAnimatorParams()
    {
        hasSpeedParam     = HasParam(animator, HashSpeed, AnimatorControllerParameterType.Float);
        hasIsCrouchParam  = HasParam(animator, HashIsCrouch, AnimatorControllerParameterType.Bool);
        hasIsJumpingParam = HasParam(animator, HashIsJumping, AnimatorControllerParameterType.Bool);
        hasYVelParam      = HasParam(animator, HashYVel, AnimatorControllerParameterType.Float);
    }

    static bool HasParam(Animator a, int hash, AnimatorControllerParameterType type)
    {
        if (a == null) return false;
        foreach (var p in a.parameters)
            if (p.nameHash == hash && p.type == type)
                return true;
        return false;
    }

    void LogAnimatorValidation()
    {
        if (!hasSpeedParam)     Debug.LogError("Animator missing float parameter 'Speed'", this);
        if (!hasIsCrouchParam)  Debug.LogError("Animator missing bool parameter 'IsCrouching'", this);
        if (!hasIsJumpingParam) Debug.LogError("Animator missing bool parameter 'IsJumping'", this);
        if (!hasYVelParam)      Debug.LogError("Animator missing float parameter 'YVel'", this);
    }

    void Update()
    {
        // input
        inputX = Input.GetAxisRaw("Horizontal");
        inputCrouchHeld = Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow);
        inputJumpPressed = Input.GetButtonDown("Jump");
        inputJumpHeld = Input.GetButton("Jump");

        // ground check
        AlignGroundCheckToFeet();
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, surfaceLayer);

        // reset jumps when grounded
        if (isGrounded) jumpsUsed = 0;

        // timers
        coyoteTimer = isGrounded ? coyoteTime : Mathf.Max(0f, coyoteTimer - Time.deltaTime);
        if (inputJumpPressed) jumpBufferTimer = jumpBufferTime;
        else jumpBufferTimer = Mathf.Max(0f, jumpBufferTimer - Time.deltaTime);

        // buffered jump
        bool canExtraJump = jumpsUsed < maxJumpCount - 1;
        if (jumpBufferTimer > 0f && (coyoteTimer > 0f || canExtraJump))
        {
            if (!inputCrouchHeld)
            {
                DoJump();
                jumpBufferTimer = 0f;
            }
        }

        // variable jump height
        if (!inputJumpHeld && rb.velocity.y > 0f)
            rb.velocity = new Vector2(rb.velocity.x, rb.velocity.y * jumpCutMultiplier);

        // animator params if present
        float speed01 = Mathf.Clamp01(Mathf.Abs(rb.velocity.x) / Mathf.Max(0.01f, maxSpeed));
        if (hasIsCrouchParam)  animator.SetBool(HashIsCrouch, inputCrouchHeld && isGrounded);
        if (hasIsJumpingParam) animator.SetBool(HashIsJumping, !isGrounded);
        if (hasSpeedParam)     animator.SetFloat(HashSpeed, speed01);
        if (hasYVelParam)      animator.SetFloat(HashYVel, rb.velocity.y);

        // face movement direction
        if (inputX > 0.01f) transform.localScale = new Vector3(1f, 1f, 1f);
        else if (inputX < -0.01f) transform.localScale = new Vector3(-1f, 1f, 1f);
    }

    void FixedUpdate()
    {
        ApplyHorizontalMovement();
    }

    void LateUpdate()
    {
        // morph collider based on crouch
        Vector2 targetSize = (inputCrouchHeld && isGrounded) ? crouchSize : standSize;
        Vector2 targetOffset = (inputCrouchHeld && isGrounded) ? crouchOffset : standOffset;

        // ceiling safety
        if (!(inputCrouchHeld && isGrounded) && !CanStandUp())
        {
            targetSize = crouchSize;
            targetOffset = crouchOffset;
        }

        if (colliderLerpSpeed > 0f)
        {
            col.size = Vector2.Lerp(col.size, targetSize, colliderLerpSpeed * Time.deltaTime);
            col.offset = Vector2.Lerp(col.offset, targetOffset, colliderLerpSpeed * Time.deltaTime);
        }
        else
        {
            col.size = targetSize;
            col.offset = targetOffset;
        }

        AlignGroundCheckToFeet();
    }

    void ApplyHorizontalMovement()
    {
        // lock horizontal while crouched on ground
        if (inputCrouchHeld && isGrounded)
        {
            rb.velocity = new Vector2(0f, rb.velocity.y);
            return;
        }

        float target = inputX * moveSpeed;

        // wall stick fix: if pressing into a wall, stop pushing and clear x
        if (IsPressingIntoWall(inputX))
        {
            target = 0f;
            if (Mathf.Sign(rb.velocity.x) == Mathf.Sign(inputX))
                rb.velocity = new Vector2(0f, rb.velocity.y);
        }

        float speedDiff = target - rb.velocity.x;
        float accel = Mathf.Abs(target) > 0.01f ? acceleration : deceleration;
        float force = speedDiff * accel;

        rb.AddForce(new Vector2(force, 0f));

        // clamp horizontal speed
        if (Mathf.Abs(rb.velocity.x) > maxSpeed)
            rb.velocity = new Vector2(Mathf.Sign(rb.velocity.x) * maxSpeed, rb.velocity.y);
    }

    void DoJump()
    {
        if (coyoteTimer > 0f) jumpsUsed = 0;
        rb.velocity = new Vector2(rb.velocity.x, 0f);
        rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
        coyoteTimer = 0f;
        jumpsUsed++;
    }

    bool CanStandUp()
    {
        float deltaH = standSize.y - col.size.y;
        if (deltaH <= 0.001f) return true;

        float extra = 0.02f;
        Vector2 castDir = Vector2.up;
        float castDist = deltaH * 0.5f + extra;

        var filter = new ContactFilter2D { useLayerMask = true, layerMask = surfaceLayer, useTriggers = false };
        RaycastHit2D[] hits = new RaycastHit2D[2];
        int count = col.Cast(castDir, filter, hits, castDist);
        return count == 0;
    }

    bool IsPressingIntoWall(float inputDir)
    {
        if (Mathf.Abs(inputDir) < 0.01f) return false;

        Vector2 dir = new Vector2(Mathf.Sign(inputDir), 0f);
        var filter = new ContactFilter2D { useLayerMask = true, layerMask = surfaceLayer, useTriggers = false };
        RaycastHit2D[] hits = new RaycastHit2D[3];

        int count = col.Cast(dir, filter, hits, wallCheckDistance);
        if (count <= 0) return false;

        for (int i = 0; i < count; i++)
        {
            Vector2 n = hits[i].normal;
            // treat as wall if near vertical and not a floor-like slope
            if (Mathf.Abs(n.x) >= wallNormalMinAbsX && n.y < floorNormalMinY)
                return true;
        }
        return false;
    }

    void AlignGroundCheckToFeet()
    {
        if (groundCheck == null || col == null) return;

        Vector2 localBottom = col.offset + Vector2.down * (col.size.y * 0.5f);
        groundCheck.localPosition = new Vector3(localBottom.x, localBottom.y - groundEpsilon, 0f);
    }

    void OnDrawGizmosSelected()
    {
        if (groundCheck != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
        }
    }
}
