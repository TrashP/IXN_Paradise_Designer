using UnityEngine;
using UnityEngine.UI;

public class MainMenuController : MonoBehaviour
{
    [Header("Main Menu Panel")]
    public GameObject mainMenuPanel;

    [Header("Menu Buttons")]
    public Button btnStartGame;
    public Button btnLoadGame;
    public Button btnSettings;
    public Button btnExit;

    [Header("Sub Panels")]
    public GameObject startGamePanel;
    public GameObject loadGamePanel;
    public GameObject settingsPanel;

    [Header("Back Buttons")]
    public Button btnStartGameBack;
    public Button btnLoadGameBack;
    public Button btnSettingsBack;

    void Start()
    {
        // 初始化：显示主菜单，隐藏所有子面板
        InitializePanels();

        // 设置按钮监听器
        SetupButtonListeners();
    }

    private void InitializePanels()
    {
        // 确保主菜单面板显示
        if (mainMenuPanel != null)
            mainMenuPanel.SetActive(true);

        // 隐藏所有子面板
        if (startGamePanel != null)
            startGamePanel.SetActive(false);
        if (loadGamePanel != null)
            loadGamePanel.SetActive(false);
        if (settingsPanel != null)
            settingsPanel.SetActive(false);
    }

    private void SetupButtonListeners()
    {
        // 主菜单按钮
        if (btnStartGame != null)
            btnStartGame.onClick.AddListener(OnStartGameClicked);
        if (btnLoadGame != null)
            btnLoadGame.onClick.AddListener(OnLoadGameClicked);
        if (btnSettings != null)
            btnSettings.onClick.AddListener(OnSettingsClicked);
        if (btnExit != null)
            btnExit.onClick.AddListener(OnExitClicked);

        // 返回按钮
        if (btnStartGameBack != null)
            btnStartGameBack.onClick.AddListener(OnStartGameBackClicked);
        if (btnLoadGameBack != null)
            btnLoadGameBack.onClick.AddListener(OnLoadGameBackClicked);
        if (btnSettingsBack != null)
            btnSettingsBack.onClick.AddListener(OnSettingsBackClicked);
    }

    // 主菜单按钮事件
    private void OnStartGameClicked()
    {
        ShowStartGamePanel();
    }

    private void OnLoadGameClicked()
    {
        ShowLoadGamePanel();
    }

    private void OnSettingsClicked()
    {
        ShowSettingsPanel();
    }

    private void OnExitClicked()
    {
        QuitGame();
    }

    // 显示面板函数
    private void ShowStartGamePanel()
    {
        HideAllPanels();
        if (startGamePanel != null)
            startGamePanel.SetActive(true);
    }

    private void ShowLoadGamePanel()
    {
        HideAllPanels();
        // 查找并打开加载游戏对话框
        LoadGameDialogController loadGameDialog = FindFirstObjectByType<LoadGameDialogController>();
        if (loadGameDialog != null)
        {
            loadGameDialog.ShowPanel();
            Debug.Log("打开加载游戏对话框");
        }
        else
        {
            Debug.LogWarning("未找到LoadGameDialogController组件");
        }
    }

    private void ShowSettingsPanel()
    {
        HideAllPanels();
        if (settingsPanel != null)
            settingsPanel.SetActive(true);
    }

    // 返回按钮事件
    private void OnStartGameBackClicked()
    {
        ShowMainMenu();
    }

    private void OnLoadGameBackClicked()
    {
        ShowMainMenu();
    }

    private void OnSettingsBackClicked()
    {
        ShowMainMenu();
    }

    // 显示主菜单
    private void ShowMainMenu()
    {
        HideAllPanels();
        if (mainMenuPanel != null)
            mainMenuPanel.SetActive(true);
    }

    // 隐藏所有面板
    private void HideAllPanels()
    {
        if (mainMenuPanel != null)
            mainMenuPanel.SetActive(false);
        if (startGamePanel != null)
            startGamePanel.SetActive(false);
        if (loadGamePanel != null)
            loadGamePanel.SetActive(false);
        if (settingsPanel != null)
            settingsPanel.SetActive(false);
    }

    // 退出游戏
    private void QuitGame()
    {
        Debug.Log("退出游戏");
        
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            Application.Quit();
        #endif
    }

    // 公共方法：显示主菜单（供外部调用）
    public void ShowMainMenuPublic()
    {
        ShowMainMenu();
    }
} 