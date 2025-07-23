using UnityEngine;
using System.IO;
using System.Linq;
using System.Collections.Generic;

public class MapToTerrainSpawner : MonoBehaviour
{
    [Header("Prefab Settings")]
    public GameObject[] forestPrefabs;
    public GameObject beachPrefab;
    public GameObject[] grasslandPrefabs;
    public GameObject pondPrefab;

    [Header("Drawing Map Settings")]
    private string folderPath;
    public int blockSize = 32;
    public float worldUnitPerBlock = 100f;

    [Header("Color Thresholds")]
    public float greenThreshold = 0.4f;
    public float beachThreshold = 0.4f;
    public float pondThreshold = 0.3f;

    [Header("Prefab Scale Adjustment")]
    public float blockScaleFactor = 1f;

    [Header("Debug Options")]
    public bool logDebug = true;

    [Header("Player Settings")]
    public GameObject playerPrefab;

    [Header("Ocean Settings")]
    public GameObject oceanPrefab;
    public float oceanOffsetTop = 0.1f;
    public float oceanOffsetBottom = 0.1f;
    public float oceanOffsetLeft = 0.1f;
    public float oceanOffsetRight = 0.1f;

    private Transform terrainParent;
    private bool[,] isPondBlock;
    private bool[,] visited;

    private int blocksX;
    private int blocksY;


    enum TerrainType { Forest, Beach, Grass }

    void Start()
    {

        string folderPath = Path.Combine(Application.persistentDataPath, "SavedImage");
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

        blocksX = tex.width / blockSize;
        blocksY = tex.height / blockSize;
        isPondBlock = new bool[blocksX, blocksY];
        visited = new bool[blocksX, blocksY];


        // Step 1: target pond region
        for (int y = 0; y < blocksY; y++)
        {
            for (int x = 0; x < blocksX; x++)
            {
                float pondRatio = GetPondRatio(tex, x * blockSize, y * blockSize, blockSize);
                isPondBlock[x, y] = pondRatio > pondThreshold;
            }
        }

        // Step 2: flood fill and put pond prefab
        for (int y = 0; y < blocksY; y++)
        {
            for (int x = 0; x < blocksX; x++)
            {
                if (isPondBlock[x, y] && !visited[x, y])
                {
                    List<Vector2Int> pondRegion = new List<Vector2Int>();
                    FloodFillPond(x, y, blocksX, blocksY, pondRegion);
                    PlacePondGroup(pondRegion);
                }
            }
        }

        // Step 3: generate the other terrains(jump the pond)
        for (int y = 0; y < blocksY; y++)
        {
            for (int x = 0; x < blocksX; x++)
            {
                if (isPondBlock[x, y]) continue;

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
                        if (forestPrefabs != null && forestPrefabs.Length > 0)
                            go = Instantiate(forestPrefabs[Random.Range(0, forestPrefabs.Length)], worldPos, Quaternion.identity, terrainParent);
                        break;
                    case TerrainType.Beach:
                        go = Instantiate(beachPrefab, worldPos, Quaternion.identity, terrainParent);
                        break;
                    case TerrainType.Grass:
                    default:
                        if (grasslandPrefabs != null && grasslandPrefabs.Length > 0)
                            go = Instantiate(grasslandPrefabs[Random.Range(0, grasslandPrefabs.Length)], worldPos, Quaternion.identity, terrainParent);
                        break;
                }

                if (go != null)
                    go.transform.localScale = new Vector3(blockScaleFactor, 1f, blockScaleFactor);
            }
        }

