using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Start Scene")]
    [Tooltip("The scene to load when the player clicks PLAY AGAIN?")]
    public string startSceneName = "Past1";

    // Scene-bound UI (bound each scene via ResultUIBinder)
    private GameObject resultBackground;
    private GameObject resultPanel;
    private TextMeshProUGUI resultTMP;
    private Button playAgainButton;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        // Subscribe to timer expiry -> YOU LOSE
        if (TimerManager.Instance != null)
            TimerManager.Instance.OnTimerExpired += HandleTimerExpired;

        // Ensure we re-bind OnTimerExpired if TimerManager is recreated (unlikely)
        SceneManager.sceneLoaded += (_, __) =>
        {
            if (TimerManager.Instance != null)
            {
                // Clear double-subscribe
                TimerManager.Instance.OnTimerExpired -= HandleTimerExpired;
                TimerManager.Instance.OnTimerExpired += HandleTimerExpired;
            }
        };
    }

    // Called by ResultUIBinder in each scene
    public void BindResultUI(GameObject background, GameObject panel, TextMeshProUGUI tmp, Button againBtn)
    {
        resultBackground = background;
        resultPanel      = panel;
        resultTMP        = tmp;
        playAgainButton  = againBtn;

        // Make sure they're hidden at scene start
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

    // === Events ===
    private void HandleTimerExpired()
    {
        ShowLoseUI();
    }

    // === Public API (call from gameplay to end the game with a win) ===
    public void ShowWinUI()
    {
        ShowResultUI("<b>YOU WIN!</b>\nPlay again?");
    }

    public void ShowLoseUI()
    {
        ShowResultUI("<b>YOU LOSE</b>\nPlay again?");
    }

    // === Internals ===
    private void ShowResultUI(string message)
    {
        // Pause gameplay if you want: (we'll just stop timer; no timeScale change needed)
        if (TimerManager.Instance != null) TimerManager.Instance.StopTimer();

        if (resultBackground) resultBackground.SetActive(true);
        if (resultPanel)      resultPanel.SetActive(true);
        if (resultTMP)
        {
            resultTMP.gameObject.SetActive(true);
            resultTMP.text = message;
        }
        if (playAgainButton)  playAgainButton.gameObject.SetActive(true);
    }

    private void OnPlayAgainClicked()
    {
        // Reset timer cycle and go back to start scene
        if (TimerManager.Instance != null)
            TimerManager.Instance.ResetCycle();   // helper added below

        SceneManager.LoadScene(startSceneName, LoadSceneMode.Single);
    }
}
