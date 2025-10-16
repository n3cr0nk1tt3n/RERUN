using UnityEngine;

public class CurrencyCollectible : MonoBehaviour
{
    [Tooltip("Set this to 'Coin', 'Gem', or 'Rupee'")]
    public string currencyType = "Coin";

    private AudioSource audioSource;
    private bool isCollected = false;

    private void Awake()
    {
        // Get the AudioSource component attached to the coin
        audioSource = GetComponent<AudioSource>();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!isCollected && collision.CompareTag("Player"))
        {
            isCollected = true;

            // Tell the CurrencyManager to add currency
            CurrencyManager.Instance.CollectCurrency(currencyType);

            // Play the audio
            if (audioSource != null && audioSource.clip != null)
            {
                audioSource.Play();

                // Disable visuals and collider so it doesn't interfere
                GetComponent<SpriteRenderer>().enabled = false;
                GetComponent<Collider2D>().enabled = false;

                // Destroy after sound finishes playing
                Destroy(gameObject, audioSource.clip.length);
            }
            else
            {
                // No audio source or clip, just destroy immediately
                Destroy(gameObject);
            }
        }
    }
}
