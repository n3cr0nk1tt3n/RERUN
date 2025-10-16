using UnityEngine;
using TMPro;
using System.Collections;
using UnityEngine.SceneManagement; // For scene reload

public class LevelCompleteTrigger : MonoBehaviour
{
    [Header("UI Elements")]
    [Tooltip("Drag the 'Level Complete' TMP GameObject here.")]
    public GameObject levelCompleteText;

    [Tooltip("Drag the background GameObject here (e.g. a panel or image).")]
    public GameObject background;

    [Header("Audio Settings")]
    public AudioSource audioSource;
    public AudioClip levelCompleteClip;

    private bool levelEnded = false;

    private void Start()
    {
        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (levelEnded) return;

        if (collision.CompareTag("Player"))
        {
            levelEnded = true;
            StartCoroutine(HandleLevelComplete());
        }
    }

    private IEnumerator HandleLevelComplete()
    {
        // Play sound
        if (audioSource != null && levelCompleteClip != null)
        {
            audioSource.PlayOneShot(levelCompleteClip);
            yield return new WaitForSecondsRealtime(levelCompleteClip.length);
        }

        // Freeze time
        Time.timeScale = 0f;

        // Show UI
        if (levelCompleteText != null)
            levelCompleteText.SetActive(true);
        else
            Debug.LogWarning("LevelCompleteText is not assigned.");

        if (background != null)
            background.SetActive(true);
        else
            Debug.LogWarning("Background GameObject is not assigned.");

        Debug.Log("Level Complete!");

        // Wait 10 seconds real-time (ignoring timeScale)
        yield return new WaitForSecondsRealtime(10f);

        // Unfreeze time
        Time.timeScale = 1f;

        // Reload current scene to reset everything
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
