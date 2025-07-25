using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class StartGameDialogController : MonoBehaviour
{
    [Header("Panel")]
    public GameObject startGamePanel;

    [Header("Buttons")]
    public Button btnBuildMap;
    public Button btnChooseMap;
    public Button btnClose;

    void Start()
    {
        // 一开始隐藏弹窗
        startGamePanel.SetActive(false);
    }

    void OnEnable()
    {
        // 确保每次激活对象时重新绑定监听（避免因 inactive 导致的 null）
        if (btnBuildMap != null)
            btnBuildMap.onClick.AddListener(OnBuildMapClicked);
        if (btnChooseMap != null)
            btnChooseMap.onClick.AddListener(OnChooseMapClicked);
        if (btnClose != null)
            btnClose.onClick.AddListener(HidePanel);
    }

    public void ShowPanel()
    {
        if (startGamePanel != null)
            startGamePanel.SetActive(true);
    }

    public void HidePanel()
    {
        if (startGamePanel != null)
            startGamePanel.SetActive(false);
    }

    void OnBuildMapClicked()
    {
        Debug.Log("Build your own map clicked!");
        // SceneManager.LoadScene("DrawingScene");
    }

    void OnChooseMapClicked()
    {
        Debug.Log("Choose pre-designed map clicked!");
        // SceneManager.LoadScene("SelectMapScene");
    }
}