        SpawnPlayer(blocksX, blocksY);
        SurroundWithOceanByBlockRange(blocksX, blocksY);
        Debug.Log("✅ Terrain generation completed with corrected pond logic!");
    }

    void FloodFillPond(int startX, int startY, int maxX, int maxY, List<Vector2Int> region)
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
                    if (isPondBlock[nx, ny] && !visited[nx, ny])
                    {
                        visited[nx, ny] = true;
                        queue.Enqueue(new Vector2Int(nx, ny));
                    }
                }
            }
        }
    }

    void PlacePondGroup(List<Vector2Int> region)
    {
        if (pondPrefab == null || region.Count == 0) return;

        int minX = region.Min(p => p.x);
        int maxX = region.Max(p => p.x);
        int minY = region.Min(p => p.y);
        int maxY = region.Max(p => p.y);

        float targetWidth = (maxX - minX + 1) * worldUnitPerBlock;
        float targetDepth = (maxY - minY + 1) * worldUnitPerBlock;

        // Calculate the bottom left corner of the target before generating.
        Vector3 targetBottomLeft = new Vector3(minX * worldUnitPerBlock, 0f, minY * worldUnitPerBlock);

        // Instantiate but do not scale. Calculate the lower left corner of the target before generating.
        GameObject pond = Instantiate(pondPrefab, Vector3.zero, Quaternion.identity, terrainParent);

        MeshFilter meshFilter = pond.GetComponentInChildren<MeshFilter>();
        if (meshFilter == null || meshFilter.sharedMesh == null)
        {
            Debug.LogError("❌ pondPrefab lacks valid MeshFilter");
            return;
        }

        //  Get the local dimensions and center point of the original mesh of the prefab.
        Bounds meshBounds = meshFilter.sharedMesh.bounds;
        Vector3 meshSize = meshBounds.size;
        Vector3 meshCenter = meshBounds.center;

        // The correct scaling factor
        float scaleX = targetWidth / meshSize.x;
        float scaleZ = targetDepth / meshSize.z;

        pond.transform.localScale = new Vector3(
            pond.transform.localScale.x * scaleX,
            pond.transform.localScale.y,
            pond.transform.localScale.z * scaleZ
        );

        // Mesh pivot offset (local space)
        Vector3 localOffset = meshCenter - new Vector3(meshSize.x / 2f, 0f, meshSize.z / 2f);

        // Transform the offset value in the world space (used for correcting the position)
        Vector3 worldOffset = Vector3.Scale(localOffset, pond.transform.localScale);

        // Set final position = Target lower left corner - pivot offset
        //pond.transform.position = targetBottomLeft - worldOffset;
        float pondYOffset = -1.5f; // based on the model
        pond.transform.position = targetBottomLeft - worldOffset + new Vector3(0f, pondYOffset, 0f);

        if (logDebug)
        {
            Debug.Log($"✅ Pond aligned at {pond.transform.position}, scale=({scaleX:F2}, {scaleZ:F2}), targetSize=({targetWidth}, {targetDepth})");
        }
    }



    float GetPondRatio(Texture2D tex, int startX, int startY, int size)
    {
        int pondCount = 0;
        int total = 0;

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                int px = startX + x;
                int py = startY + y;

                if (px >= tex.width || py >= tex.height) continue;

                Color c = tex.GetPixel(px, py);
                total++;
                if (IsPixelBlue(c)) pondCount++;
            }
        }

        return (total == 0) ? 0f : (pondCount / (float)total);
    }

    TerrainType GetTerrainType(Texture2D tex, int startX, int startY, int size)
    {
        int forestCount = 0;
        int beachCount = 0;
        int total = 0;

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                int px = startX + x;
                int py = startY + y;

                if (px >= tex.width || py >= tex.height) continue;

                Color c = tex.GetPixel(px, py);
                total++;

                if (IsPixelGreen(c)) forestCount++;
                else if (IsPixelSand(c)) beachCount++;
            }
        }

        if (total == 0) return TerrainType.Grass;

        float forestRatio = forestCount / (float)total;
        float beachRatio = beachCount / (float)total;

        if (forestRatio > greenThreshold) return TerrainType.Forest;
        if (beachRatio > beachThreshold) return TerrainType.Beach;

        return TerrainType.Grass;
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


    void SpawnPlayer(int blocksX, int blocksY)
    {
        if (playerPrefab == null) return;

        float centerX = (blocksX / 2f) * worldUnitPerBlock;
        float centerZ = (blocksY / 2f) * worldUnitPerBlock;
        Vector3 spawnRayOrigin = new Vector3(centerX, 100f, centerZ);

        if (Physics.Raycast(spawnRayOrigin, Vector3.down, out RaycastHit hit, 200f))
        {
            Instantiate(playerPrefab, hit.point + Vector3.up * 2f, Quaternion.identity);
        }
        else
        {
            Instantiate(playerPrefab, new Vector3(centerX, 2f, centerZ), Quaternion.identity);
        }
    }

    void SurroundWithOceanByBlockRange(int blocksX, int blocksY)
    {
        if (oceanPrefab == null) return;

        float unit = worldUnitPerBlock;
        float offset = unit / 2f;

        for (int x = -1; x <= blocksX; x++)
        {
            float posX = x * unit + offset;
            float topZ = (blocksY * unit + offset) + unit * oceanOffsetTop;
            float bottomZ = (-1 * unit + offset) - unit * oceanOffsetBottom;

            Instantiate(oceanPrefab, new Vector3(posX, 0f, topZ), Quaternion.identity, terrainParent);
            Instantiate(oceanPrefab, new Vector3(posX, 0f, bottomZ), Quaternion.identity, terrainParent);
        }

        for (int y = 0; y < blocksY; y++)
        {
            float posZ = y * unit + offset;
            float leftX = (-1 * unit + offset) - unit * oceanOffsetLeft;
            float rightX = (blocksX * unit + offset) + unit * oceanOffsetRight;

            Instantiate(oceanPrefab, new Vector3(leftX, 0f, posZ), Quaternion.identity, terrainParent);
            Instantiate(oceanPrefab, new Vector3(rightX, 0f, posZ), Quaternion.identity, terrainParent);
        }
    }

    string GetLatestImagePath(string folderPath)
    {
        if (!Directory.Exists(folderPath))
        {
            Debug.LogError("📂 Folder does not exist: " + folderPath);
            return null;
        }

        var files = Directory.GetFiles(folderPath, "*.png");
        if (files.Length == 0)
        {
            Debug.LogError("🖼 No images in folder: " + folderPath);
            return null;
        }

        string latestFile = files.OrderByDescending(File.GetLastWriteTime).First();
        Debug.Log("🆕 Latest image path: " + latestFile);
        return latestFile;
    }
}
