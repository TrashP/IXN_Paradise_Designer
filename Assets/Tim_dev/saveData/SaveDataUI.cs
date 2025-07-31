using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class SaveDataUI : MonoBehaviour
{
    [Header("UI引用")]
    public SaveData saveData; // 存档数据管理器
    
    [Header("存档槽位UI")]
    public Button[] saveSlotButtons; // 存档槽位按钮数组
    public TextMeshProUGUI[] slotInfoTexts; // 槽位信息文本数组
    public Button saveButton; // 保存按钮
    public Button loadButton; // 加载按钮
    public Button deleteButton; // 删除按钮
    
    [Header("当前槽位显示")]
    public TextMeshProUGUI currentSlotText; // 当前槽位显示文本
    
    private void Start()
    {
        // 如果没有指定SaveData，尝试在场景中查找
        if (saveData == null)
        {
            saveData = FindObjectOfType<SaveData>();
        }
        
        // 设置按钮监听器
        SetupButtonListeners();
        
        // 更新UI显示
        UpdateUI();
    }

    private void SetupButtonListeners()
    {
        // 设置存档槽位按钮监听器
        for (int i = 0; i < saveSlotButtons.Length; i++)
        {
            int slotNumber = i + 1; // 槽位从1开始
            saveSlotButtons[i].onClick.AddListener(() => OnSlotButtonClicked(slotNumber));
        }
        
        // 设置功能按钮监听器
        if (saveButton != null)
            saveButton.onClick.AddListener(OnSaveButtonClicked);
            
        if (loadButton != null)
            loadButton.onClick.AddListener(OnLoadButtonClicked);
            
        if (deleteButton != null)
            deleteButton.onClick.AddListener(OnDeleteButtonClicked);
    }

    // 槽位按钮点击事件
    private void OnSlotButtonClicked(int slotNumber)
    {
        if (saveData != null)
        {
            saveData.SwitchSaveSlot(slotNumber);
            UpdateUI();
        }
    }

    // 保存按钮点击事件
    private void OnSaveButtonClicked()
    {
        if (saveData != null)
        {
            saveData.SaveCurrentSlot();
            UpdateUI(); // 更新UI显示
        }
    }

    // 加载按钮点击事件
    private void OnLoadButtonClicked()
    {
        if (saveData != null)
        {
            saveData.LoadCurrentSlot();
        }
    }

    // 删除按钮点击事件
    private void OnDeleteButtonClicked()
    {
        if (saveData != null)
        {
            int currentSlot = saveData.GetCurrentSlot();
            saveData.DeleteSave(currentSlot);
            UpdateUI(); // 更新UI显示
        }
    }

    // 更新UI显示
    public void UpdateUI()
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
        int currentSlot = saveData.GetCurrentSlot();
        bool hasSave = saveData.HasSave(currentSlot);

        // 加载和删除按钮只在有存档时可用
        if (loadButton != null)
            loadButton.interactable = hasSave;
            
        if (deleteButton != null)
            deleteButton.interactable = hasSave;
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

    // 公共方法：保存到指定槽位
    public void SaveToSlot(int slotNumber)
    {
        if (saveData != null)
        {
            saveData.SavePlayer(slotNumber);
            UpdateUI();
        }
    }

    // 公共方法：从指定槽位加载
    public void LoadFromSlot(int slotNumber)
    {
        if (saveData != null)
        {
            saveData.LoadPlayer(slotNumber);
        }
    }
} 