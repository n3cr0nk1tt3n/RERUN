using UnityEngine;
using TMPro; // <-- Make sure to include this

public class CurrencyManager : MonoBehaviour
{
    public static CurrencyManager Instance;

    [Header("Currency Count")]
    public int coinCount = 0;
    public int gemCount = 0;
    public int rupeeCount = 0;

    [Header("UI Elements")]
    public TextMeshProUGUI coinCounterText;

    private void Awake()
    {
        // Singleton setup
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // Optional
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        UpdateUI();
    }

    public void CollectCurrency(string currencyType)
    {
        switch (currencyType)
        {
            case "Coin":
                coinCount++;
                UpdateUI();
                break;
            case "Gem":
                gemCount++;
                // Add gem UI update here later
                break;
            case "Rupee":
                rupeeCount++;
                // Add rupee UI update here later
                break;
            default:
                Debug.LogWarning("Unknown currency type: " + currencyType);
                break;
        }
    }

    private void UpdateUI()
    {
        if (coinCounterText != null)
        {
            coinCounterText.text = "Coins: " + coinCount;
        }
    }
}
