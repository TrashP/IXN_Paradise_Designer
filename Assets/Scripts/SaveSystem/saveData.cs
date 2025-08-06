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
    public string saveTimeString; // 改为字符串格式 save as string format
}

public static class SaveDataManager
{
    [Header("存档设置")]
    private static int maxSaveSlots = 3; // 最大存档槽位数 max save slots
    private static string saveFolderPath;

    // 初始化存档系统 initialize save system
    public static void Initialize()
    {
        // 创建保存目录（位于项目根目录的 Saves 文件夹） create save directory (in the Saves folder at the root of the project)
        saveFolderPath = Application.dataPath + "/../Saves";
        if (!Directory.Exists(saveFolderPath))
        {
            Directory.CreateDirectory(saveFolderPath);
        }
    }

    // 保存玩家数据到指定槽位 save player data to specified slot
    public static void SavePlayer(int slotNumber, Vector3 playerPosition, Quaternion playerRotation, Vector3 playerScale)
    {
        if (slotNumber < 1 || slotNumber > maxSaveSlots)
        {
            Debug.LogError($"存档槽位 {slotNumber} 超出范围 (1-{maxSaveSlots}) save slot {slotNumber} out of range (1-{maxSaveSlots})");
            return;
        }

        // 确保初始化 ensure initialization
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
            saveTimeString = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") // 保存为字符串 save as string
        };

        string json = JsonUtility.ToJson(data, true);
        File.WriteAllText(savePath, json);

        Debug.Log($"玩家数据已保存到槽位 {slotNumber}: {savePath} player data saved to slot {slotNumber}: {savePath}");
    }

    // 重载方法：保持向后兼容性，只保存位置 keep backward compatibility, only save position
    public static void SavePlayer(int slotNumber, Vector3 playerPosition)
    {
        SavePlayer(slotNumber, playerPosition, Quaternion.identity, Vector3.one);
    }

    // 从指定槽位加载玩家数据 load player data from specified slot
    public static PlayerSaveData LoadPlayer(int slotNumber)
    {
        if (slotNumber < 1 || slotNumber > maxSaveSlots)
        {
            Debug.LogError($"存档槽位 {slotNumber} 超出范围 (1-{maxSaveSlots}) save slot {slotNumber} out of range (1-{maxSaveSlots})");
            return null;
        }

        // 确保初始化 ensure initialization
        if (string.IsNullOrEmpty(saveFolderPath))
        {
            Initialize();
        }

        string savePath = Path.Combine(saveFolderPath, $"save_slot_{slotNumber}.json");
        
        if (!File.Exists(savePath))
        {
            Debug.LogWarning($"槽位 {slotNumber} 的存档文件不存在: {savePath} save file for slot {slotNumber} does not exist: {savePath}");
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
            Debug.Log($"检测到旧格式存档，已设置默认的rotation和scale值 old format save detected, default rotation and scale values set");
        }

        Debug.Log($"从槽位 {slotNumber} 加载玩家数据: {savePath} load player data from slot {slotNumber}: {savePath}");
        Debug.Log($"存档时间: {data.saveTimeString} save time: {data.saveTimeString}");
        Debug.Log($"Transform数据: 位置={data.position}, 旋转={data.rotation}, 缩放={data.scale} transform data: position={data.position}, rotation={data.rotation}, scale={data.scale}");
        
        return data;
    }

    // 获取所有存档信息 get all save data
    public static List<PlayerSaveData> GetAllSaveData()
    {
        // 确保初始化 ensure initialization
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
                
                // 检查是否为旧格式存档（缺少rotation和scale字段） check if it is an old format save (missing rotation and scale fields)
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

    // 删除指定槽位的存档 delete save for specified slot
    public static void DeleteSave(int slotNumber)
    {
        if (slotNumber < 1 || slotNumber > maxSaveSlots)
        {
            Debug.LogError($"存档槽位 {slotNumber} 超出范围 (1-{maxSaveSlots}) save slot {slotNumber} out of range (1-{maxSaveSlots})");
            return;
        }

        // 确保初始化 ensure initialization
        if (string.IsNullOrEmpty(saveFolderPath))
        {
            Initialize();
        }

        string savePath = Path.Combine(saveFolderPath, $"save_slot_{slotNumber}.json");
        
        if (File.Exists(savePath))
        {
            File.Delete(savePath);
            Debug.Log($"已删除槽位 {slotNumber} 的存档 delete save for slot {slotNumber}");
        }
        else
        {
            Debug.LogWarning($"槽位 {slotNumber} 的存档文件不存在 save file for slot {slotNumber} does not exist");
        }
    }

    // 检查指定槽位是否有存档 check if there is a save for specified slot
    public static bool HasSave(int slotNumber)
    {
        if (slotNumber < 1 || slotNumber > maxSaveSlots)
            return false;
            
        // 确保初始化 ensure initialization
        if (string.IsNullOrEmpty(saveFolderPath))
        {
            Initialize();
        }
            
        string savePath = Path.Combine(saveFolderPath, $"save_slot_{slotNumber}.json");
        return File.Exists(savePath);
    }

    // 获取存档信息（用于UI显示） get save info (for UI display)
    public static PlayerSaveData GetSaveInfo(int slotNumber)
    {
        if (slotNumber < 1 || slotNumber > maxSaveSlots)
            return null;
            
        // 确保初始化 ensure initialization
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

                data.rotation = Quaternion.identity;
                data.scale = Vector3.one;
            }
            
            return data;
        }
        
        return null;
    }

    // 获取最大槽位数 get max save slots
    public static int GetMaxSaveSlots()
    {
        return maxSaveSlots;
    }

    // 设置最大槽位数 set max save slots
    public static void SetMaxSaveSlots(int maxSlots)
    {
        maxSaveSlots = maxSlots;
    }
}
