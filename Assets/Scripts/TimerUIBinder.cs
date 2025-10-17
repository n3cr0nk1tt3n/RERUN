using UnityEngine;
using TMPro;

public class TimerUIBinder : MonoBehaviour
{
    [Header("Assign your scene UI")]
    public TextMeshProUGUI timerTMP;
    public TextMeshProUGUI targetTMP; // optional (can be null)

    void Awake()
    {
        var tm = TimerManager.Instance;
        if (tm == null)
        {
            Debug.LogError("[TimerUIBinder] TimerManager.Instance is null. Ensure a TimerManager exists in the boot scene and is DontDestroyOnLoad.");
            return;
        }

        // Bind and refresh instantly (grabs the new allotment set for this scene)
        tm.RebindUI(timerTMP, targetTMP);
    }
}