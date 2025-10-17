using UnityEngine;
using TMPro;

public class SoulsManager : MonoBehaviour
{
    public static SoulsManager Instance { get; private set; }

    [Header("Souls")]
    [Min(0)] public int currentSouls = 0;    // currently collected souls
    [Min(1)] public int maxSouls = 30;       // total collectible souls in this level

    [Header("UI")]
    public TextMeshProUGUI soulsText;        // assign in Inspector
    [Tooltip("How fast the UI count animates toward the target value (units per second).")]
    public float uiCountSpeed = 200f;

    // internal UI animation state
    int _uiDisplayedSouls = 0;
    bool _animating = false;

    void Awake()
    {
        // Singleton pattern
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    void Start()
    {
        // Initialize UI display
        _uiDisplayedSouls = Mathf.Max(0, currentSouls);
        UpdateUIText(_uiDisplayedSouls);
    }

    /// <summary>Add a positive or negative amount of souls (clamped at 0–maxSouls) and animate the UI.</summary>
    public void AddSouls(int amount)
    {
        int prev = currentSouls;
        currentSouls = Mathf.Clamp(currentSouls + amount, 0, maxSouls);
        if (currentSouls != prev) StartUIAnim();
    }

    /// <summary>Set the soul count directly and animate the UI.</summary>
    public void SetSouls(int value)
    {
        int clamped = Mathf.Clamp(value, 0, maxSouls);
        if (clamped == currentSouls) return;
        currentSouls = clamped;
        StartUIAnim();
    }

    void StartUIAnim()
    {
        if (!_animating) StartCoroutine(AnimateUICount());
    }

    System.Collections.IEnumerator AnimateUICount()
    {
        _animating = true;

        // Smoothly interpolate UI count toward currentSouls
        while (_uiDisplayedSouls != currentSouls)
        {
            float step = uiCountSpeed * Time.unscaledDeltaTime;

            if (Mathf.Abs(currentSouls - _uiDisplayedSouls) <= step)
            {
                _uiDisplayedSouls = currentSouls;
            }
            else
            {
                int dir = currentSouls > _uiDisplayedSouls ? 1 : -1;
                _uiDisplayedSouls += Mathf.Max(1, Mathf.FloorToInt(step)) * dir;

                if ((dir > 0 && _uiDisplayedSouls > currentSouls) ||
                    (dir < 0 && _uiDisplayedSouls < currentSouls))
                    _uiDisplayedSouls = currentSouls;
            }

            UpdateUIText(_uiDisplayedSouls);
            yield return null;
        }

        _animating = false;
    }

    void UpdateUIText(int value)
    {
        if (soulsText != null)
            soulsText.text = $"Souls: {value}/{maxSouls}";
    }
}
