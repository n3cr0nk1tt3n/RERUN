using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class GameSceneManager : MonoBehaviour
{
    public static GameSceneManager Instance { get; private set; }

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    /// <summary>
    /// Loads a scene by name after a real-time delay.
    /// If prepareShadowAllotment is true, calls TimerManager.FinalizeRunAndPrepareNext()
    /// BEFORE waiting, so the next scene starts with (baseline - lastElapsed) and uses lastElapsed as the target.
    /// </summary>
    public void LoadSceneAfter(string sceneName, float delaySeconds, bool prepareShadowAllotment = false)
    {
        StartCoroutine(LoadSceneAfter_Coroutine(sceneName, delaySeconds, prepareShadowAllotment));
    }

    private IEnumerator LoadSceneAfter_Coroutine(string sceneName, float delaySeconds, bool prepareShadowAllotment)
    {
        if (prepareShadowAllotment && TimerManager.Instance != null)
        {
            // Store last run's elapsed as target and compute next allotment = baseline - elapsed
            TimerManager.Instance.FinalizeRunAndPrepareNext();
        }

        yield return new WaitForSecondsRealtime(Mathf.Max(0f, delaySeconds));

        SceneManager.LoadScene(sceneName, LoadSceneMode.Single);
    }
}