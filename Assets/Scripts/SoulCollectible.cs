using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class SoulCollectible : MonoBehaviour
{
    [Min(1)]
    public int soulValue = 1;

    [Header("FX (optional)")]
    public AudioSource audioSource; // assign if you want sound
    public bool destroyOnCollect = true;

    bool _collected;

    void Reset()
    {
        // Ensure collider is trigger by default
        var col = GetComponent<Collider2D>();
        if (col) col.isTrigger = true;
    }

    void Awake()
    {
        // Allow auto-find an AudioSource on this object
        if (audioSource == null) audioSource = GetComponent<AudioSource>();
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (_collected) return;
        if (!other.CompareTag("Player")) return;

        _collected = true;

        // Increment souls
        if (SoulsManager.Instance != null)
            SoulsManager.Instance.AddSouls(soulValue);
        else
            Debug.LogWarning("[SoulCollectible] SoulsManager.Instance is null. Add SoulsManager to the scene.");

        // Optional: play audio and hide visuals before destroy
        var sr = GetComponent<SpriteRenderer>(); // in case you used a SpriteRenderer for the pickup
        if (sr) sr.enabled = false;

        var col = GetComponent<Collider2D>();
        if (col) col.enabled = false;

        if (audioSource != null && audioSource.clip != null)
        {
            audioSource.Play();
            if (destroyOnCollect)
                Destroy(gameObject, audioSource.clip.length);
        }
        else if (destroyOnCollect)
        {
            Destroy(gameObject);
        }
    }
}