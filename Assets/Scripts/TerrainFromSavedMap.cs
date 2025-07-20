using UnityEngine;
using System.IO;

public class TerrainFromSavedMap : MonoBehaviour
{
    public Terrain terrain;

    void Start()
    {
        string path = PlayerPrefs.GetString("SavedMapPath", "");
        if (File.Exists(path))
        {
            Texture2D colorMap = LoadImage(path);
            float[,] heights = ConvertToHeightMap(colorMap);
            ApplyHeightsToTerrain(heights);
        }
        else
        {
            Debug.LogError("can not find the path of saved image: " + path);
        }
    }

    Texture2D LoadImage(string path)
    {
        byte[] fileData = File.ReadAllBytes(path);
        Texture2D tex = new Texture2D(2, 2);
        tex.LoadImage(fileData);
        return tex;
    }

    float[,] ConvertToHeightMap(Texture2D colorMap)
    {
        int width = colorMap.width;
        int height = colorMap.height;
        float[,] heights = new float[height, width];

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Color pixel = colorMap.GetPixel(x, y);
                float value = GetHeightFromColor(pixel);
                heights[y, x] = value;
            }
        }
        return heights;
    }

    float GetHeightFromColor(Color color)
    {
        // 将颜色转换为向量形式便于计算距离
        Vector3 c = new Vector3(color.r, color.g, color.b);

        // 你的图片中使用的三种代表颜色
        Vector3 forestBrown = new Vector3(0.55f, 0.33f, 0.0f);     // brown：forest
        Vector3 sandOrange = new Vector3(1.0f, 0.635f, 0.0f);      // orange：beach
        Vector3 seaBlue = new Vector3(0.0f, 0.77f, 1.0f);          // blue：ocean

        // 计算距离
        float forestDist = Vector3.Distance(c, forestBrown);
        float sandDist = Vector3.Distance(c, sandOrange);
        float seaDist = Vector3.Distance(c, seaBlue);

        float min = Mathf.Min(forestDist, sandDist, seaDist);

        if (min == forestDist)
            return 0.8f; 
        else if (min == sandDist)
            return 0.4f; 
        else
            return 0.1f;
    }


    void ApplyHeightsToTerrain(float[,] heights)
    {
        TerrainData terrainData = terrain.terrainData;
        int width = heights.GetLength(1);
        int height = heights.GetLength(0);

        terrainData.heightmapResolution = Mathf.Max(width, height) + 1;
        terrainData.size = new Vector3(500, 100, 500);
        terrainData.SetHeights(0, 0, heights);

        Debug.Log("HeightMap is applied！");
    }
}
