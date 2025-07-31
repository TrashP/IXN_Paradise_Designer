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
    public Button btnDeleteSave;
    public Button btnClose;

    [Header("Save Data Manager")]
    public SaveData saveData; // 存档数据管理器

    void Start()
    {
        loadGamePanel.SetActive(false);

        // 设置按钮监听器
        SetupButtonListeners();
        
        // 如果没有指定SaveData，尝试在场景中查找
        if (saveData == null)
        {
            saveData = FindObjectOfType<SaveData>();
        }
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
        if (saveData != null)
        {
            saveData.SwitchSaveSlot(slotNumber);
            UpdateUI();
            Debug.Log($"切换到存档槽位 {slotNumber}");
        }
    }

    // 加载游戏按钮点击事件
    private void OnLoadGameClicked()
    {
        if (saveData != null)
        {
            int currentSlot = saveData.GetCurrentSlot();
            
            if (saveData.HasSave(currentSlot))
            {
                saveData.LoadCurrentSlot();
                Debug.Log($"从槽位 {currentSlot} 加载游戏");
                
                // 加载完成后可以切换到游戏场景
                LoadGameScene();
            }
            else
            {
                Debug.LogWarning($"槽位 {currentSlot} 没有存档数据");
            }
        }
    }

    // 删除存档按钮点击事件
    private void OnDeleteSaveClicked()
    {
        if (saveData != null)
        {
            int currentSlot = saveData.GetCurrentSlot();
            
            if (saveData.HasSave(currentSlot))
            {
                saveData.DeleteSave(currentSlot);
                UpdateUI();
                Debug.Log($"已删除槽位 {currentSlot} 的存档");
            }
            else
            {
                Debug.LogWarning($"槽位 {currentSlot} 没有存档数据");
            }
        }
    }

    // 更新UI显示
    private void UpdateUI()
    {
        if (saveData == null) return;

        // 更新当前槽位显示
        if (currentSlotText != null)
        {
            currentSlotText.text = $"当前槽位: {saveData.GetCurrentSlot()}";
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

        if (saveData.HasSave(slotNumber))
        {
            PlayerSaveData saveInfo = saveData.GetSaveInfo(slotNumber);
            if (saveInfo != null)
            {
                string timeStr = saveInfo.saveTime.ToString("yyyy-MM-dd HH:mm");
                infoText.text = $"槽位 {slotNumber}\n{saveInfo.saveName}\n{timeStr}";
            }
        }
        else
        {
            infoText.text = $"槽位 {slotNumber}\n空槽位";
        }
    }

    // 更新按钮状态
    private void UpdateButtonStates()
    {
        if (saveData == null) return;

        int currentSlot = saveData.GetCurrentSlot();
        bool hasSave = saveData.HasSave(currentSlot);

        // 加载和删除按钮只在有存档时可用
        if (btnLoadGame != null)
            btnLoadGame.interactable = hasSave;
            
        if (btnDeleteSave != null)
            btnDeleteSave.interactable = hasSave;
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
        if (saveData != null)
        {
            saveData.SwitchSaveSlot(slotNumber);
            UpdateUI();
        }
    }

    // 公共方法：从指定槽位加载
    public void LoadFromSlot(int slotNumber)
    {
        if (saveData != null && saveData.HasSave(slotNumber))
        {
            saveData.LoadPlayer(slotNumber);
            LoadGameScene();
        }
    }
}
