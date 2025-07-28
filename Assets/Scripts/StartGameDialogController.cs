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
        startGamePanel.SetActive(false);

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
        LoadColorPickerScene();
    }

    void OnChooseMapClicked()
    {
        Debug.Log("Choose pre-designed map clicked!");
        LoadPreDesignedScene();
    }

    public void LoadColorPickerScene()
    {
        SceneManager.LoadScene("ColorPickerScene");
    }

    public void LoadPreDesignedScene()
    {
        SceneManager.LoadScene("Dev");
    }
}
