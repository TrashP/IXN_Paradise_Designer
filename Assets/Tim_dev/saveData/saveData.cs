using System.IO;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class PlayerSaveData
{
    public Vector3 position;
    public string saveName;
    public System.DateTime saveTime;
}

public class SaveData : MonoBehaviour
{
    [Header("存档设置")]
    public int currentSaveSlot = 1; // 当前选中的存档槽位
    public int maxSaveSlots = 3; // 最大存档槽位数
    
    private string saveFolderPath;
    private string currentSavePath;

    private void Start()
    {
        // 创建保存目录（位于项目根目录的 Saves 文件夹）
        saveFolderPath = Application.dataPath + "/../Saves";
        if (!Directory.Exists(saveFolderPath))
        {
            Directory.CreateDirectory(saveFolderPath);
        }

        UpdateCurrentSavePath();
    }

    private void Update()
    {
        // 存档槽位切换（保留键盘切换功能，可选）
        if (Input.GetKeyDown(KeyCode.Alpha1)) SwitchSaveSlot(1);
        if (Input.GetKeyDown(KeyCode.Alpha2)) SwitchSaveSlot(2);
        if (Input.GetKeyDown(KeyCode.Alpha3)) SwitchSaveSlot(3);
    }

    // 切换存档槽位
    public void SwitchSaveSlot(int slotNumber)
    {
        if (slotNumber >= 1 && slotNumber <= maxSaveSlots)
        {
            currentSaveSlot = slotNumber;
            UpdateCurrentSavePath();
            Debug.Log($"切换到存档槽位 {currentSaveSlot}");
        }
    }

    // 更新当前存档路径
    private void UpdateCurrentSavePath()
    {
        currentSavePath = Path.Combine(saveFolderPath, $"save_slot_{currentSaveSlot}.json");
    }

    // 保存玩家数据到当前槽位（供UI按钮调用）
    public void SaveCurrentSlot()
    {
        SavePlayer(currentSaveSlot);
    }

    // 从当前槽位加载玩家数据（供UI按钮调用）
    public void LoadCurrentSlot()
    {
        LoadPlayer(currentSaveSlot);
    }

    // 保存玩家数据到指定槽位
    public void SavePlayer(int slotNumber)
    {
        if (slotNumber < 1 || slotNumber > maxSaveSlots)
        {
            Debug.LogError($"存档槽位 {slotNumber} 超出范围 (1-{maxSaveSlots})");
            return;
        }

        string savePath = Path.Combine(saveFolderPath, $"save_slot_{slotNumber}.json");
        
        PlayerSaveData data = new PlayerSaveData
        {
            position = transform.position,
            saveName = $"存档 {slotNumber}",
            saveTime = System.DateTime.Now
        };

        string json = JsonUtility.ToJson(data, true);
        File.WriteAllText(savePath, json);

        Debug.Log($"玩家数据已保存到槽位 {slotNumber}: {savePath}");
    }

    // 从指定槽位加载玩家数据
    public void LoadPlayer(int slotNumber)
    {
        if (slotNumber < 1 || slotNumber > maxSaveSlots)
        {
            Debug.LogError($"存档槽位 {slotNumber} 超出范围 (1-{maxSaveSlots})");
            return;
        }

        string savePath = Path.Combine(saveFolderPath, $"save_slot_{slotNumber}.json");
        
        if (!File.Exists(savePath))
        {
            Debug.LogWarning($"槽位 {slotNumber} 的存档文件不存在: {savePath}");
            return;
        }

        string json = File.ReadAllText(savePath);
        PlayerSaveData data = JsonUtility.FromJson<PlayerSaveData>(json);

        transform.position = data.position;

        Debug.Log($"从槽位 {slotNumber} 加载玩家数据: {savePath}");
        Debug.Log($"存档时间: {data.saveTime}");
    }

    // 获取所有存档信息
    public List<PlayerSaveData> GetAllSaveData()
    {
        List<PlayerSaveData> saveDataList = new List<PlayerSaveData>();
        
        for (int i = 1; i <= maxSaveSlots; i++)
        {
            string savePath = Path.Combine(saveFolderPath, $"save_slot_{i}.json");
            if (File.Exists(savePath))
            {
                string json = File.ReadAllText(savePath);
                PlayerSaveData data = JsonUtility.FromJson<PlayerSaveData>(json);
                saveDataList.Add(data);
            }
        }
        
        return saveDataList;
    }

    // 删除指定槽位的存档
    public void DeleteSave(int slotNumber)
    {
        if (slotNumber < 1 || slotNumber > maxSaveSlots)
        {
            Debug.LogError($"存档槽位 {slotNumber} 超出范围 (1-{maxSaveSlots})");
            return;
        }

        string savePath = Path.Combine(saveFolderPath, $"save_slot_{slotNumber}.json");
        
        if (File.Exists(savePath))
        {
            File.Delete(savePath);
            Debug.Log($"已删除槽位 {slotNumber} 的存档");
        }
        else
        {
            Debug.LogWarning($"槽位 {slotNumber} 的存档文件不存在");
        }
    }

    // 检查指定槽位是否有存档
    public bool HasSave(int slotNumber)
    {
        if (slotNumber < 1 || slotNumber > maxSaveSlots)
            return false;
            
        string savePath = Path.Combine(saveFolderPath, $"save_slot_{slotNumber}.json");
        return File.Exists(savePath);
    }

    // 获取存档信息（用于UI显示）
    public PlayerSaveData GetSaveInfo(int slotNumber)
    {
        if (slotNumber < 1 || slotNumber > maxSaveSlots)
            return null;
            
        string savePath = Path.Combine(saveFolderPath, $"save_slot_{slotNumber}.json");
        
        if (File.Exists(savePath))
        {
            string json = File.ReadAllText(savePath);
            return JsonUtility.FromJson<PlayerSaveData>(json);
        }
        
        return null;
    }

    // 获取当前槽位号（供UI显示）
    public int GetCurrentSlot()
    {
        return currentSaveSlot;
    }
}
