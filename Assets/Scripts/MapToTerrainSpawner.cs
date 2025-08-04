using UnityEngine;
using System.IO;
using System.Linq;
using System.Collections.Generic;

public class MapToTerrainSpawner : MonoBehaviour
{
    [Header("Prefab Settings")]
    public GameObject[] forestPrefabs;
    public GameObject sandPrefab;
    public GameObject beachPrefab;
    public GameObject[] grasslandPrefabs;
    public GameObject pondPrefab;
    public GameObject mountainPrefab;

    [Header("Drawing Map Settings")]
    private string folderPath;
    public int blockSize = 32;
    public float worldUnitPerBlock = 200f;

    [Header("Color Thresholds")]
    public float greenThreshold = 0.4f;
    public float sandThreshold = 0.4f;
    public float pondThreshold = 0.3f;

    [Header("Prefab Scale Adjustment")]
    public float prefabOriginalSize = 50f;

    [Header("Debug Options")]
    public bool logDebug = true;

    [Header("Player Settings")]
    public GameObject playerPrefab;

    [Header("Ocean Settings")]
    public GameObject oceanPrefab;

    private Transform terrainParent;

    private bool[,] isPondBlock;
    private bool[,] visitedPond;
    private bool[,] isMountainBlock;
    private bool[,] visitedMountain;

    enum TerrainType { Forest, Sand, Grass }

    void Start()
    {
        folderPath = Path.Combine(Application.persistentDataPath, "SavedImage");
        string imagePath = GetLatestImagePath(folderPath);
        if (string.IsNullOrEmpty(imagePath) || !File.Exists(imagePath))
        {
            Debug.LogError("❌ Cannot find image: " + imagePath);
            return;
        }

        byte[] data = File.ReadAllBytes(imagePath);
        Texture2D tex = new Texture2D(2, 2);
        tex.LoadImage(data);
        Debug.Log($"✅ Image loaded: {tex.width} x {tex.height}");

        terrainParent = new GameObject("GeneratedTerrain").transform;

        int blocksX = tex.width / blockSize;
        int blocksY = tex.height / blockSize;
        isPondBlock = new bool[blocksX, blocksY];
        visitedPond = new bool[blocksX, blocksY];
        isMountainBlock = new bool[blocksX, blocksY];
        visitedMountain = new bool[blocksX, blocksY];

        for (int y = 0; y < blocksY; y++)
        {
            for (int x = 0; x < blocksX; x++)
            {
                Color avgColor = GetAverageBlockColor(tex, x * blockSize, y * blockSize, blockSize);
                isPondBlock[x, y] = IsPixelBlue(avgColor);
                isMountainBlock[x, y] = IsPixelGray(avgColor);
            }
        }

        for (int y = 0; y < blocksY; y++)
        {
            for (int x = 0; x < blocksX; x++)
            {
                if (isPondBlock[x, y] && !visitedPond[x, y])
                {
                    List<Vector2Int> region = new List<Vector2Int>();
                    FloodFill(x, y, blocksX, blocksY, isPondBlock, visitedPond, region);
                    PlacePrefabGroup(region, pondPrefab, new Vector3(0f, -1.5f, 0f));
                }
            }
        }

        for (int y = 0; y < blocksY; y++)
        {
            for (int x = 0; x < blocksX; x++)
            {
                if (isMountainBlock[x, y] && !visitedMountain[x, y])
                {
                    List<Vector2Int> region = new List<Vector2Int>();
                    FloodFill(x, y, blocksX, blocksY, isMountainBlock, visitedMountain, region);
                    PlacePrefabGroup(region, mountainPrefab, Vector3.zero);
                }
            }
        }

        float scaleFactor = worldUnitPerBlock / prefabOriginalSize;

        for (int y = 0; y < blocksY; y++)
        {
            for (int x = 0; x < blocksX; x++)
            {
                if (isPondBlock[x, y] || isMountainBlock[x, y]) continue;

                Vector3 worldPos = new Vector3(
                    x * worldUnitPerBlock + worldUnitPerBlock / 2f,
                    0f,
                    y * worldUnitPerBlock + worldUnitPerBlock / 2f
                );

                GameObject go = null;
                TerrainType type = GetTerrainType(tex, x * blockSize, y * blockSize, blockSize);
                switch (type)
                {
                    case TerrainType.Forest:
                        if (forestPrefabs.Length > 0)
                            go = Instantiate(forestPrefabs[Random.Range(0, forestPrefabs.Length)], worldPos, Quaternion.identity, terrainParent);
                        break;
                    case TerrainType.Sand:
                        go = Instantiate(sandPrefab, worldPos, Quaternion.identity, terrainParent);
                        break;
                    case TerrainType.Grass:
                    default:
                        if (grasslandPrefabs.Length > 0)
                            go = Instantiate(grasslandPrefabs[Random.Range(0, grasslandPrefabs.Length)], worldPos, Quaternion.identity, terrainParent);
                        break;
                }

                if (go != null)
                    go.transform.localScale = Vector3.one * scaleFactor;
            }
        }

        SpawnPlayer(blocksX, blocksY);
        SurroundWithBeachAndOcean(blocksX, blocksY);

        Debug.Log("✅ Terrain generation completed.");
    }

