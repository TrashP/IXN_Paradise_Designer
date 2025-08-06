using UnityEngine;
using System.IO;
using System.Collections.Generic;

public class MapGenerator : MonoBehaviour
{
    [Header("Prefab Settings")]
    public GameObject[] forestPrefabs;
    public GameObject sandPrefab;
    public GameObject mountainPrefab;
    public GameObject islandPrefab;
    public GameObject oceanPrefab;

    [Header("Player Settings")]
    public GameObject playerPrefab;
    public float playerSpawnHeight = 150f;

    [Header("NPC Settings")]
    public GameObject npcPrefab;
    public float npcYOffset = 80f;


    [Header("Block & Placement Settings")]
    public int blockSize = 32;
    public float worldUnitPerBlock = 100f;
    public float forestYOffset = 60f;
    public float sandYOffset = 80f;
    public float mountainYOffset = 60f;
    public float oceanSizeMultiplier = 3f;

    [Header("Color Thresholds")]
    public float greenThreshold = 0.4f;
    public float sandThreshold = 0.4f;
    public float grayThreshold = 0.4f;

    private Texture2D texture;
    private Vector3 islandCenter;
    private Vector3 islandMinBound;
    private Vector3 islandMaxBound;

    void Start()
    {
        string latestImagePath = GetLatestImagePath();
        if (string.IsNullOrEmpty(latestImagePath))
        {
            Debug.LogError("❌ cannot find the image！");
            return;
        }

        byte[] fileData = File.ReadAllBytes(latestImagePath);
        texture = new Texture2D(2, 2);
        texture.LoadImage(fileData);

        GenerateIsland(texture.width, texture.height);
        SpawnTerrainFromImage();
        SpawnPlayer();
    }

    void GenerateIsland(int texWidth, int texHeight)
    {
        int blocksX = texWidth / blockSize;
        int blocksY = texHeight / blockSize;

        float totalWidth = blocksX * worldUnitPerBlock;
        float totalHeight = blocksY * worldUnitPerBlock;

        islandCenter = new Vector3(totalWidth / 2f, 0f, totalHeight / 2f);
        islandMinBound = new Vector3(0, 0, 0);
        islandMaxBound = new Vector3(totalWidth, 0, totalHeight);

        // ✅ Ocean
        if (oceanPrefab != null)
        {
            float oceanWidth = totalWidth * oceanSizeMultiplier;
            float oceanHeight = totalHeight * oceanSizeMultiplier;
            Vector3 oceanPos = islandCenter + Vector3.down * 2f;
            GameObject ocean = Instantiate(oceanPrefab, oceanPos, Quaternion.identity);
            ocean.transform.localScale = new Vector3(oceanWidth, 1f, oceanHeight);
        }

        // ✅ Island
        if (islandPrefab != null)
        {
            Instantiate(islandPrefab, islandCenter, Quaternion.identity);
        }
        else
        {
            Debug.LogWarning("⚠️ islandPrefab is not assigned！");
        }
    }

    void SpawnTerrainFromImage()
    {
        int texWidth = texture.width;
        int texHeight = texture.height;

        int blocksX = texWidth / blockSize;
        int blocksY = texHeight / blockSize;

        Vector3? npcSpawnPoint = null;

        for (int y = 0; y < blocksY; y++)
        {
            for (int x = 0; x < blocksX; x++)
            {
                Color avgColor = GetAverageColor(x, y);
                Vector3 basePos = new Vector3(
                    x * worldUnitPerBlock + worldUnitPerBlock / 2f,
                    islandCenter.y,
                    y * worldUnitPerBlock + worldUnitPerBlock / 2f
                );

                if (!IsInsideIsland(basePos)) continue;

                if (IsGreen(avgColor) && forestPrefabs != null && forestPrefabs.Length > 0)
                {
                    int randomIndex = Random.Range(0, forestPrefabs.Length);
                    GameObject prefab = forestPrefabs[randomIndex];
                    Vector3 pos = basePos + Vector3.up * forestYOffset;
                    Instantiate(prefab, pos, Quaternion.identity);

                    if (npcSpawnPoint == null)
                    {
                        npcSpawnPoint = basePos;
                    }
                }
                else if (IsYellow(avgColor) && sandPrefab != null)
                {
                    Vector3 pos = basePos + Vector3.up * sandYOffset;
                    Instantiate(sandPrefab, pos, Quaternion.identity);
                }
                else if (IsGray(avgColor) && mountainPrefab != null)
                {
                    Vector3 pos = basePos + Vector3.up * mountainYOffset;
                    Instantiate(mountainPrefab, pos, Quaternion.identity);
                }
            }
        }
        if (npcSpawnPoint != null && npcPrefab != null)
        {
            Vector3 npcPos = npcSpawnPoint.Value + Vector3.up * npcYOffset;
            Instantiate(npcPrefab, npcPos, Quaternion.identity);
        }
    }

    void SpawnPlayer()
    {
        if (playerPrefab != null)
        {
            Vector3 spawnPos = new Vector3(islandCenter.x, playerSpawnHeight, islandCenter.z);
            Instantiate(playerPrefab, spawnPos, Quaternion.identity);
        }
        else
        {
            Debug.LogWarning("⚠️ Player prefab not assigned in Inspector!");
        }
    }

    Color GetAverageColor(int blockX, int blockY)
    {
        int startX = blockX * blockSize;
        int startY = blockY * blockSize;

        Color avg = new Color(0, 0, 0);
        int total = 0;

        for (int dx = 0; dx < blockSize; dx++)
        {
            for (int dy = 0; dy < blockSize; dy++)
            {
                Color c = texture.GetPixel(startX + dx, startY + dy);
                avg += c;
                total++;
            }
        }

        return avg / total;
    }

    bool IsGreen(Color color) => color.g > greenThreshold && color.g > color.r && color.g > color.b;
    bool IsYellow(Color color) => color.r > 0.8f && color.g > 0.6f && color.b < 0.3f;
    bool IsGray(Color color) => Mathf.Abs(color.r - color.g) < 0.1f && Mathf.Abs(color.g - color.b) < 0.1f && color.r < 0.6f;

    bool IsInsideIsland(Vector3 pos)
    {
        return pos.x >= islandMinBound.x && pos.x <= islandMaxBound.x
            && pos.z >= islandMinBound.z && pos.z <= islandMaxBound.z;
    }

    string GetLatestImagePath()
    {
        string folderPath = Path.Combine(Application.persistentDataPath, "SavedImage");
        if (!Directory.Exists(folderPath)) return null;

        DirectoryInfo dir = new DirectoryInfo(folderPath);
        FileInfo[] files = dir.GetFiles("*.png");
        if (files.Length == 0) return null;

        FileInfo latestFile = files[0];
        foreach (var file in files)
        {
            if (file.LastWriteTime > latestFile.LastWriteTime)
            {
                latestFile = file;
            }
        }

        return latestFile.FullName;
    }
}
