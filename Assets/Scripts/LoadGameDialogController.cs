using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class LoadGameDialogController : MonoBehaviour
{
    [Header("Panel")]
    public GameObject loadGamePanel;

    [Header("Save Slots")]
    public Button[] saveSlotButtons = new Button[3]; // 3个存档槽位按钮
    public TextMeshProUGUI[] slotInfoTexts = new TextMeshProUGUI[3]; // 槽位信息文本
    public TextMeshProUGUI currentSlotText; // 当前槽位显示

    [Header("Action Buttons")]
    public Button btnLoadGame;
    public Button btnSaveGame; // 新增保存按钮
    public Button btnDeleteSave;
    public Button btnClose;

    [Header("Player Reference")]
    public Transform playerTransform; // 玩家Transform引用，用于保存位置

    private int currentSelectedSlot = 1; // 当前选中的槽位

    void Start()
    {
        loadGamePanel.SetActive(false);

        // 设置按钮监听器
        SetupButtonListeners();
        
        // 如果没有指定玩家Transform，尝试在场景中查找
        if (playerTransform == null)
        {
            // 方法1：通过Player标签查找
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                playerTransform = player.transform;
                Debug.Log("通过Player标签找到玩家对象");
            }
            else
            {
                // 方法2：通过名称查找PlayerBoy
                player = GameObject.Find("PlayerBoy");
                if (player != null)
                {
                    playerTransform = player.transform;
                    Debug.Log("通过名称PlayerBoy找到玩家对象");
                }
                else
                {
                    // 方法3：查找所有GameObject，找到包含"Player"的对象
                    GameObject[] allObjects = FindObjectsOfType<GameObject>();
                    foreach (GameObject obj in allObjects)
                    {
                        if (obj.name.Contains("Player") || obj.name.Contains("player"))
                        {
                            playerTransform = obj.transform;
                            Debug.Log($"通过名称包含Player找到玩家对象: {obj.name}");
                            break;
                        }
                    }
                }
            }
            
            if (playerTransform == null)
            {
                Debug.LogWarning("未找到玩家对象，请确保场景中有PlayerBoy对象或设置正确的Player标签");
            }
        }

        // 初始化存档系统
        SaveDataManager.Initialize();
    }

    private void SetupButtonListeners()
    {
        // 设置存档槽位按钮监听器
        for (int i = 0; i < saveSlotButtons.Length; i++)
        {
            int slotNumber = i + 1; // 槽位从1开始
            if (saveSlotButtons[i] != null)
            {
                saveSlotButtons[i].onClick.AddListener(() => OnSlotButtonClicked(slotNumber));
            }
        }

        // 设置功能按钮监听器
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
            UpdateUI(); // 显示面板时更新UI
        }
    }

    public void HidePanel()
    {
        if (loadGamePanel != null)
            loadGamePanel.SetActive(false);
    }

    // 槽位按钮点击事件
    private void OnSlotButtonClicked(int slotNumber)
    {
        currentSelectedSlot = slotNumber;
        UpdateUI();
        Debug.Log($"选中存档槽位 {currentSelectedSlot}");
    }

    // 加载游戏按钮点击事件
    private void OnLoadGameClicked()
    {
        if (SaveDataManager.HasSave(currentSelectedSlot))
        {
            PlayerSaveData loadedData = SaveDataManager.LoadPlayer(currentSelectedSlot);
            if (loadedData != null && playerTransform != null)
            {
                // 设置玩家位置
                playerTransform.position = loadedData.position;
                Debug.Log($"从槽位 {currentSelectedSlot} 加载游戏，位置: {loadedData.position}");
                Debug.Log($"玩家当前位置: {playerTransform.position}");
                
                // 确保玩家控制脚本被启用
                EnsurePlayerControlsEnabled();
                
                // 强制恢复游戏状态，确保立即返回游戏
                ForceResumeGame();
                
                // 关闭加载游戏对话框
                HidePanel();
            }
        }
        else
        {
            Debug.LogWarning($"槽位 {currentSelectedSlot} 没有存档数据");
        }
    }

    // 保存游戏按钮点击事件
    private void OnSaveGameClicked()
    {
        if (playerTransform != null)
        {
            SaveDataManager.SavePlayer(currentSelectedSlot, playerTransform.position);
            Debug.Log($"已保存到槽位 {currentSelectedSlot}");
            UpdateUI(); // 保存后刷新UI
        }
        else
        {
            Debug.LogError("未找到玩家Transform，无法保存位置");
        }
    }

    // 删除存档按钮点击事件
    private void OnDeleteSaveClicked()
    {
        if (SaveDataManager.HasSave(currentSelectedSlot))
        {
            SaveDataManager.DeleteSave(currentSelectedSlot);
            UpdateUI();
            Debug.Log($"已删除槽位 {currentSelectedSlot} 的存档");
        }
        else
        {
            Debug.LogWarning($"槽位 {currentSelectedSlot} 没有存档数据");
        }
    }

    // 更新UI显示
    private void UpdateUI()
    {
        // 更新当前槽位显示
        if (currentSlotText != null)
        {
            currentSlotText.text = $"当前槽位: {currentSelectedSlot}";
        }

        // 更新槽位信息显示
        for (int i = 0; i < slotInfoTexts.Length; i++)
        {
            int slotNumber = i + 1;
            UpdateSlotInfo(slotNumber, slotInfoTexts[i]);
        }

        // 更新按钮状态
        UpdateButtonStates();
    }

    // 更新单个槽位信息
    private void UpdateSlotInfo(int slotNumber, TextMeshProUGUI infoText)
    {
        if (infoText == null) return;

        if (SaveDataManager.HasSave(slotNumber))
        {
            PlayerSaveData saveInfo = SaveDataManager.GetSaveInfo(slotNumber);
            if (saveInfo != null)
            {
                // 直接使用保存的时间字符串，不需要转换
                infoText.text = $"{saveInfo.saveName}  {saveInfo.saveTimeString}";
            }
        }
        else
        {
            infoText.text = $"EMPTY";
        }
    }

    // 更新按钮状态
    private void UpdateButtonStates()
    {
        bool hasSave = SaveDataManager.HasSave(currentSelectedSlot);

        // 加载和删除按钮只在有存档时可用
        if (btnLoadGame != null)
            btnLoadGame.interactable = hasSave;
            
        if (btnDeleteSave != null)
            btnDeleteSave.interactable = hasSave;

        // 保存按钮在任何情况下都可用
        if (btnSaveGame != null)
            btnSaveGame.interactable = true;
    }

    // 加载游戏场景
    private void LoadGameScene()
    {
        // 这里可以根据需要加载不同的游戏场景
        // 例如：SceneManager.LoadScene("GameScene");
        Debug.Log("加载游戏场景");
        
        // 示例：加载开发场景
        SceneManager.LoadScene("Dev");
    }

    // 公共方法：手动刷新UI（供外部调用）
    public void RefreshUI()
    {
        UpdateUI();
    }

    // 公共方法：切换到指定槽位
    public void SwitchToSlot(int slotNumber)
    {
        currentSelectedSlot = slotNumber;
        UpdateUI();
    }

    // 公共方法：从指定槽位加载
    public void LoadFromSlot(int slotNumber)
    {
        if (SaveDataManager.HasSave(slotNumber))
        {
            PlayerSaveData loadedData = SaveDataManager.LoadPlayer(slotNumber);
            if (loadedData != null && playerTransform != null)
            {
                // 设置玩家位置
                playerTransform.position = loadedData.position;
                Debug.Log($"从槽位 {slotNumber} 加载游戏，位置: {loadedData.position}");
                Debug.Log($"玩家当前位置: {playerTransform.position}");
                
                // 确保玩家控制脚本被启用
                EnsurePlayerControlsEnabled();
                
                // 强制恢复游戏状态，确保立即返回游戏
                ForceResumeGame();
                
                // 隐藏加载面板
                HidePanel();
            }
        }
    }

    // 公共方法：保存到指定槽位
    public void SaveToSlot(int slotNumber)
    {
        if (playerTransform != null)
        {
            SaveDataManager.SavePlayer(slotNumber, playerTransform.position);
            UpdateUI();
            Debug.Log($"已保存到槽位 {slotNumber}");
        }
        else
        {
            Debug.LogError("未找到玩家Transform，无法保存位置");
        }
    }

    // 公共方法：获取当前选中的槽位
    public int GetCurrentSelectedSlot()
    {
        return currentSelectedSlot;
    }

    // 强制恢复游戏状态
    private void ForceResumeGame()
    {
        // 查找暂停菜单管理器
        PauseMenuManager pauseMenu = FindFirstObjectByType<PauseMenuManager>();
        if (pauseMenu != null)
        {
            // 强制恢复游戏状态
            pauseMenu.ForceResume();
            Debug.Log("强制恢复游戏状态，立即返回游戏");
        }
        else
        {
            Debug.LogWarning("未找到PauseMenuManager，无法强制恢复游戏状态");
        }
    }

    // 确保玩家控制脚本被启用
    private void EnsurePlayerControlsEnabled()
    {
        if (playerTransform != null)
        {
            // 获取玩家对象上的所有MonoBehaviour脚本
            MonoBehaviour[] playerScripts = playerTransform.GetComponents<MonoBehaviour>();
            
            foreach (MonoBehaviour script in playerScripts)
            {
                if (script != null)
                {
                    // 检查是否是玩家控制相关的脚本
                    string scriptName = script.GetType().Name.ToLower();
                    if (scriptName.Contains("player") || 
                        scriptName.Contains("movement") || 
                        scriptName.Contains("controller") ||
                        scriptName.Contains("input") ||
                        scriptName.Contains("camera"))
                    {
                        // 确保脚本被启用
                        if (!script.enabled)
                        {
                            script.enabled = true;
                            Debug.Log($"重新启用玩家控制脚本: {script.GetType().Name}");
                        }
                    }
                }
            }
            
            // 如果有CharacterController，确保它不会阻止位置变化
            CharacterController characterController = playerTransform.GetComponent<CharacterController>();
            if (characterController != null)
            {
                // 强制更新CharacterController的位置
                characterController.enabled = false;
                characterController.enabled = true;
                Debug.Log("重置CharacterController状态");
            }
        }
        
        // 查找并处理暂停菜单状态（如果存在）
        PauseMenuManager pauseMenu = FindFirstObjectByType<PauseMenuManager>();
        if (pauseMenu != null)
        {
            if (pauseMenu.IsPaused())
            {
                // 使用新的位置变化处理方法
                pauseMenu.HandlePlayerPositionChange();
                Debug.Log("处理暂停状态下的玩家位置变化");
            }
            else
            {
                // 如果不在暂停状态，确保游戏正常运行
                pauseMenu.ForceResume();
                Debug.Log("确保游戏正常运行状态");
            }
        }
    }
}
