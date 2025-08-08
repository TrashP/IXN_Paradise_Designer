using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using MagicSystem;
using System.IO;

public class MagicCleanerEditor : EditorWindow
{
    enum CleanScope { RootOnly, ChildrenOnly, All }
    CleanScope cleanScope = CleanScope.All;

    [MenuItem("Tools/Magic System/Clean Magic Components")]
    public static void ShowWindow()
    {
        GetWindow<MagicCleanerEditor>("Clean Magic");
    }

    void OnGUI()
    {
        GUILayout.Label("Magic Cleaner", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox("This tool removes MagicEnabler and all magic modules (e.g. DestroyMagic, CopyMagic) from selected objects or prefabs.", MessageType.Info);

        cleanScope = (CleanScope)EditorGUILayout.EnumPopup("Clean Scope", cleanScope);

        if (GUILayout.Button("Clean Selected"))
        {
            CleanSelected();
        }
    }

    void CleanSelected()
    {
        var selection = Selection.objects;
        int cleaned = 0;

        foreach (var obj in selection)
        {
            if (obj is GameObject go)
            {
                string path = AssetDatabase.GetAssetPath(go);

                if (!string.IsNullOrEmpty(path) && path.EndsWith(".prefab"))
                {
                    ProcessPrefab(path);
                    cleaned++;
                }
                else
                {
                    ProcessSceneObject(go);
                    cleaned++;
                }
            }
        }

        Debug.Log($"[MagicCleaner] Cleaned {cleaned} object(s).");
        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
    }

    void ProcessSceneObject(GameObject obj)
    {
        Undo.RegisterCompleteObjectUndo(obj, "Clean Magic");

        if (cleanScope == CleanScope.RootOnly || cleanScope == CleanScope.All)
            CleanMagicComponents(obj);

        if (cleanScope == CleanScope.ChildrenOnly || cleanScope == CleanScope.All)
        {
            foreach (Transform child in obj.transform)
                CleanMagicComponentsRecursive(child.gameObject);
        }
    }

    void ProcessPrefab(string path)
    {
        var prefabRoot = PrefabUtility.LoadPrefabContents(path);

        if (cleanScope == CleanScope.RootOnly || cleanScope == CleanScope.All)
            CleanMagicComponents(prefabRoot);

        if (cleanScope == CleanScope.ChildrenOnly || cleanScope == CleanScope.All)
        {
            foreach (Transform child in prefabRoot.transform)
                CleanMagicComponentsRecursive(child.gameObject);
        }

        PrefabUtility.SaveAsPrefabAsset(prefabRoot, path);
        PrefabUtility.UnloadPrefabContents(prefabRoot);

        Debug.Log($"[Prefab] Cleaned magic from: {Path.GetFileName(path)}");
    }

    void CleanMagicComponentsRecursive(GameObject obj)
    {
        CleanMagicComponents(obj);
        foreach (Transform child in obj.transform)
            CleanMagicComponentsRecursive(child.gameObject);
    }

    void CleanMagicComponents(GameObject go)
    {
        // Destroy MagicEnabler
        var enabler = go.GetComponent<MagicEnabler>();
        if (enabler)
            Object.DestroyImmediate(enabler, true);

        // Destroy all BaseMagic derived modules
        var modules = go.GetComponents<BaseMagic>();
        foreach (var module in modules)
            Object.DestroyImmediate(module, true);
    }
}
