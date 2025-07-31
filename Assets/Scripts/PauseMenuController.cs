using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class PauseMenuManager : MonoBehaviour
{
    [Header("Pause Menu Panel")]
    public GameObject pauseMenuPanel;

    [Header("Menu Buttons")]
    public Button btnContinue;
    public Button btnLoadGame;
    public Button btnSettings;
    public Button btnMainMenu;
    public Button btnQuit;

    [Header("Player Control")]
    public GameObject playerObject; // 玩家对象
    public Camera playerCamera; // 玩家相机

    [Header("Cursor Control")]
    public bool lockCursorOnResume = true; // 恢复游戏时是否锁定鼠标

    // 私有变量
    private bool isPaused = false;
    private CursorLockMode previousCursorLockMode;
    private bool previousCursorVisible;
    private MonoBehaviour[] playerControlScripts; // 存储玩家控制脚本

    void Start()
    {
        // 初始化暂停菜单
        if (pauseMenuPanel != null)
        {
            pauseMenuPanel.SetActive(false);
        }

        // 设置按钮监听器
        SetupButtonListeners();

        // 初始化玩家控制脚本
        InitializePlayerControls();

        // 设置初始鼠标状态
        SetCursorLocked(true);
    }

    void Update()
    {
        // 检测Tab键输入
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            TogglePauseMenu();
        }

        // 检测ESC键输入（备用暂停键）
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            TogglePauseMenu();
        }
    }

    private void SetupButtonListeners()
    {
        // 设置继续游戏按钮
        if (btnContinue != null)
            btnContinue.onClick.AddListener(OnContinueClicked);

        // 设置加载游戏按钮
        if (btnLoadGame != null)
            btnLoadGame.onClick.AddListener(OnLoadGameClicked);

        // 设置设置按钮
        if (btnSettings != null)
            btnSettings.onClick.AddListener(OnSettingsClicked);

        // 设置主菜单按钮
        if (btnMainMenu != null)
            btnMainMenu.onClick.AddListener(OnMainMenuClicked);

        // 设置退出按钮
        if (btnQuit != null)
            btnQuit.onClick.AddListener(OnQuitClicked);
    }

    private void InitializePlayerControls()
    {
        // 如果没有指定玩家对象，尝试查找
        if (playerObject == null)
        {
            playerObject = GameObject.FindGameObjectWithTag("Player");
        }

        // 如果没有指定相机，尝试查找主相机
        if (playerCamera == null)
        {
            playerCamera = Camera.main;
        }

        // 获取玩家控制脚本（常见的玩家控制脚本）
        if (playerObject != null)
        {
            playerControlScripts = playerObject.GetComponents<MonoBehaviour>();
        }
    }

    // 切换暂停菜单状态
    public void TogglePauseMenu()
    {
        if (isPaused)
        {
            ResumeGame();
        }
        else
        {
            PauseGame();
        }
    }

    // 暂停游戏
    public void PauseGame()
    {
        if (isPaused) return;

        isPaused = true;

        // 显示暂停菜单
        if (pauseMenuPanel != null)
        {
            pauseMenuPanel.SetActive(true);
        }

        // 不暂停时间，让世界继续运行
        // Time.timeScale = 0f; // 注释掉这行，让时间继续流动

        // 保存当前鼠标状态
        previousCursorLockMode = Cursor.lockState;
        previousCursorVisible = Cursor.visible;

        // 显示鼠标光标
        SetCursorLocked(false);

        // 禁用玩家控制
        DisablePlayerControls();

        Debug.Log("玩家操作已暂停，世界继续运行");
    }

    // 恢复游戏
    public void ResumeGame()
    {
        if (!isPaused) return;

        isPaused = false;

        // 隐藏暂停菜单
        if (pauseMenuPanel != null)
        {
            pauseMenuPanel.SetActive(false);
        }

        // 时间继续正常流动，不需要恢复
        // Time.timeScale = 1f; // 注释掉这行，因为时间从未被暂停

        // 恢复鼠标状态
        if (lockCursorOnResume)
        {
            SetCursorLocked(true);
        }
        else
        {
            Cursor.lockState = previousCursorLockMode;
            Cursor.visible = previousCursorVisible;
        }

        // 启用玩家控制
        EnablePlayerControls();

        Debug.Log("玩家操作已恢复");
    }

    // 设置鼠标锁定状态
    private void SetCursorLocked(bool locked)
    {
        if (locked)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
        else
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }

    // 禁用玩家控制
    private void DisablePlayerControls()
    {
        if (playerControlScripts != null)
        {
            foreach (MonoBehaviour script in playerControlScripts)
            {
                if (script != null && script != this)
                {
                    // 检查是否是常见的玩家控制脚本
                    string scriptName = script.GetType().Name.ToLower();
                    if (scriptName.Contains("player") || 
                        scriptName.Contains("movement") || 
                        scriptName.Contains("controller") ||
                        scriptName.Contains("input") ||
                        scriptName.Contains("camera"))
                    {
                        script.enabled = false;
                    }
                }
            }
        }
    }

    // 启用玩家控制
    private void EnablePlayerControls()
    {
        if (playerControlScripts != null)
        {
            foreach (MonoBehaviour script in playerControlScripts)
            {
                if (script != null && script != this)
                {
                    // 检查是否是常见的玩家控制脚本
                    string scriptName = script.GetType().Name.ToLower();
                    if (scriptName.Contains("player") || 
                        scriptName.Contains("movement") || 
                        scriptName.Contains("controller") ||
                        scriptName.Contains("input") ||
                        scriptName.Contains("camera"))
                    {
                        script.enabled = true;
                    }
                }
            }
        }
    }

    // 按钮事件处理

    // 继续游戏按钮
    private void OnContinueClicked()
    {
        ResumeGame();
    }

    // 加载游戏按钮
    private void OnLoadGameClicked()
    {
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

    // 设置按钮
    private void OnSettingsClicked()
    {
        // 这里可以打开设置菜单
        Debug.Log("打开设置菜单");
    }

    // 主菜单按钮
    private void OnMainMenuClicked()
    {
        // 确保时间正常（虽然我们不再暂停时间，但为了安全起见）
        Time.timeScale = 1f;
        
        // 加载主菜单场景
        SceneManager.LoadScene("HomePage");
        Debug.Log("返回主菜单");
    }

    // 退出按钮
    private void OnQuitClicked()
    {
        // 确保时间正常（虽然我们不再暂停时间，但为了安全起见）
        Time.timeScale = 1f;
        
        // 退出游戏
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            Application.Quit();
        #endif
        
        Debug.Log("退出游戏");
    }

    // 公共方法：检查是否暂停
    public bool IsPaused()
    {
        return isPaused;
    }

    // 公共方法：强制暂停
    public void ForcePause()
    {
        PauseGame();
    }

    // 公共方法：强制恢复
    public void ForceResume()
    {
        ResumeGame();
    }

    // 公共方法：设置玩家对象
    public void SetPlayerObject(GameObject player)
    {
        playerObject = player;
        InitializePlayerControls();
    }

    // 公共方法：设置玩家相机
    public void SetPlayerCamera(Camera camera)
    {
        playerCamera = camera;
    }

    // 公共方法：处理玩家位置变化（用于加载游戏时）
    public void HandlePlayerPositionChange()
    {
        // 如果当前处于暂停状态，确保玩家控制脚本被正确管理
        if (isPaused)
        {
            // 临时启用玩家控制以应用位置变化
            EnablePlayerControls();
            
            // 延迟一帧后重新禁用（如果仍然暂停）
            StartCoroutine(DelayedDisablePlayerControls());
        }
    }

    // 协程：延迟禁用玩家控制
    private System.Collections.IEnumerator DelayedDisablePlayerControls()
    {
        yield return null; // 等待一帧
        
        if (isPaused)
        {
            DisablePlayerControls();
        }
    }

    // 在销毁时确保时间恢复正常
    void OnDestroy()
    {
        Time.timeScale = 1f;
    }

    // 在应用失去焦点时暂停游戏（可选）
    void OnApplicationPause(bool pauseStatus)
    {
        if (pauseStatus && !isPaused)
        {
            PauseGame();
        }
    }
} 