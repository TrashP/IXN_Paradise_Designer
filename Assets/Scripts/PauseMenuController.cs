using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Polyperfect.Crafting.Demo; // 添加 Polyperfect 命名空间

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
    public GameObject playerObject; // 玩家对象 player object
    public Camera playerCamera; // 玩家相机 player camera

    [Header("Cursor Control")]
    public bool lockCursorOnResume = true; // 恢复游戏时是否锁定鼠标 lock cursor on resume

    // 私有变量 private variables
    private bool isPaused = false;
    private CursorLockMode previousCursorLockMode;
    private bool previousCursorVisible;
    private MonoBehaviour[] playerControlScripts; // 存储玩家控制脚本 store player control scripts
    private ICommandablePlayer commandablePlayer; // 存储可命令玩家组件 store commandable player component

    void Start()
    {
        // 初始化暂停菜单 initialize pause menu
        if (pauseMenuPanel != null)
        {
            pauseMenuPanel.SetActive(false);
        }

        // 设置按钮监听器 set button listeners
        SetupButtonListeners();

        // 初始化玩家控制脚本 initialize player control scripts
        InitializePlayerControls();

        // 设置初始鼠标状态 set initial mouse state
        SetCursorLocked(true);
    }

    void Update()
    {
        // 检测Tab键输入 check Tab key input
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            TogglePauseMenu();
        }

        // 检测ESC键输入（备用暂停键） check ESC key input (backup pause key)
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            TogglePauseMenu();
        }
    }

    private void SetupButtonListeners()
    {
        // 设置继续游戏按钮 set continue game button
        if (btnContinue != null)
            btnContinue.onClick.AddListener(OnContinueClicked);

        // 设置加载游戏按钮 set load game button
        if (btnLoadGame != null)
            btnLoadGame.onClick.AddListener(OnLoadGameClicked);

        // 设置设置按钮 set settings button 
        if (btnSettings != null)
            btnSettings.onClick.AddListener(OnSettingsClicked);

        // 设置主菜单按钮 set main menu button
        if (btnMainMenu != null)
            btnMainMenu.onClick.AddListener(OnMainMenuClicked);

        // 设置退出按钮 set exit button
        if (btnQuit != null)
            btnQuit.onClick.AddListener(OnQuitClicked);
    }

    private void InitializePlayerControls()
    {
        // 如果没有指定玩家对象，尝试查找 if no player object is specified, try to find
        if (playerObject == null)
        {
            playerObject = GameObject.FindGameObjectWithTag("Player");
        }

        // 如果没有指定相机，尝试查找主相机 if no camera is specified, try to find the main camera
        if (playerCamera == null)
        {
            playerCamera = Camera.main;
        }

        // 获取玩家控制脚本（常见的玩家控制脚本） get player control scripts (common player control scripts)
        if (playerObject != null)
        {
            playerControlScripts = playerObject.GetComponents<MonoBehaviour>();
            
            // 获取可命令玩家组件 get commandable player component
            commandablePlayer = playerObject.GetComponent<ICommandablePlayer>();
        }
    }

    // 切换暂停菜单状态 toggle pause menu state
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

    // 暂停游戏 pause game
    public void PauseGame()
    {
        if (isPaused) return;

        isPaused = true;

        // 显示暂停菜单 show pause menu
        if (pauseMenuPanel != null)
        {
            pauseMenuPanel.SetActive(true);
        }

        // 不暂停时间，让世界继续运行 do not pause time, let the world continue running
        // Time.timeScale = 0f; // 注释掉这行，让时间继续流动

        // 保存当前鼠标状态 save current mouse state
        previousCursorLockMode = Cursor.lockState;
        previousCursorVisible = Cursor.visible;

        // 显示鼠标光标 show mouse cursor
        SetCursorLocked(false);

        // 取消正在执行的命令 cancel any executing commands
        CancelExecutingCommands();

        // 禁用玩家 control
        DisablePlayerControls();

        Debug.Log("玩家操作已暂停，世界继续运行 player control is paused, world continues running");
    }

    // 恢复游戏 resume game
    public void ResumeGame()
    {
        if (!isPaused) return;

        isPaused = false;

        // 隐藏暂停菜单 hide pause menu
        if (pauseMenuPanel != null)
        {
            pauseMenuPanel.SetActive(false);
        }

        // 时间继续正常流动，不需要恢复 time continues normally, no need to restore
        // Time.timeScale = 1f; // 注释掉这行，因为时间从未被暂停

        // 恢复鼠标状态 restore mouse state
        if (lockCursorOnResume)
        {
            SetCursorLocked(true);
        }
        else
        {
            Cursor.lockState = previousCursorLockMode;
            Cursor.visible = previousCursorVisible;
        }

        // 启用玩家控制 enable player control
        EnablePlayerControls();

        Debug.Log("玩家操作已恢复 player control is resumed");
    }

    // 取消正在执行的命令 cancel executing commands
    private void CancelExecutingCommands()
    {
        // 查找所有正在执行的命令并取消它们 find all executing commands and cancel them
        var commands = FindObjectsOfType<MonoBehaviour>();
        foreach (var command in commands)
        {
            if (command is ICommand cmd)
            {
                // 检查命令是否已完成 check if command is finished
                var baseCommand = cmd as BaseCommand;
                if (baseCommand != null)
                {
                    // 使用反射检查 finished 字段 use reflection to check finished field
                    var finishedField = typeof(BaseCommand).GetField("finished", 
                        System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                    if (finishedField != null)
                    {
                        bool isFinished = (bool)finishedField.GetValue(baseCommand);
                        if (!isFinished)
                        {
                            cmd.Cancel();
                            Debug.Log($"已取消正在执行的命令: {cmd.GetType().Name} cancelled executing command: {cmd.GetType().Name}");
                        }
                    }
                }
            }
        }
    }

    // 设置鼠标锁定状态 set cursor locked state
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

    // 禁用玩家控制 disable player control
    private void DisablePlayerControls()
    {
        if (playerControlScripts != null)
        {
            foreach (MonoBehaviour script in playerControlScripts)
            {
                if (script != null && script != this)
                {
                    // 检查是否是常见的玩家控制脚本 check if it is a common player control script
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

    // 启用玩家控制 enable player control
    private void EnablePlayerControls()
    {
        if (playerControlScripts != null)
        {
            foreach (MonoBehaviour script in playerControlScripts)
            {
                if (script != null && script != this)
                {
                    // 检查是否是常见的玩家控制脚本 check if it is a common player control script
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

    // 按钮事件处理 button event handling

    // 继续游戏按钮 continue game button
    private void OnContinueClicked()
    {
        ResumeGame();
    }

    // 加载游戏按钮 load game button
    private void OnLoadGameClicked()
    {
        // 查找并打开加载游戏对话框 find and open the load game dialog
        LoadGameDialogController loadGameDialog = FindFirstObjectByType<LoadGameDialogController>();
        if (loadGameDialog != null)
        {
            loadGameDialog.ShowPanel();
            Debug.Log("打开加载游戏对话框 open the load game dialog");
        }
        else
        {
            Debug.LogWarning("未找到LoadGameDialogController组件 no LoadGameDialogController component found");
        }
    }

    // 设置按钮 set button
    private void OnSettingsClicked()
    {
        // 这里可以打开设置菜单 here you can open the settings menu
        Debug.Log("打开设置菜单 open the settings menu");
    }

    // 主菜单按钮 main menu button
    private void OnMainMenuClicked()
    {
        // 确保时间正常（虽然我们不再暂停时间，但为了安全起见） ensure time is normal (although we no longer pause time, for safety)
        Time.timeScale = 1f;
        
        // 加载主菜单场景 load the main menu scene
        SceneManager.LoadScene("HomePage");
        Debug.Log("返回主菜单 return to the main menu");
    }

    // 退出按钮 exit button
    private void OnQuitClicked()
    {
        // 确保时间正常（虽然我们不再暂停时间，但为了安全起见） ensure time is normal (although we no longer pause time, for safety)
        Time.timeScale = 1f;
        
        // 退出游戏 exit game
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            Application.Quit();
        #endif
        
        Debug.Log("退出游戏 exit game");
    }

    // 公共方法：检查是否暂停 public method: check if paused
    public bool IsPaused()
    {
        return isPaused;
    }

    // 公共方法：强制暂停 public method: force pause
    public void ForcePause()
    {
        PauseGame();
    }

    // 公共方法：强制恢复 public method: force resume
    public void ForceResume()
    {
        ResumeGame();
    }

    // 公共方法：设置玩家对象 public method: set player object
    public void SetPlayerObject(GameObject player)
    {
        playerObject = player;
        InitializePlayerControls();
    }

    // 公共方法：设置玩家相机 public method: set player camera
    public void SetPlayerCamera(Camera camera)
    {
        playerCamera = camera;
    }

    // 公共方法：处理玩家位置变化（用于加载游戏时） public method: handle player position change (for loading game)
    public void HandlePlayerPositionChange()
    {
        // 如果当前处于暂停状态，确保玩家控制脚本被正确管理 if currently in pause state, ensure player control scripts are correctly managed
        if (isPaused)
        {
            // 临时启用玩家控制以应用位置变化 temporarily enable player control to apply position changes
            EnablePlayerControls();
            
            // 延迟一帧后重新禁用（如果仍然暂停） delay one frame to re-disable (if still paused)
            StartCoroutine(DelayedDisablePlayerControls());
        }
    }

    // 协程：延迟禁用玩家控制 coroutine: delay disable player control
    private System.Collections.IEnumerator DelayedDisablePlayerControls()
    {
        yield return null; // 等待一帧 wait one frame
        
        if (isPaused)
        {
            DisablePlayerControls();
        }
    }

    // 在销毁时确保时间恢复正常 on destroy, ensure time is normal
    void OnDestroy()
    {
        Time.timeScale = 1f;
    }

    // 在应用失去焦点时暂停游戏（可选） on application pause (optional)
    void OnApplicationPause(bool pauseStatus)
    {
        if (pauseStatus && !isPaused)
        {
            PauseGame();
        }
    }
} 