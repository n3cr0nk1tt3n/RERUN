using UnityEngine;
using System.Collections;

public class PlayerRespawn : MonoBehaviour
{
    [Header("Player Settings")]
    public int maxHealth = 3;
    public int currentHealth;

    [Header("Respawn Settings")]
    [Tooltip("Optional tiny delay so the FX spawns before we teleport.")]
    public float respawnDelay = 0.05f;

    private Vector2 defaultStartPoint;
    private Vector2 respawnPoint;
    private bool hasReachedCheckpoint = false;

    [Header("Audio Settings")]
    public AudioSource audioSource;
    public AudioClip respawnClip;

    [Header("Death FX")]
    [Tooltip("Prefab with SpriteRenderer + Animator whose default state is the 'HeroDeath' clip.")]
    public GameObject deathAnimPrefab;
    [Tooltip("Vertical offset so the FX appears at the player's feet.")]
    public float deathOffsetY = -0.1f;
    [Tooltip("Optional parent for spawned FX. Leave null to spawn at the root.")]
    public Transform fxParent;

    [Header("Animator Options")]
    [Tooltip("If you pause Time.timeScale on death, set this true so the FX still animates.")]
    public bool deathFxUsesUnscaledTime = false;

    // cached
    private SpriteRenderer playerSR;

    void Start()
    {
        currentHealth = maxHealth;

        defaultStartPoint = transform.position;
        respawnPoint = defaultStartPoint;

        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();

        playerSR = GetComponent<SpriteRenderer>();
    }

    /// <summary>
    /// Update the active checkpoint position.
    /// </summary>
    public void UpdateCheckpoint(Vector2 newCheckpoint)
    {
        respawnPoint = newCheckpoint;
        hasReachedCheckpoint = true;
        // Debug.Log($"[Respawn] Checkpoint set: {respawnPoint}");
    }

    /// <summary>
    /// Apply damage; triggers death when health <= 0.
    /// </summary>
    public void TakeDamage(int amount)
    {
        currentHealth -= amount;
        if (currentHealth <= 0)
            DieAndRespawn();
    }

    /// <summary>
    /// Public entry point so other scripts (HazardZone, enemies, etc.) can trigger death+respawn.
    /// </summary>
    public void DieAndRespawn()
    {
        StartCoroutine(DieAndRespawnRoutine());
    }

    /// <summary>
    /// Internal flow: spawn FX at death spot, tiny delay, then teleport and restore.
    /// </summary>
    private IEnumerator DieAndRespawnRoutine()
    {
        Vector2 deathPos = transform.position;
        bool facingLeft = transform.localScale.x < 0f;

        // 1) Spawn the death animation at the death position
        SpawnDeathFX(deathPos, facingLeft);

        // 2) Optional sfx
        if (audioSource != null && respawnClip != null)
            audioSource.PlayOneShot(respawnClip);

        // 3) Let one frame (or a tiny delay) pass so Instantiate completes even if you disable/teleport after
        if (respawnDelay > 0f)
            yield return new WaitForSeconds(respawnDelay);
        else
            yield return null;

        // 4) Teleport to checkpoint/start and restore health
        Vector2 spawnPosition = hasReachedCheckpoint ? respawnPoint : defaultStartPoint;
        transform.position = spawnPosition;

        currentHealth = maxHealth;
    }

    /// <summary>
    /// Spawns the HeroDeath FX at a position, copies sorting from player, forces clip to play, and auto-destroys.
    /// </summary>
    private void SpawnDeathFX(Vector2 deathPos, bool facingLeft)
    {
        if (deathAnimPrefab == null)
        {
            Debug.LogWarning("[Respawn] deathAnimPrefab not assigned.");
            return;
        }

        Vector3 fxPos = new Vector3(deathPos.x, deathPos.y + deathOffsetY, 0f);
        GameObject fx = Instantiate(deathAnimPrefab, fxPos, Quaternion.identity, fxParent);
        if (fx == null) return;

        // Copy sorting + facing from the player (simple case: single SpriteRenderer on root)
        var fxSR = fx.GetComponent<SpriteRenderer>();
        if (fxSR != null && playerSR != null)
        {
            fxSR.sortingLayerID = playerSR.sortingLayerID;
            fxSR.sortingOrder   = playerSR.sortingOrder;
            fxSR.flipX          = facingLeft;
        }

        // Force the Animator to play the death clip from t=0 and destroy after its length
        var fxAnim = fx.GetComponent<Animator>();
        if (fxAnim != null)
        {
            fxAnim.cullingMode = AnimatorCullingMode.AlwaysAnimate;
            fxAnim.updateMode  = deathFxUsesUnscaledTime ? AnimatorUpdateMode.UnscaledTime : AnimatorUpdateMode.Normal;
            fxAnim.speed       = 1f;

            // Play by state name (state in controller must be named "HeroDeath" or adjust below)
            fxAnim.Play("HeroDeath", 0, 0f);

            // Auto-destroy after first clip length (fallback = 1s)
            float lifetime = 1.0f;
            var ctrl = fxAnim.runtimeAnimatorController;
            if (ctrl != null && ctrl.animationClips != null && ctrl.animationClips.Length > 0)
                lifetime = Mathf.Max(0.05f, ctrl.animationClips[0].length + 0.05f);

            Destroy(fx, lifetime);
        }
        else
        {
            // No animator? Destroy after a short default.
            Destroy(fx, 1.0f);
        }
    }
}