    void FloodFill(int startX, int startY, int maxX, int maxY, bool[,] mask, bool[,] visited, List<Vector2Int> region)
    {
        Queue<Vector2Int> queue = new Queue<Vector2Int>();
        queue.Enqueue(new Vector2Int(startX, startY));
        visited[startX, startY] = true;

        int[] dx = { 0, 1, 0, -1 };
        int[] dy = { 1, 0, -1, 0 };

        while (queue.Count > 0)
        {
            Vector2Int current = queue.Dequeue();
            region.Add(current);

            for (int i = 0; i < 4; i++)
            {
                int nx = current.x + dx[i];
                int ny = current.y + dy[i];

                if (nx >= 0 && nx < maxX && ny >= 0 && ny < maxY)
                {
                    if (mask[nx, ny] && !visited[nx, ny])
                    {
                        visited[nx, ny] = true;
                        queue.Enqueue(new Vector2Int(nx, ny));
                    }
                }
            }
        }
    }

    void PlacePrefabGroup(List<Vector2Int> region, GameObject prefab, Vector3 positionOffset)
    {
        if (prefab == null || region.Count == 0) return;

        int minX = region.Min(p => p.x);
        int maxX = region.Max(p => p.x);
        int minY = region.Min(p => p.y);
        int maxY = region.Max(p => p.y);

        float width = (maxX - minX + 1) * worldUnitPerBlock;
        float depth = (maxY - minY + 1) * worldUnitPerBlock;
        Vector3 bottomLeft = new Vector3(minX * worldUnitPerBlock, 0f, minY * worldUnitPerBlock);

        GameObject go = Instantiate(prefab, Vector3.zero, Quaternion.identity, terrainParent);
        MeshFilter mf = go.GetComponentInChildren<MeshFilter>();
        if (mf == null || mf.sharedMesh == null)
        {
            Debug.LogError("❌ Prefab lacks mesh");
            return;
        }

        Bounds bounds = mf.sharedMesh.bounds;
        Vector3 meshSize = bounds.size;
        Vector3 meshCenter = bounds.center;

        float scaleX = width / meshSize.x;
        float scaleZ = depth / meshSize.z;

        go.transform.localScale = new Vector3(
            go.transform.localScale.x * scaleX,
            go.transform.localScale.y,
            go.transform.localScale.z * scaleZ
        );

        Vector3 worldOffset = Vector3.Scale(meshCenter - new Vector3(meshSize.x / 2f, 0f, meshSize.z / 2f), go.transform.localScale);
        go.transform.position = bottomLeft - worldOffset + positionOffset;
    }

