using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class LoadGameDialogController : MonoBehaviour
{
    [Header("Panel")]
    public GameObject loadGamePanel;

    [Header("Save Slots")]
    public Button[] saveSlotButtons = new Button[3]; // 3个存档槽位按钮 3 slots
    public TextMeshProUGUI[] slotInfoTexts = new TextMeshProUGUI[3]; // 槽位信息文本 3 slot info texts
    public TextMeshProUGUI currentSlotText; // 当前槽位显示 current slot text

    [Header("Action Buttons")]
    public Button btnLoadGame;
    public Button btnSaveGame; // 新增保存按钮 new save button
    public Button btnDeleteSave;
    public Button btnClose;

    [Header("Player Reference")]
    public Transform playerTransform; // 玩家Transform引用，用于保存位置 player transform reference, for saving position

    private int currentSelectedSlot = 1; // 当前选中的槽位 current selected slot

    // 静态变量用于在场景切换后保存数据 static variables for saving data after scene switch
    private static bool pendingLoadGame = false;
    private static int pendingSlotNumber = 1;
    private static Vector3 pendingPlayerPosition = Vector3.zero;
    private static Quaternion pendingPlayerRotation = Quaternion.identity;
    private static Vector3 pendingPlayerScale = Vector3.one;

    void Start()
    {
        loadGamePanel.SetActive(false);

        // 设置按钮监听器 set button listeners
        SetupButtonListeners();
        
        // 如果没有指定玩家Transform，尝试在场景中查找 if no player transform is specified, try to find it in the scene
        if (playerTransform == null)
        {
            // 方法1：通过Player标签查找 method 1: find player by Player tag
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                playerTransform = player.transform;
                Debug.Log("通过Player标签找到玩家对象 find player object by Player tag");
            }
            else
            {
                // 方法2：通过名称查找PlayerBoy method 2: find player by name
                player = GameObject.Find("PlayerBoy");
                if (player != null)
                {
                    playerTransform = player.transform;
                    Debug.Log("通过名称PlayerBoy找到玩家对象 find player object by name");
                }
                else
                {
                    // 方法3：查找所有GameObject，找到包含"Player"的对象 method 3: find all GameObjects, find objects containing "Player"
                    GameObject[] allObjects = FindObjectsByType<GameObject>(FindObjectsSortMode.None);
                    foreach (GameObject obj in allObjects)
                    {
                        if (obj.name.Contains("Player") || obj.name.Contains("player"))
                        {
                            playerTransform = obj.transform;
                            Debug.Log($"通过名称包含Player找到玩家对象: {obj.name} find player object by name containing Player ");
                            break;
                        }
                    }
                }
            }
            
            if (playerTransform == null)
            {
                Debug.LogWarning("未找到玩家对象，请确保场景中有PlayerBoy对象或设置正确的Player标签 no player object found, please ensure there is a PlayerBoy object or set the correct Player tag");
            }
        }

        // 初始化存档系统 initialize save system
        SaveDataManager.Initialize();

        // 检查是否有待加载的游戏数据 check if there is any pending game data to load
        CheckPendingLoadGame();
    }

    private void SetupButtonListeners()
    {
        // 设置存档槽位按钮监听器 set save slot button listeners
        for (int i = 0; i < saveSlotButtons.Length; i++)
        {
            int slotNumber = i + 1; // 槽位从1开始 slot number starts from 1
            if (saveSlotButtons[i] != null)
            {
                saveSlotButtons[i].onClick.AddListener(() => OnSlotButtonClicked(slotNumber));
            }
        }

        // 设置功能按钮监听器 set function button listeners
        if (btnLoadGame != null)
            btnLoadGame.onClick.AddListener(OnLoadGameClicked);
        if (btnSaveGame != null)
            btnSaveGame.onClick.AddListener(OnSaveGameClicked);
        if (btnDeleteSave != null)
            btnDeleteSave.onClick.AddListener(OnDeleteSaveClicked);
        if (btnClose != null)
            btnClose.onClick.AddListener(HidePanel);
    }

    public void ShowPanel()
    {
        if (loadGamePanel != null)
        {
            loadGamePanel.SetActive(true);
            UpdateUI(); // 显示面板时更新UI when the panel is displayed, update the UI
        }
    }

    public void HidePanel()
    {
        if (loadGamePanel != null)
            loadGamePanel.SetActive(false);
    }

    // 槽位按钮点击事件 slot button click event
    private void OnSlotButtonClicked(int slotNumber)
    {
        currentSelectedSlot = slotNumber;
        UpdateUI();
        Debug.Log($"选中存档槽位 {currentSelectedSlot} selected save slot {currentSelectedSlot}");
    }

    // 加载游戏按钮点击事件 load game button click event
    private void OnLoadGameClicked()
    {
        if (SaveDataManager.HasSave(currentSelectedSlot))
        {
            PlayerSaveData loadedData = SaveDataManager.LoadPlayer(currentSelectedSlot);
            if (loadedData != null)
            {
                // 检查当前场景 check current scene
                string currentScene = SceneManager.GetActiveScene().name;
                
                if (currentScene == "HomePage" || currentScene == "MainMenu" || currentScene != "Dev")
                {
                    // 在主菜单场景中，需要先加载Dev场景，然后设置玩家位置 in the main menu scene, you need to load the Dev scene first, then set the player position
                    Debug.Log($"当前在 {currentScene} 场景，需要先加载Dev场景 current scene is {currentScene}, you need to load the Dev scene first");
                    
                    // 保存需要加载的数据到静态变量 save the data to load to static variables
                    pendingLoadGame = true;
                    pendingSlotNumber = currentSelectedSlot;
                    pendingPlayerPosition = loadedData.position;
                    pendingPlayerRotation = loadedData.rotation;
                    pendingPlayerScale = loadedData.scale;
                    
                    // 加载Dev场景 load the Dev scene
                    SceneManager.LoadScene("Dev");
                }
                else
                {
                    // 在Dev场景中，直接设置玩家transform in the Dev scene, set the player transform directly   
                    if (playerTransform != null)
                    {
                        playerTransform.position = loadedData.position;
                        playerTransform.rotation = loadedData.rotation;
                        playerTransform.localScale = loadedData.scale;
                        Debug.Log($"从槽位 {currentSelectedSlot} 加载游戏，位置: {loadedData.position}, 旋转: {loadedData.rotation}, 缩放: {loadedData.scale} load game from slot {currentSelectedSlot}, position: {loadedData.position}, rotation: {loadedData.rotation}, scale: {loadedData.scale}");
                        Debug.Log($"玩家当前transform: 位置={playerTransform.position}, 旋转={playerTransform.rotation}, 缩放={playerTransform.localScale} player current transform: position={playerTransform.position}, rotation={playerTransform.rotation}, scale={playerTransform.localScale}");
                        
                        // 确保玩家控制脚本被启用 ensure player control script is enabled
                        EnsurePlayerControlsEnabled();
                        
                        // 强制恢复游戏状态，确保立即返回游戏 force resume game, ensure immediate return to game
                        ForceResumeGame();
                    }
                    else
                    {
                        Debug.LogError("未找到玩家Transform，无法设置transform数据 no player transform found, cannot set transform data");
                    }
                }
                
                // 关闭加载游戏对话框 close the load game dialog
                HidePanel();
            }
        }
        else
        {
            Debug.LogWarning($"槽位 {currentSelectedSlot} 没有存档数据 slot {currentSelectedSlot} has no save data");
        }
    }

    // 保存游戏按钮点击事件 save game button click event
    private void OnSaveGameClicked()
    {
        if (playerTransform != null)
        {
            SaveDataManager.SavePlayer(currentSelectedSlot, playerTransform.position, playerTransform.rotation, playerTransform.localScale);
            Debug.Log($"已保存到槽位 {currentSelectedSlot} saved to slot {currentSelectedSlot}");
            UpdateUI(); // 保存后刷新UI after saving, refresh the UI
        }
        else
        {
            Debug.LogError("未找到玩家Transform，无法保存transform数据 no player transform found, cannot save transform data");
        }
    }

    // 删除存档按钮点击事件 delete save button click event
    private void OnDeleteSaveClicked()
    {
        if (SaveDataManager.HasSave(currentSelectedSlot))
        {
            SaveDataManager.DeleteSave(currentSelectedSlot);
            UpdateUI();
            Debug.Log($"已删除槽位 {currentSelectedSlot} 的存档 deleted slot {currentSelectedSlot} save data");
        }
        else
        {
            Debug.LogWarning($"槽位 {currentSelectedSlot} 没有存档数据 slot {currentSelectedSlot} has no save data  ");
        }
    }

    // 更新UI显示 update UI display
    private void UpdateUI()
    {
        // 更新当前槽位显示 update current slot display
        if (currentSlotText != null)
        {
            currentSlotText.text = $"当前槽位: {currentSelectedSlot} current slot: {currentSelectedSlot}";
        }

        // 更新槽位信息显示 update slot info display
        for (int i = 0; i < slotInfoTexts.Length; i++)
        {
            int slotNumber = i + 1; // 槽位从1开始 slot number starts from 1
            UpdateSlotInfo(slotNumber, slotInfoTexts[i]);
        }

        // 更新按钮状态 update button states
        UpdateButtonStates();
    }

    // 更新单个槽位信息 update single slot info
    private void UpdateSlotInfo(int slotNumber, TextMeshProUGUI infoText)
    {
        if (infoText == null) return;

        if (SaveDataManager.HasSave(slotNumber))
        {
            PlayerSaveData saveInfo = SaveDataManager.GetSaveInfo(slotNumber);
            if (saveInfo != null)
            {
                // 直接使用保存的时间字符串，不需要转换 directly use the saved time string, no conversion needed
                infoText.text = $"{saveInfo.saveName}  {saveInfo.saveTimeString}";
            }
        }
        else
        {
            infoText.text = $"EMPTY";
        }
    }

    // 更新按钮状态 update button states
    private void UpdateButtonStates()
    {
        bool hasSave = SaveDataManager.HasSave(currentSelectedSlot);

        // 加载和删除按钮只在有存档时可用 load and delete buttons are only available when there is save data
        if (btnLoadGame != null)
            btnLoadGame.interactable = hasSave;
            
        if (btnDeleteSave != null)
            btnDeleteSave.interactable = hasSave;

        // 保存按钮在任何情况下都可用 save button is always available
        if (btnSaveGame != null)
            btnSaveGame.interactable = true;
    }

    // 加载游戏场景 load game scene
    private void LoadGameScene()
    {
        // 这里可以根据需要加载不同的游戏场景 here you can load different game scenes based on your needs
        // 例如：SceneManager.LoadScene("GameScene");
        Debug.Log("加载游戏场景 load game scene");
        
        // 示例：加载开发场景 example: load the Dev scene
        SceneManager.LoadScene("Dev");
    }

    // 公共方法：手动刷新UI（供外部调用） public method: manually refresh UI (for external use)
    public void RefreshUI()
    {
        UpdateUI();
    }

    // 公共方法：切换到指定槽位 public method: switch to a specific slot
    public void SwitchToSlot(int slotNumber)
    {
        currentSelectedSlot = slotNumber;
        UpdateUI();
    }

    // 公共方法：从指定槽位加载 public method: load from a specific slot
    public void LoadFromSlot(int slotNumber)
    {
        if (SaveDataManager.HasSave(slotNumber))
        {
            PlayerSaveData loadedData = SaveDataManager.LoadPlayer(slotNumber);
            if (loadedData != null)
            {
                // 检查当前场景 check current scene
                string currentScene = SceneManager.GetActiveScene().name;
                
                if (currentScene == "HomePage" || currentScene == "MainMenu" || currentScene != "Dev")
                {
                    // 在主菜单场景中，需要先加载Dev场景，然后设置玩家位置 in the main menu scene, you need to load the Dev scene first, then set the player position
                    Debug.Log($"当前在 {currentScene} 场景，需要先加载Dev场景 current scene is {currentScene}, you need to load the Dev scene first");
                    
                    // 保存需要加载的数据到静态变量 save the data to load to static variables
                    pendingLoadGame = true;
                    pendingSlotNumber = slotNumber;
                    pendingPlayerPosition = loadedData.position;
                    pendingPlayerRotation = loadedData.rotation;
                    pendingPlayerScale = loadedData.scale;
                    
                    // 加载Dev场景 load the Dev scene
                    SceneManager.LoadScene("Dev");
                }
                else
                {
                    // 在Dev场景中，直接设置玩家transform in the Dev scene, set the player transform directly
                    if (playerTransform != null)
                    {
                        playerTransform.position = loadedData.position;
                        playerTransform.rotation = loadedData.rotation;
                        playerTransform.localScale = loadedData.scale;
                        Debug.Log($"从槽位 {slotNumber} 加载游戏，位置: {loadedData.position}, 旋转: {loadedData.rotation}, 缩放: {loadedData.scale}");
                        Debug.Log($"玩家当前transform: 位置={playerTransform.position}, 旋转={playerTransform.rotation}, 缩放={playerTransform.localScale}");
                        
                        // 确保玩家控制脚本被启用 ensure player control script is enabled
                        EnsurePlayerControlsEnabled();
                        
                        // 强制恢复游戏状态，确保立即返回游戏 force resume game, ensure immediate return to game
                        ForceResumeGame();
                    }
                    else
                    {
                        Debug.LogError("未找到玩家Transform，无法设置transform数据 no player transform found, cannot set transform data");
                    }
                }
                
                // 隐藏加载面板 hide the load panel
                HidePanel();
            }
        }
    }

    // 公共方法：保存到指定槽位 public method: save to a specific slot
    public void SaveToSlot(int slotNumber)
    {
        if (playerTransform != null)
        {
            SaveDataManager.SavePlayer(slotNumber, playerTransform.position, playerTransform.rotation, playerTransform.localScale);
            UpdateUI();
            Debug.Log($"已保存到槽位 {slotNumber} saved to slot {slotNumber}");
        }
        else
        {
            Debug.LogError("未找到玩家Transform，无法保存transform数据 no player transform found, cannot save transform data");
        }
    }

    // 公共方法：获取当前选中的槽位 public method: get the current selected slot
    public int GetCurrentSelectedSlot()
    {
        return currentSelectedSlot;
    }

    // 强制恢复游戏状态 force resume game
    private void ForceResumeGame()
    {
        // 查找暂停菜单管理器 find the pause menu manager
        PauseMenuManager pauseMenu = FindFirstObjectByType<PauseMenuManager>();
        if (pauseMenu != null)
        {
            // 强制恢复游戏状态 force resume game
            pauseMenu.ForceResume();
            Debug.Log("强制恢复游戏状态，立即返回游戏 force resume game, ensure immediate return to game");
        }
        else
        {
            Debug.LogWarning("未找到PauseMenuManager，无法强制恢复游戏状态 no PauseMenuManager found, cannot force resume game");
        }
    }

    // 确保玩家控制脚本被启用 ensure player control script is enabled
    private void EnsurePlayerControlsEnabled()
    {
        if (playerTransform != null)
        {
            // 获取玩家对象上的所有MonoBehaviour脚本 get all MonoBehaviour scripts on the player object
            MonoBehaviour[] playerScripts = playerTransform.GetComponents<MonoBehaviour>();
            
            foreach (MonoBehaviour script in playerScripts)
            {
                if (script != null)
                {
                    // 检查是否是玩家控制相关的脚本 check if it is a player control related script
                    string scriptName = script.GetType().Name.ToLower();
                    if (scriptName.Contains("player") || 
                        scriptName.Contains("movement") || 
                        scriptName.Contains("controller") ||
                        scriptName.Contains("input") ||
                        scriptName.Contains("camera"))
                    {
                        // 确保脚本被启用 ensure the script is enabled
                        if (!script.enabled)
                        {
                            script.enabled = true;
                            Debug.Log($"重新启用玩家控制脚本: {script.GetType().Name} re-enable player control script: {script.GetType().Name}");
                        }
                    }
                }
            }
            
            // 如果有CharacterController，确保它不会阻止位置变化 if there is a CharacterController, ensure it does not prevent position changes
            CharacterController characterController = playerTransform.GetComponent<CharacterController>();
            if (characterController != null)
            {
                // 强制更新CharacterController的位置 force update the CharacterController position
                characterController.enabled = false;
                characterController.enabled = true;
                Debug.Log("重置CharacterController状态 reset CharacterController state");
            }
        }
        
        // 查找并处理暂停菜单状态（如果存在） find and handle the pause menu state (if it exists)
        PauseMenuManager pauseMenu = FindFirstObjectByType<PauseMenuManager>();
        if (pauseMenu != null)
        {
            if (pauseMenu.IsPaused())
            {
                // 使用新的位置变化处理方法 use the new position change handling method
                pauseMenu.HandlePlayerPositionChange();
                Debug.Log("处理暂停状态下的玩家位置变化 handle player position change in pause state");
            }
            else
            {
                // 如果不在暂停状态，确保游戏正常运行 if not in pause state, ensure the game runs normally
                pauseMenu.ForceResume();
                Debug.Log("确保游戏正常运行状态 ensure the game runs normally");
            }
        }
    }

    // 检查并处理待加载的游戏数据 check and handle pending game data
    private void CheckPendingLoadGame()
    {
        if (pendingLoadGame)
        {
            Debug.Log($"检测到待加载的游戏数据，槽位: {pendingSlotNumber} detected pending game data, slot: {pendingSlotNumber}");
            
            // 延迟一帧执行，确保场景完全加载 delay one frame to ensure the scene is fully loaded
            StartCoroutine(DelayedLoadGameData());
        }
    }

    // 协程：延迟加载游戏数据 coroutine: delay load game data
    private System.Collections.IEnumerator DelayedLoadGameData()
    {
        // 等待一帧，确保场景完全加载 wait one frame to ensure the scene is fully loaded
        yield return null;
        
        // 再次尝试查找玩家对象 try to find the player object again
        if (playerTransform == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player == null)
            {
                player = GameObject.Find("PlayerBoy");
            }
            if (player == null)
            {
                GameObject[] allObjects = FindObjectsByType<GameObject>(FindObjectsSortMode.None);
                foreach (GameObject obj in allObjects)
                {
                    if (obj.name.Contains("Player") || obj.name.Contains("player"))
                    {
                        player = obj;
                        break;
                    }
                }
            }
            
            if (player != null)
            {
                playerTransform = player.transform;
                Debug.Log($"场景切换后重新找到玩家对象: {player.name} re-find player object after scene switch: {player.name}");
            }
        }

        if (playerTransform != null)
        {
            // 设置玩家transform set the player transform
            playerTransform.position = pendingPlayerPosition;
            playerTransform.rotation = pendingPlayerRotation;
            playerTransform.localScale = pendingPlayerScale;
            Debug.Log($"从槽位 {pendingSlotNumber} 加载游戏，位置: {pendingPlayerPosition}, 旋转: {pendingPlayerRotation}, 缩放: {pendingPlayerScale}");
            Debug.Log($"玩家当前transform: 位置={playerTransform.position}, 旋转={playerTransform.rotation}, 缩放={playerTransform.localScale}");
            
            // 确保玩家控制脚本被启用 ensure player control script is enabled
            EnsurePlayerControlsEnabled();
            
            // 强制恢复游戏状态，确保立即返回游戏 force resume game, ensure immediate return to game
            ForceResumeGame();
            
            Debug.Log("成功加载游戏数据 success load game data");
        }
        else
        {
            Debug.LogError("场景切换后仍未找到玩家对象，无法设置transform数据 no player object found after scene switch, cannot set transform data");
        }

        // 清除待加载数据 clear pending load data
        pendingLoadGame = false;
        pendingSlotNumber = 1;
        pendingPlayerPosition = Vector3.zero;
        pendingPlayerRotation = Quaternion.identity;
        pendingPlayerScale = Vector3.one;
    }
}
