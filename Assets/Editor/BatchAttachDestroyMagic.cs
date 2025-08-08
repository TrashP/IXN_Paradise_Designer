using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using System.IO;
using MagicSystem;

public class DestroyMagicBinderEditor : EditorWindow
{
    [MenuItem("Tools/Magic System/Batch Attach DestroyMagic")]
    public static void ShowWindow()
    {
        GetWindow<DestroyMagicBinderEditor>("DestroyMagic Binder");
    }

    GameObject customDropPrefab;
    int dropCount = 1;

    void OnGUI()
    {
        GUILayout.Label("Batch Attach DestroyMagic", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox("Adds DestroyMagic to selected GameObjects or prefab files. Auto-links into MagicEnabler.", MessageType.Info);

        customDropPrefab = (GameObject)EditorGUILayout.ObjectField("Drop Prefab (Optional)", customDropPrefab, typeof(GameObject), false);
        dropCount = EditorGUILayout.IntField("Drop Count", dropCount);

        if (GUILayout.Button("Attach to Selected"))
        {
            AttachToSelected();
        }
    }

    void AttachToSelected()
    {
        var selection = Selection.objects;
        int success = 0;

        foreach (var obj in selection)
        {
            if (obj is GameObject go)
            {
                string path = AssetDatabase.GetAssetPath(go);

                if (!string.IsNullOrEmpty(path) && path.EndsWith(".prefab"))
                {
                    ProcessPrefab(path);
                    success++;
                }
                else
                {
                    ProcessSceneObject(go);
                    success++;
                }
            }
        }

        Debug.Log($"[DestroyMagicBinder] Finished processing {success} object(s).");
        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
    }

    void ProcessSceneObject(GameObject obj)
    {
        Undo.RegisterCompleteObjectUndo(obj, "Attach DestroyMagic");

        var magic = obj.GetComponent<DestroyMagic>() ?? Undo.AddComponent<DestroyMagic>(obj);
        ApplyDropSettings(magic);

        var enabler = obj.GetComponent<MagicEnabler>() ?? Undo.AddComponent<MagicEnabler>(obj);
        EnsureInModuleList(enabler, magic);

        Debug.Log($"[Scene] Added DestroyMagic to {obj.name}");
    }

    void ProcessPrefab(string path)
    {
        var prefabRoot = PrefabUtility.LoadPrefabContents(path);

        var magic = prefabRoot.GetComponent<DestroyMagic>() ?? prefabRoot.AddComponent<DestroyMagic>();
        ApplyDropSettings(magic);

        var enabler = prefabRoot.GetComponent<MagicEnabler>() ?? prefabRoot.AddComponent<MagicEnabler>();
        EnsureInModuleList(enabler, magic);

        PrefabUtility.SaveAsPrefabAsset(prefabRoot, path);
        PrefabUtility.UnloadPrefabContents(prefabRoot);

        Debug.Log($"[Prefab] Added DestroyMagic to {Path.GetFileName(path)}");
    }

    void ApplyDropSettings(DestroyMagic magic)
    {
        var so = new SerializedObject(magic);
        so.FindProperty("dropItemPrefab").objectReferenceValue = customDropPrefab;
        so.FindProperty("dropCount").intValue = dropCount;
        so.ApplyModifiedProperties();
    }

    void EnsureInModuleList(MagicEnabler enabler, MonoBehaviour module)
    {
        var so = new SerializedObject(enabler);
        var list = so.FindProperty("magicModules");

        bool exists = false;
        for (int i = 0; i < list.arraySize; i++)
        {
            if (list.GetArrayElementAtIndex(i).objectReferenceValue == module)
            {
                exists = true;
                break;
            }
        }

        if (!exists)
        {
            int index = list.arraySize;
            list.InsertArrayElementAtIndex(index);
            list.GetArrayElementAtIndex(index).objectReferenceValue = module;
            so.ApplyModifiedProperties();
        }
    }
}
