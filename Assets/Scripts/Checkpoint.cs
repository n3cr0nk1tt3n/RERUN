using UnityEngine;

public class Checkpoint : MonoBehaviour
{
    [Header("Audio Settings")]
    public AudioSource audioSource;
    public AudioClip checkpointClip;

    private bool activated = false;

    private Collider2D checkpointCollider;
    private SpriteRenderer spriteRenderer;

    private void Start()
    {
        // Auto-assign AudioSource
        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
        }

        // Cache components
        checkpointCollider = GetComponent<Collider2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (activated) return;
        if (!collision.CompareTag("Player")) return;

        PlayerRespawn respawn = collision.GetComponent<PlayerRespawn>();
        if (respawn != null)
        {
            // Set checkpoint in respawn system
            respawn.UpdateCheckpoint(transform.position);

            // Play audio
            if (audioSource != null && checkpointClip != null)
            {
                audioSource.PlayOneShot(checkpointClip);
            }

            // Tell the manager this is now the active checkpoint
            CheckpointManager.Instance.ActivateCheckpoint(this);

            activated = true;
            Debug.Log("Checkpoint updated to: " + transform.position);
        }
    }

    // Called by manager to hide this checkpoint when active
    public void Hide()
    {
        if (checkpointCollider != null) checkpointCollider.enabled = false;
        if (spriteRenderer != null) spriteRenderer.enabled = false;
        activated = true;
    }

    // Called by manager to make it visible again
    public void Reactivate()
    {
        if (checkpointCollider != null) checkpointCollider.enabled = true;
        if (spriteRenderer != null) spriteRenderer.enabled = true;
        activated = false;
    }
}
