using System.IO;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class PlayerSaveData
{
    public Vector3 position;
    public Quaternion rotation;
    public Vector3 scale;
    public string saveName;
    public string saveTimeString; // 改为字符串格式
}

public static class SaveDataManager
{
    [Header("存档设置")]
    private static int maxSaveSlots = 3; // 最大存档槽位数
    private static string saveFolderPath;

    // 初始化存档系统
    public static void Initialize()
    {
        // 创建保存目录（位于项目根目录的 Saves 文件夹）
        saveFolderPath = Application.dataPath + "/../Saves";
        if (!Directory.Exists(saveFolderPath))
        {
            Directory.CreateDirectory(saveFolderPath);
        }
    }

    // 保存玩家数据到指定槽位
    public static void SavePlayer(int slotNumber, Vector3 playerPosition, Quaternion playerRotation, Vector3 playerScale)
    {
        if (slotNumber < 1 || slotNumber > maxSaveSlots)
        {
            Debug.LogError($"存档槽位 {slotNumber} 超出范围 (1-{maxSaveSlots})");
            return;
        }

        // 确保初始化
        if (string.IsNullOrEmpty(saveFolderPath))
        {
            Initialize();
        }

        string savePath = Path.Combine(saveFolderPath, $"save_slot_{slotNumber}.json");
        
        PlayerSaveData data = new PlayerSaveData
        {
            position = playerPosition,
            rotation = playerRotation,
            scale = playerScale,
            saveName = $"Checkpoint {slotNumber}",
            saveTimeString = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") // 保存为字符串
        };

        string json = JsonUtility.ToJson(data, true);
        File.WriteAllText(savePath, json);

        Debug.Log($"玩家数据已保存到槽位 {slotNumber}: {savePath}");
    }

    // 重载方法：保持向后兼容性，只保存位置
    public static void SavePlayer(int slotNumber, Vector3 playerPosition)
    {
        SavePlayer(slotNumber, playerPosition, Quaternion.identity, Vector3.one);
    }

    // 从指定槽位加载玩家数据
    public static PlayerSaveData LoadPlayer(int slotNumber)
    {
        if (slotNumber < 1 || slotNumber > maxSaveSlots)
        {
            Debug.LogError($"存档槽位 {slotNumber} 超出范围 (1-{maxSaveSlots})");
            return null;
        }

        // 确保初始化
        if (string.IsNullOrEmpty(saveFolderPath))
        {
            Initialize();
        }

        string savePath = Path.Combine(saveFolderPath, $"save_slot_{slotNumber}.json");
        
        if (!File.Exists(savePath))
        {
            Debug.LogWarning($"槽位 {slotNumber} 的存档文件不存在: {savePath}");
            return null;
        }

        string json = File.ReadAllText(savePath);
        PlayerSaveData data = JsonUtility.FromJson<PlayerSaveData>(json);

        // 检查是否为旧格式存档（缺少rotation和scale字段）
        if (data.rotation == Quaternion.identity && data.scale == Vector3.zero)
        {
            // 可能是旧格式，设置默认值
            data.rotation = Quaternion.identity;
            data.scale = Vector3.one;
            Debug.Log($"检测到旧格式存档，已设置默认的rotation和scale值");
        }

        Debug.Log($"从槽位 {slotNumber} 加载玩家数据: {savePath}");
        Debug.Log($"存档时间: {data.saveTimeString}");
        Debug.Log($"Transform数据: 位置={data.position}, 旋转={data.rotation}, 缩放={data.scale}");
        
        return data;
    }

    // 获取所有存档信息
    public static List<PlayerSaveData> GetAllSaveData()
    {
        // 确保初始化
        if (string.IsNullOrEmpty(saveFolderPath))
        {
            Initialize();
        }

        List<PlayerSaveData> saveDataList = new List<PlayerSaveData>();
        
        for (int i = 1; i <= maxSaveSlots; i++)
        {
            string savePath = Path.Combine(saveFolderPath, $"save_slot_{i}.json");
            if (File.Exists(savePath))
            {
                string json = File.ReadAllText(savePath);
                PlayerSaveData data = JsonUtility.FromJson<PlayerSaveData>(json);
                
                // 检查是否为旧格式存档（缺少rotation和scale字段）
                if (data.rotation == Quaternion.identity && data.scale == Vector3.zero)
                {
                    // 可能是旧格式，设置默认值
                    data.rotation = Quaternion.identity;
                    data.scale = Vector3.one;
                }
                
                saveDataList.Add(data);
            }
        }
        
        return saveDataList;
    }

    // 删除指定槽位的存档
    public static void DeleteSave(int slotNumber)
    {
        if (slotNumber < 1 || slotNumber > maxSaveSlots)
        {
            Debug.LogError($"存档槽位 {slotNumber} 超出范围 (1-{maxSaveSlots})");
            return;
        }

        // 确保初始化
        if (string.IsNullOrEmpty(saveFolderPath))
        {
            Initialize();
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
    public static bool HasSave(int slotNumber)
    {
        if (slotNumber < 1 || slotNumber > maxSaveSlots)
            return false;
            
        // 确保初始化
        if (string.IsNullOrEmpty(saveFolderPath))
        {
            Initialize();
        }
            
        string savePath = Path.Combine(saveFolderPath, $"save_slot_{slotNumber}.json");
        return File.Exists(savePath);
    }

    // 获取存档信息（用于UI显示）
    public static PlayerSaveData GetSaveInfo(int slotNumber)
    {
        if (slotNumber < 1 || slotNumber > maxSaveSlots)
            return null;
            
        // 确保初始化
        if (string.IsNullOrEmpty(saveFolderPath))
        {
            Initialize();
        }
            
        string savePath = Path.Combine(saveFolderPath, $"save_slot_{slotNumber}.json");
        
        if (File.Exists(savePath))
        {
            string json = File.ReadAllText(savePath);
            PlayerSaveData data = JsonUtility.FromJson<PlayerSaveData>(json);
            
            // 检查是否为旧格式存档（缺少rotation和scale字段）
            if (data.rotation == Quaternion.identity && data.scale == Vector3.zero)
            {
                // 可能是旧格式，设置默认值
                data.rotation = Quaternion.identity;
                data.scale = Vector3.one;
            }
            
            return data;
        }
        
        return null;
    }

    // 获取最大槽位数
    public static int GetMaxSaveSlots()
    {
        return maxSaveSlots;
    }

    // 设置最大槽位数
    public static void SetMaxSaveSlots(int maxSlots)
    {
        maxSaveSlots = maxSlots;
    }
}
