using UnityEngine;
using UnityEditor;
using System.IO;

public class MaterialUnlinkerTool : EditorWindow
{
    enum TargetScope
    {
        RootOnly,
        IncludeChildren
    }

    TargetScope scope = TargetScope.IncludeChildren;
    string saveFolder = "Assets/Materials/Generated";

    [MenuItem("Tools/VFX/Make Materials Unique")]
    static void OpenWindow() => GetWindow<MaterialUnlinkerTool>("Material Unlinker");

    void OnGUI()
    {
        GUILayout.Label("Detach Shared Materials", EditorStyles.boldLabel);
        scope = (TargetScope)EditorGUILayout.EnumPopup("Target Scope", scope);
        saveFolder = EditorGUILayout.TextField("Save Folder", saveFolder);

        EditorGUILayout.HelpBox("This tool duplicates shared materials into unique assets to avoid unintended sharing between VFX.", MessageType.Info);

        if (GUILayout.Button("Process Selected GameObjects"))
        {
            ProcessSelection();
        }
    }

    void ProcessSelection()
    {
        var selected = Selection.gameObjects;
        if (selected.Length == 0)
        {
            Debug.LogWarning("[MaterialUnlinker] No GameObjects selected.");
            return;
        }

        if (!Directory.Exists(saveFolder))
            Directory.CreateDirectory(saveFolder);

        foreach (var go in selected)
        {
            var renderers = scope == TargetScope.IncludeChildren
                ? go.GetComponentsInChildren<Renderer>(true)
                : go.GetComponents<Renderer>();

            for (int r = 0; r < renderers.Length; r++)
            {
                var renderer = renderers[r];
                var mats = renderer.sharedMaterials;

                for (int i = 0; i < mats.Length; i++)
                {
                    var original = mats[i];
                    if (!original || AssetDatabase.IsSubAsset(original)) continue;

                    string sanitizedName = SanitizeFileName($"{go.name}_{original.name}_R{r}M{i}");
                    string path = $"{saveFolder}/{sanitizedName}.mat";
                    path = AssetDatabase.GenerateUniqueAssetPath(path);

                    var newMat = new Material(original);
                    AssetDatabase.CreateAsset(newMat, path);
                    mats[i] = newMat;

                    Debug.Log($"[MaterialUnlinker] Created new material: {path}");
                }

                renderer.sharedMaterials = mats;
            }
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("[MaterialUnlinker] Done. All selected objects now use unique materials.");
    }

    string SanitizeFileName(string name)
    {
        foreach (char c in Path.GetInvalidFileNameChars())
            name = name.Replace(c, '_');
        return name;
    }
}
