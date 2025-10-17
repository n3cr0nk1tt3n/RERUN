using UnityEngine;
using TMPro;
using System.Collections;

[RequireComponent(typeof(Collider2D))]
public class LevelCompleteTrigger : MonoBehaviour
{
    [Header("UI Elements")]
    public GameObject levelCompletePanel;
    public GameObject background;
    public TextMeshProUGUI levelCompleteTMP;

    [Header("Audio Settings")]
    public AudioSource audioSource;
    public AudioClip levelCompleteClip;

    [Header("Flow")]
    public bool isFinalLevel = false;                 // <-- set true in Future1
    public float successBreakSeconds = 5f;
    public string nextSceneName = "Future1";          // ignored if isFinalLevel = true

    bool levelEnded = false;

    void Reset()
    {
        var col = GetComponent<Collider2D>();
        if (col) col.isTrigger = true;
    }

    void Start()
    {
        if (audioSource == null) audioSource = GetComponent<AudioSource>();
        if (levelCompletePanel) levelCompletePanel.SetActive(false);
        if (background)         background.SetActive(false);
        if (levelCompleteTMP)   levelCompleteTMP.gameObject.SetActive(false);
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (levelEnded) return;
        if (!collision.CompareTag("Player")) return;

        levelEnded = true;
        StartCoroutine(HandleLevelComplete());
    }

    IEnumerator HandleLevelComplete()
    {
        var tm = TimerManager.Instance;
        if (tm != null) tm.StopTimer();

        if (background)       background.SetActive(true);
        if (levelCompletePanel) levelCompletePanel.SetActive(true);
        if (levelCompleteTMP) levelCompleteTMP.gameObject.SetActive(true);

        if (levelCompleteTMP && tm)
        {
            string run = TimerManager.FormatTime(tm.ElapsedThisRun);
            levelCompleteTMP.text = $"<b>Level Complete!</b>\nYour Time: {run}";
        }

        if (audioSource != null && levelCompleteClip != null)
            audioSource.PlayOneShot(levelCompleteClip);

        yield return new WaitForSecondsRealtime(successBreakSeconds);

        if (isFinalLevel)
        {
            // Show YOU WIN and stop; player decides to Play Again?
            if (GameManager.Instance != null)
                GameManager.Instance.ShowWinUI();
            yield break;
        }

        // Not final level: prepare next scene with shadow logic (baseline - elapsed)
        if (GameSceneManager.Instance != null && tm != null)
        {
            GameSceneManager.Instance.LoadSceneAfter(nextSceneName, 0f, prepareShadowAllotment: true);
        }
        else
        {
            if (tm != null) tm.FinalizeRunAndPrepareNext();
            UnityEngine.SceneManagement.SceneManager.LoadScene(nextSceneName);
        }
    }
}
