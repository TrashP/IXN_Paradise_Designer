using UnityEngine;
using UnityEngine.SceneManagement;
using System.IO;
using System.Linq;

public class DrawingToTerrain : MonoBehaviour
{
    public string imageFolder = @"C:\Users\lenovo\Desktop\SavedImage";

    public void OnGenerateButtonClicked()
    {
        string latestPng = GetLatestPngFile(imageFolder);

        if (string.IsNullOrEmpty(latestPng))
        {
            Debug.LogError("❌ 没有找到任何 PNG 文件！");
            return;
        }

        Debug.Log("✅ 找到最新图片：" + latestPng);
        PlayerPrefs.SetString("SavedMapPath", latestPng);
        SceneManager.LoadScene("TerrainScene");
    }

    string GetLatestPngFile(string folderPath)
    {
        if (!Directory.Exists(folderPath))
        {
            Debug.LogError("❌ 文件夹不存在: " + folderPath);
            return null;
        }

        var pngFiles = Directory.GetFiles(folderPath, "*.png");
        if (pngFiles.Length == 0) return null;

        var latestFile = pngFiles
            .Select(f => new FileInfo(f))
            .OrderByDescending(f => f.LastWriteTime)
            .First();

        return latestFile.FullName;
    }
}
