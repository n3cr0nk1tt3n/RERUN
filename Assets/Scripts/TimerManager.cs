using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using System;

public class TimerManager : MonoBehaviour
{
    public static TimerManager Instance { get; private set; }

    [Header("Default Allotment (used by Past1)")]
    [Tooltip("The starting countdown shown in Past1 (and any scene with no pending allotment).")]
    [Min(1f)] public float startSeconds = 60f;

    [Header("UI (optional)")]
    [Tooltip("TMP text showing the current countdown.")]
    public TextMeshProUGUI timerText;
    [Tooltip("TMP text showing the target/shadow time for THIS level (previous run’s time).")]
    public TextMeshProUGUI targetText;

    [Header("Behavior")]
    [Tooltip("Auto-start the timer on every scene load.")]
    public bool autoStartOnSceneLoaded = true;

    // ---- Runtime state ----
    public bool IsRunning { get; private set; }
    public float TimeRemaining { get; private set; }
    public float ElapsedThisRun { get; private set; }

    // The previous run’s elapsed time you need to beat in THIS scene.
    private float targetToBeatSeconds = -1f;      // -1 => none
    public bool HasTargetToBeat => targetToBeatSeconds >= 0f;
    public float TargetToBeatSeconds => targetToBeatSeconds;

    // Baseline start captured from the FIRST scene you load with this manager alive (your Past1).
    private float baselineStartSeconds = -1f;

    // The starting allotment to use for the NEXT scene load (baseline - lastElapsed).
    private float pendingNextAllotment = -1f;     // -1 => none pending

    public event Action OnTimerExpired;

    // -------------------- Lifecycle --------------------
    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        SceneManager.sceneLoaded += HandleSceneLoaded;
    }

    private void OnDestroy()
    {
        if (Instance == this)
            SceneManager.sceneLoaded -= HandleSceneLoaded;
    }

    private void HandleSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Capture the baseline from the FIRST scene that loads (your Past1).
        if (baselineStartSeconds < 0f)
            baselineStartSeconds = Mathf.Max(0.001f, startSeconds);

        // Choose starting allotment for THIS scene:
        // - If we prepared a pending allotment (baseline - lastElapsed), use it.
        // - Otherwise, use the scene’s configured startSeconds (e.g., Past1).
        float startingAllotment = (pendingNextAllotment >= 0f)
            ? Mathf.Max(0.001f, pendingNextAllotment)
            : Mathf.Max(0.001f, startSeconds);

        // Consume the pending allotment (it applies only once).
        pendingNextAllotment = -1f;

        StartNewRun(startingAllotment, autoStartOnSceneLoaded);
        UpdateTargetUI();
    }

    // -------------------- Public API --------------------

    /// <summary>Rebind scene TMPs and immediately refresh text.</summary>
    public void RebindUI(TextMeshProUGUI timer, TextMeshProUGUI target = null)
    {
        timerText = timer;
        targetText = target;
        UpdateTimerUI();
        UpdateTargetUI();
    }

    /// <summary>Begin a new run with a specific allotment.</summary>
    public void StartNewRun(float allotmentSeconds, bool startImmediately)
    {
        TimeRemaining  = Mathf.Max(0.001f, allotmentSeconds);
        ElapsedThisRun = 0f;
        IsRunning      = startImmediately;
        UpdateTimerUI();
    }

    public void PauseTimer()  => IsRunning = false;
    public void ResumeTimer() { if (TimeRemaining > 0f) IsRunning = true; }

    /// <summary>Stop counting (keeps ElapsedThisRun as final).</summary>
    public void StopTimer()
    {
        IsRunning = false;
        if (ElapsedThisRun < 0f) ElapsedThisRun = 0f;
        UpdateTimerUI();
    }

    /// <summary>
    /// Call at end of level. Prepares next scene to:
    ///  - start at (baselineStartSeconds - this ElapsedThisRun),
    ///  - show this Elapsed as the target/shadow to beat.
    /// </summary>
    public void FinalizeRunAndPrepareNext()
    {
        StopTimer();

        // The shadow/target for the NEXT scene is this run’s elapsed.
        targetToBeatSeconds = ElapsedThisRun;

        // Next scene’s starting countdown = baselineStart - this elapsed.
        float remainingFromBaseline = (baselineStartSeconds >= 0f ? baselineStartSeconds : startSeconds) - ElapsedThisRun;
        pendingNextAllotment = Mathf.Max(0.001f, remainingFromBaseline);

        UpdateTargetUI();
    }

    /// <summary>Clear any target/shadow requirement (useful to reset flow).</summary>
    public void ClearTargetRequirement()
    {
        targetToBeatSeconds = -1f;
        UpdateTargetUI();
    }

    /// <summary>True if current run is strictly faster than the shadow/target.</summary>
    public bool DidBeatTargetThisLevel()
    {
        if (!HasTargetToBeat) return true; // No target means auto-pass
        return ElapsedThisRun < targetToBeatSeconds;
    }

    /// <summary>MM:SS.mmm formatting.</summary>
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
    private void Update()
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

    private void UpdateTimerUI()
    {
        if (timerText != null)
            timerText.text = FormatTime(TimeRemaining);
    }

    private void UpdateTargetUI()
    {
        if (targetText == null) return;

        if (HasTargetToBeat)
            targetText.text = $"Target: {FormatTime(targetToBeatSeconds)}";
        else
            targetText.text = string.Empty;
    }
    
    // Add inside TimerManager class
    public void ResetCycle()
    {
        StopTimer();
        // Clear target/shadow and any prepared allotment so we start fresh on startSceneName
        var field = typeof(TimerManager).GetField("targetToBeatSeconds", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        if (field != null) field.SetValue(this, -1f);

        field = typeof(TimerManager).GetField("pendingNextAllotment", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        if (field != null) field.SetValue(this, -1f);

        field = typeof(TimerManager).GetField("baselineStartSeconds", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        if (field != null) field.SetValue(this, -1f);
    }

}
