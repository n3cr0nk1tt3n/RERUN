using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class ResultUIBinder : MonoBehaviour
{
    [Header("Bind these scene UI objects")]
    public GameObject resultBackground;  // inactive at start
    public GameObject resultPanel;       // inactive at start
    public TextMeshProUGUI resultTMP;    // inactive at start
    public Button playAgainButton;       // inactive at start

    void Awake()
    {
        // You can still hide here to ensure a clean start
        if (resultBackground) resultBackground.SetActive(false);
        if (resultPanel)      resultPanel.SetActive(false);
        if (resultTMP)        resultTMP.gameObject.SetActive(false);
        if (playAgainButton)  playAgainButton.gameObject.SetActive(false);

        StartCoroutine(WaitAndBind());
    }

    IEnumerator WaitAndBind()
    {
        // wait up to ~1 second for GameManager to appear (handles late init or loading in mid-level)
        float deadline = Time.realtimeSinceStartup + 1f;
        while (GameManager.Instance == null && Time.realtimeSinceStartup < deadline)
            yield return null;

        if (GameManager.Instance == null)
        {
            Debug.LogError("[ResultUIBinder] No GameManager found after waiting. " +
                           "Make sure a GameManager exists (DontDestroyOnLoad) or add a bootstrapper.");
            yield break;
        }

        GameManager.Instance.BindResultUI(resultBackground, resultPanel, resultTMP, playAgainButton);
    }
}