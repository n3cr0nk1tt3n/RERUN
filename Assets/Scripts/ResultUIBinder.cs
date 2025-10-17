using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ResultUIBinder : MonoBehaviour
{
    [Header("Bind these scene UI objects")]
    public GameObject resultBackground;      // background panel/image (starts disabled)
    public GameObject resultPanel;           // container panel (starts disabled)
    public TextMeshProUGUI resultTMP;        // TMP text object (starts disabled)
    public Button playAgainButton;           // button (starts disabled)

    void Awake()
    {
        if (GameManager.Instance == null)
        {
            Debug.LogError("[ResultUIBinder] No GameManager in scene! Add one in the boot scene.");
            return;
        }

        GameManager.Instance.BindResultUI(resultBackground, resultPanel, resultTMP, playAgainButton);
    }
}