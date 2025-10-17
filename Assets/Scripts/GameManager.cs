using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Start Scene")]
    [Tooltip("Scene to load when the player clicks PLAY AGAIN?")]
    public string startSceneName = "Past1";

    // Scene-bound UI (provided by ResultUIBinder per scene)
    GameObject resultBackground;
    GameObject resultPanel;
    TextMeshProUGUI resultTMP;
    Button playAgainButton;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        // Hide result UI whenever a new scene loads (binder will rebind the refs)
        SceneManager.sceneLoaded += OnSceneLoadedRebind;

        // Re-hook timer expiry on each scene load
        SceneManager.sceneLoaded += (_, __) =>
        {
            if (TimerManager.Instance != null)
            {
                TimerManager.Instance.OnTimerExpired -= HandleTimerExpired;
                TimerManager.Instance.OnTimerExpired += HandleTimerExpired;
            }
        };
    }

    void OnDestroy()
    {
        if (Instance == this)
            SceneManager.sceneLoaded -= OnSceneLoadedRebind;
    }

    void OnSceneLoadedRebind(Scene scene, LoadSceneMode mode)
    {
        // If these are already bound for this scene, ensure they start hidden
        if (resultBackground) resultBackground.SetActive(false);
        if (resultPanel)      resultPanel.SetActive(false);
        if (resultTMP)        resultTMP.gameObject.SetActive(false);
        if (playAgainButton)  playAgainButton.gameObject.SetActive(false);
    }

    /// <summary>Called by ResultUIBinder in each scene to provide scene UI references.</summary>
    public void BindResultUI(GameObject background, GameObject panel, TextMeshProUGUI tmp, Button againBtn)
    {
        resultBackground = background;
        resultPanel      = panel;
        resultTMP        = tmp;
        playAgainButton  = againBtn;

        // Ensure clean state
        if (resultBackground) resultBackground.SetActive(false);
        if (resultPanel)      resultPanel.SetActive(false);
        if (resultTMP)        resultTMP.gameObject.SetActive(false);

        if (playAgainButton)
        {
            playAgainButton.gameObject.SetActive(false);
            playAgainButton.onClick.RemoveAllListeners();
            playAgainButton.onClick.AddListener(OnPlayAgainClicked);
        }
    }

    // === Timer event ===
    void HandleTimerExpired()
    {
        ShowLoseUI();
    }

    // === Public controls ===
    public void ShowWinUI()
    {
        ShowResultUI("<b>YOU WIN!</b>\nPlay again?");
    }

    public void ShowLoseUI()
    {
        ShowResultUI("<b>YOU LOSE</b>\nPlay again?");
    }

    // === Internals ===
    void ShowResultUI(string message)
    {
        // Make sure gameplay is unpaused and timer is stopped
        Time.timeScale = 1f;
        if (TimerManager.Instance != null) TimerManager.Instance.StopTimer();

        if (resultBackground == null || resultPanel == null || resultTMP == null || playAgainButton == null)
            Debug.LogWarning("[GameManager] Result UI not bound in this scene (ResultUIBinder missing/incorrect).");

        if (resultBackground) resultBackground.SetActive(true);
        if (resultPanel)      resultPanel.SetActive(true);

        if (resultTMP)
        {
            resultTMP.gameObject.SetActive(true);
            resultTMP.text = message;
        }

        if (playAgainButton)  playAgainButton.gameObject.SetActive(true);
    }

    void OnPlayAgainClicked()
    {
        // 1) Restore timescale in case anything paused it
        Time.timeScale = 1f;

        // 2) Reset persistent gameplay state
        if (TimerManager.Instance != null)
            TimerManager.Instance.ResetCycle();

        // (Optional) Reset other singletons here, e.g. SoulsManager:
        // SoulsManager.Instance?.ResetSouls(0);

        // 3) Hide current result UI immediately (polish)
        if (resultBackground) resultBackground.SetActive(false);
        if (resultPanel)      resultPanel.SetActive(false);
        if (resultTMP)        resultTMP.gameObject.SetActive(false);
        if (playAgainButton)  playAgainButton.gameObject.SetActive(false);

        // 4) Load the start scene fresh
        SceneManager.LoadScene(startSceneName, LoadSceneMode.Single);
    }
}
