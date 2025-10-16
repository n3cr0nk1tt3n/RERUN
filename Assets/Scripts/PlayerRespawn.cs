using UnityEngine;

public class PlayerRespawn : MonoBehaviour
{
    [Header("Player Settings")]
    public int maxHealth = 3;
    public int currentHealth;

    [Header("Respawn Settings")]
    private Vector2 defaultStartPoint;
    private Vector2 respawnPoint;
    private bool hasReachedCheckpoint = false;

    [Header("Audio Settings")]
    public AudioSource audioSource;
    public AudioClip respawnClip;

    private void Start()
    {
        currentHealth = maxHealth;

        // Set default starting point from where player begins in the scene
        defaultStartPoint = transform.position;

        // Initial respawn point is also the start
        respawnPoint = defaultStartPoint;

        // Try to find an AudioSource if not manually assigned
        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
        }
    }

    /// <summary>
    /// Call this to update the player's checkpoint position.
    /// </summary>
    public void UpdateCheckpoint(Vector2 newCheckpoint)
    {
        respawnPoint = newCheckpoint;
        hasReachedCheckpoint = true;
    }

    /// <summary>
    /// Call this to apply damage.
    /// </summary>
    public void TakeDamage(int amount)
    {
        currentHealth -= amount;

        if (currentHealth <= 0)
        {
            DieAndRespawn();
        }
    }

    /// <summary>
    /// Respawn the player at the last checkpoint or start if none reached.
    /// </summary>
    public void DieAndRespawn()
    {
        Debug.Log("Player died. Respawning...");

        // ✅ Play respawn audio if assigned
        if (audioSource != null && respawnClip != null)
        {
            audioSource.PlayOneShot(respawnClip);
        }

        Vector2 spawnPosition = hasReachedCheckpoint ? respawnPoint : defaultStartPoint;
        transform.position = spawnPosition;

        currentHealth = maxHealth;

        // You can add visual effects or animations here too
    }
}
