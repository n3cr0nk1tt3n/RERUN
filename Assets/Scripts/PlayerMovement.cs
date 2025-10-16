using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    // === References ===
    private Rigidbody2D rb;
    private Animator animator;

    // === Input ===
    private float horizontalInput;
    private bool isCrouching;

    // === Ground Check ===
    private bool isGrounded;

    [Header("Ground Check Settings")]
    public Transform groundCheck;
    public float groundCheckRadius = 0.2f;
    public LayerMask groundLayer;

    // === Movement Settings ===
    [Header("Movement Settings")]
    public float maxSpeed = 5f;
    public float moveSpeed = 10f;
    public float acceleration = 10f;
    public float deceleration = 15f;

    // === Jump Settings ===
    [Header("Jump Settings")]
    public float jumpForce = 10f;
    public int maxJumpCount = 2;
    [Range(0f, 1f)]
    public float jumpCutMultiplier = 0.5f;
    public float coyoteTime = 0.2f;
    public float jumpBufferTime = 0.2f;

    // === Internal Tracking ===
    private float coyoteTimeCounter;
    private float jumpBufferCounter;
    private int currentJumpCount;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
    }

    void Update()
    {
        // === Input ===
        horizontalInput = Input.GetAxisRaw("Horizontal");
        isCrouching = Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow);

        // === Ground Check (Raycast down only) ===
        RaycastHit2D hit = Physics2D.Raycast(groundCheck.position, Vector2.down, groundCheckRadius, groundLayer);
        isGrounded = hit.collider != null;

        // === Reset Jump Count on Ground ===
        if (isGrounded)
        {
            currentJumpCount = 0;
        }

        // === Coyote Time ===
        if (isGrounded)
        {
            coyoteTimeCounter = coyoteTime;
        }
        else
        {
            coyoteTimeCounter -= Time.deltaTime;
        }

        // === Jump Buffering ===
        if (Input.GetButtonDown("Jump"))
        {
            jumpBufferCounter = jumpBufferTime;
        }
        else
        {
            jumpBufferCounter -= Time.deltaTime;
        }

        // === Jump Execution (ONLY if not crouching) ===
        if (jumpBufferCounter > 0f && (coyoteTimeCounter > 0f || currentJumpCount < maxJumpCount))
        {
            if (!isCrouching) // Must be standing to jump
            {
                Jump();
                jumpBufferCounter = 0f;
            }
        }

        // === Variable Jump Height ===
        if (Input.GetButtonUp("Jump") && rb.velocity.y > 0f)
        {
            rb.velocity = new Vector2(rb.velocity.x, rb.velocity.y * jumpCutMultiplier);
        }

        // === Animator Updates ===
        if (isCrouching && isGrounded)
        {
            animator.SetBool("IsCrouching", true);
            animator.SetFloat("Speed", 0f); // prevent run animation
        }
        else
        {
            animator.SetBool("IsCrouching", false);
            animator.SetFloat("Speed", Mathf.Abs(horizontalInput));
        }

        animator.SetBool("IsJumping", !isGrounded);

        // === Flip Sprite to Face Movement Direction ===
        if (horizontalInput > 0.01f)
        {
            transform.localScale = new Vector3(1f, 1f, 1f);
        }
        else if (horizontalInput < -0.01f)
        {
            transform.localScale = new Vector3(-1f, 1f, 1f);
        }
    }

    void FixedUpdate()
    {
        Move();
    }

    /// <summary>
    /// Applies smooth horizontal movement with acceleration and deceleration.
    /// Prevents movement while crouching.
    /// </summary>
    void Move()
    {
        if (isCrouching && isGrounded)
        {
            // Lock movement while crouched
            rb.velocity = new Vector2(0f, rb.velocity.y);
            return;
        }

        float targetSpeed = horizontalInput * moveSpeed;
        float speedDifference = targetSpeed - rb.velocity.x;

        float accelRate = (Mathf.Abs(targetSpeed) > 0.01f) ? acceleration : deceleration;

        float movement = speedDifference * accelRate;

        rb.AddForce(Vector2.right * movement);

        // Clamp max speed
        if (Mathf.Abs(rb.velocity.x) > maxSpeed)
        {
            rb.velocity = new Vector2(Mathf.Sign(rb.velocity.x) * maxSpeed, rb.velocity.y);
        }
    }

    /// <summary>
    /// Applies upward jump force and handles jump tracking.
    /// </summary>
    void Jump()
    {
        rb.velocity = new Vector2(rb.velocity.x, 0f); // Cancel vertical velocity before jump
        rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
        coyoteTimeCounter = 0f;
        currentJumpCount++;
    }

    /// <summary>
    /// Draws the ground check gizmos in the Scene view.
    /// </summary>
    void OnDrawGizmosSelected()
    {
        if (groundCheck != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);

            Gizmos.color = Color.red;
            Gizmos.DrawLine(groundCheck.position, groundCheck.position + Vector3.down * groundCheckRadius);
        }
    }
}
