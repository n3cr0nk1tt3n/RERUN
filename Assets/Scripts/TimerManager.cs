using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using System;

public class TimerManager : MonoBehaviour
{
    public static TimerManager Instance { get; private set; }

    [Header("Default Allotment (first/Past scene)")]
    [Tooltip("Baseline starting countdown for the first run (e.g., Past1).")]
    [Min(1f)] public float startSeconds = 60f;

    [Header("UI (optional)")]
    [Tooltip("TMP text showing the current countdown.")]
    public TextMeshProUGUI timerText;
    [Tooltip("TMP text showing the shadow/target time for THIS level.")]
    public TextMeshProUGUI targetText;

    [Header("Behavior")]
    [Tooltip("Auto-start the timer on every scene load.")]
    public bool autoStartOnSceneLoaded = true;

    // ---- Runtime state ----
    public bool IsRunning { get; private set; }
    public float TimeRemaining { get; private set; }
    public float ElapsedThisRun { get; private set; }

    // Shadow target to beat in THIS scene (set from prior scene’s elapsed)
    float targetToBeatSeconds = -1f;    // -1 => none
    public bool HasTargetToBeat => targetToBeatSeconds >= 0f;
    public float TargetToBeatSeconds => targetToBeatSeconds;

    // Race-your-shadow pipeline
    float baselineStartSeconds = -1f;   // captured from the very first scene this manager sees
    float pendingNextAllotment = -1f;   // next scene starts with this (baseline - lastElapsed)

    public event Action OnTimerExpired;

    // -------------------- Lifecycle --------------------
    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        SceneManager.sceneLoaded += HandleSceneLoaded;
    }

    void OnDestroy()
    {
        if (Instance == this)
            SceneManager.sceneLoaded -= HandleSceneLoaded;
    }

    void HandleSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Capture baseline once (first scene that loads with this alive)
        if (baselineStartSeconds < 0f)
            baselineStartSeconds = Mathf.Max(0.001f, startSeconds);

        // Decide the starting allotment for THIS scene
        float startingAllotment = (pendingNextAllotment >= 0f)
            ? Mathf.Max(0.001f, pendingNextAllotment)
            : Mathf.Max(0.001f, startSeconds);

        // Consume pending (one-shot)
        pendingNextAllotment = -1f;

        StartNewRun(startingAllotment, autoStartOnSceneLoaded);
        UpdateTargetUI(); // reflect any shadow target for this scene
    }

    // -------------------- Public API --------------------
    /// <summary>Bind scene TMPs and push the current values immediately.</summary>
    public void RebindUI(TextMeshProUGUI timer, TextMeshProUGUI target = null)
    {
        timerText = timer;
        targetText = target;
        UpdateTimerUI();
        UpdateTargetUI();
    }

    /// <summary>Start a new run for the active scene.</summary>
    public void StartNewRun(float allotmentSeconds, bool startImmediately)
    {
        TimeRemaining  = Mathf.Max(0.001f, allotmentSeconds);
        ElapsedThisRun = 0f;
        IsRunning      = startImmediately;
        UpdateTimerUI();
    }

    public void PauseTimer()  => IsRunning = false;
    public void ResumeTimer() { if (TimeRemaining > 0f) IsRunning = true; }

    /// <summary>Stop the countdown and keep current elapsed as final.</summary>
    public void StopTimer()
    {
        IsRunning = false;
        if (ElapsedThisRun < 0f) ElapsedThisRun = 0f;
        UpdateTimerUI();
    }

    /// <summary>
    /// Call at end of level. Prepares the next scene to:
    ///  - start at (baselineStartSeconds - this ElapsedThisRun),
    ///  - use this Elapsed as the target/shadow to beat.
    /// </summary>
    public void FinalizeRunAndPrepareNext()
    {
        StopTimer();

        // Next scene must beat this
        targetToBeatSeconds = ElapsedThisRun;

        // Next scene’s countdown = baseline - this run
        float baseStart = (baselineStartSeconds >= 0f ? baselineStartSeconds : startSeconds);
        float remainingFromBaseline = baseStart - ElapsedThisRun;
        pendingNextAllotment = Mathf.Max(0.001f, remainingFromBaseline);

        UpdateTargetUI();
    }

    /// <summary>Clear shadow requirement (fresh level).</summary>
    public void ClearTargetRequirement()
    {
        targetToBeatSeconds = -1f;
        UpdateTargetUI();
    }

    /// <summary>True if current run’s elapsed is strictly faster than shadow/target.</summary>
    public bool DidBeatTargetThisLevel()
    {
        if (!HasTargetToBeat) return true;
        return ElapsedThisRun < targetToBeatSeconds;
    }

    /// <summary>Reset all persistent timer/shadow state for a clean restart (used by PLAY AGAIN?).</summary>
    public void ResetCycle()
    {
        StopTimer();
        TimeRemaining  = 0f;
        ElapsedThisRun = 0f;
        IsRunning      = false;

        targetToBeatSeconds  = -1f;
        pendingNextAllotment = -1f;
        baselineStartSeconds = -1f;  // recapture on next scene load

        UpdateTimerUI();
        UpdateTargetUI();
    }

    /// <summary>MM:SS.mmm</summary>
    public static string FormatTime(float seconds)
    {
        if (seconds < 0f) seconds = 0f;
        int whole  = Mathf.FloorToInt(seconds);
        int mins   = whole / 60;
        int secs   = whole % 60;
        int millis = Mathf.FloorToInt((seconds - whole) * 1000f);
        return $"{mins:00}:{secs:00}.{millis:000}";
    }

    // -------------------- Update & UI --------------------
    void Update()
    {
        if (!IsRunning) return;

        float dt = Time.deltaTime;
        ElapsedThisRun += dt;
        TimeRemaining  -= dt;

        if (TimeRemaining <= 0f)
        {
            TimeRemaining = 0f;
            IsRunning = false;
            UpdateTimerUI();
            OnTimerExpired?.Invoke();
            return;
        }

        UpdateTimerUI();
    }

    void UpdateTimerUI()
    {
        if (timerText != null)
            timerText.text = FormatTime(TimeRemaining);
    }

    void UpdateTargetUI()
    {
        if (targetText == null) return;

        if (HasTargetToBeat)
            targetText.text = $"Target: {FormatTime(targetToBeatSeconds)}";
        else
            targetText.text = string.Empty;
    }
}