    void SurroundWithBeachAndOcean(int blocksX, int blocksY)
    {
        if (beachPrefab == null || oceanPrefab == null) return;

        float unit = worldUnitPerBlock;
        float offset = unit / 2f;

        int beachRingMinX = -1;
        int beachRingMaxX = blocksX;
        int beachRingMinY = -1;
        int beachRingMaxY = blocksY;

        for (int x = beachRingMinX; x <= beachRingMaxX; x++)
        {
            for (int y = beachRingMinY; y <= beachRingMaxY; y++)
            {
                if (x == beachRingMinX || x == beachRingMaxX || y == beachRingMinY || y == beachRingMaxY)
                {
                    float posX = x * unit + offset;
                    float posZ = y * unit + offset;
                    Instantiate(beachPrefab, new Vector3(posX, 0f, posZ), Quaternion.identity, terrainParent);
                }
            }
        }

        for (int x = beachRingMinX - 1; x <= beachRingMaxX + 1; x++)
        {
            for (int y = beachRingMinY - 1; y <= beachRingMaxY + 1; y++)
            {
                bool isOuterRing = (x == beachRingMinX - 1 || x == beachRingMaxX + 1 || y == beachRingMinY - 1 || y == beachRingMaxY + 1);
                if (isOuterRing)
                {
                    float posX = x * unit + offset;
                    float posZ = y * unit + offset;
                    Instantiate(oceanPrefab, new Vector3(posX, 0f, posZ), Quaternion.identity, terrainParent);
                }
            }
        }
    }

    void SpawnPlayer(int blocksX, int blocksY)
    {
        if (playerPrefab == null) return;

        float centerX = (blocksX / 2f) * worldUnitPerBlock;
        float centerZ = (blocksY / 2f) * worldUnitPerBlock;
        Vector3 origin = new Vector3(centerX, 100f, centerZ);

        if (Physics.Raycast(origin, Vector3.down, out RaycastHit hit, 200f))
            Instantiate(playerPrefab, hit.point + Vector3.up * 2f, Quaternion.identity);
        else
            Instantiate(playerPrefab, new Vector3(centerX, 2f, centerZ), Quaternion.identity);
    }

    TerrainType GetTerrainType(Texture2D tex, int startX, int startY, int size)
    {
        int forest = 0, sand = 0, total = 0;
        for (int y = 0; y < size; y++)
            for (int x = 0; x < size; x++)
            {
                int px = startX + x, py = startY + y;
                if (px >= tex.width || py >= tex.height) continue;

                Color c = tex.GetPixel(px, py);
                if (IsPixelGreen(c)) forest++;
                else if (IsPixelSand(c)) sand++;
                total++;
            }

        float forestRatio = forest / (float)total;
        float sandRatio = sand / (float)total;

        if (forestRatio > greenThreshold) return TerrainType.Forest;
        if (sandRatio > sandThreshold) return TerrainType.Sand;

        return TerrainType.Grass;
    }

    Color GetAverageBlockColor(Texture2D tex, int startX, int startY, int size)
    {
        Color sum = Color.black;
        int count = 0;

        for (int y = 0; y < size; y++)
            for (int x = 0; x < size; x++)
            {
                int px = startX + x;
                int py = startY + y;
                if (px >= tex.width || py >= tex.height) continue;

                sum += tex.GetPixel(px, py);
                count++;
            }

        return (count > 0) ? sum / count : Color.black;
    }

    bool IsPixelGreen(Color color)
    {
        Color.RGBToHSV(color, out float h, out float s, out float v);
        return h >= 0.25f && h <= 0.45f && s > 0.3f && v > 0.3f;
    }

    bool IsPixelSand(Color color)
    {
        Color.RGBToHSV(color, out float h, out float s, out float v);
        return h >= 0.10f && h <= 0.17f && s > 0.4f && v > 0.4f;
    }

    bool IsPixelBlue(Color color)
    {
        Color.RGBToHSV(color, out float h, out float s, out float v);
        return h >= 0.50f && h <= 0.70f && s > 0.4f && v > 0.4f;
    }

    bool IsPixelGray(Color color)
    {
        Color.RGBToHSV(color, out float h, out float s, out float v);
        return s < 0.2f && v > 0.2f && v < 0.8f;
    }

    string GetLatestImagePath(string folderPath)
    {
        if (!Directory.Exists(folderPath)) return null;
        var files = Directory.GetFiles(folderPath, "*.png");
        if (files.Length == 0) return null;
        return files.OrderByDescending(File.GetLastWriteTime).First();
    }
}
