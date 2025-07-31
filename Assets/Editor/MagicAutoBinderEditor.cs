using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using System.IO;
using MagicSystem;

public class MagicAutoBinderEditor : EditorWindow
{
    [MenuItem("Tools/Magic System/Auto Attach Prefab CopyMagic")]
    public static void ShowWindow()
    {
        GetWindow<MagicAutoBinderEditor>("Magic Auto Binder");
    }

    void OnGUI()
    {
        GUILayout.Label("Auto-Bind CopyMagic to Prefab Instances", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox("This tool will attach MagicEnabler + CopyMagic to selected scene objects or prefab files, and auto-link their prefab reference.", MessageType.Info);

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
                    
                    ProcessPrefabAsset(path);
                    success++;
                }
                else
                {
                   
                    ProcessSceneObject(go);
                    success++;
                }
            }
        }

        Debug.Log($"[MagicAutoBinder] Successfully attached to {success} object(s).");
        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
    }

    void ProcessSceneObject(GameObject obj)
    {
        GameObject prefabRef = PrefabUtility.GetCorrespondingObjectFromSource(obj);
        if (prefabRef == null)
        {
            Debug.LogWarning($"[Scene] {obj.name} is not a prefab instance. Skipped.");
            return;
        }

        Undo.RegisterCompleteObjectUndo(obj, "Attach Magic");

        var enabler = obj.GetComponent<MagicEnabler>() ?? Undo.AddComponent<MagicEnabler>(obj);
        var copyMagic = obj.GetComponent<CopyMagic>() ?? Undo.AddComponent<CopyMagic>(obj);

        // Assign linked prefab
        var so = new SerializedObject(copyMagic);
        so.FindProperty("linkedPrefab").objectReferenceValue = prefabRef;
        so.ApplyModifiedProperties();

        // Ensure CopyMagic is in MagicEnabler list
        var eso = new SerializedObject(enabler);
        var list = eso.FindProperty("magicModules");

        bool alreadyIn = false;
        for (int i = 0; i < list.arraySize; i++)
        {
            if (list.GetArrayElementAtIndex(i).objectReferenceValue == copyMagic)
            {
                alreadyIn = true;
                break;
            }
        }

        if (!alreadyIn)
        {
            int index = list.arraySize;
            list.InsertArrayElementAtIndex(index);
            list.GetArrayElementAtIndex(index).objectReferenceValue = copyMagic;
            eso.ApplyModifiedProperties();
        }

        Debug.Log($"[Scene] Attached Magic to: {obj.name}");
    }

    void ProcessPrefabAsset(string prefabPath)
    {
        GameObject prefabRoot = PrefabUtility.LoadPrefabContents(prefabPath);

        // Add or get components
        var enabler = prefabRoot.GetComponent<MagicEnabler>() ?? prefabRoot.AddComponent<MagicEnabler>();
        var copyMagic = prefabRoot.GetComponent<CopyMagic>() ?? prefabRoot.AddComponent<CopyMagic>();

        // Link self as prefab
        var so = new SerializedObject(copyMagic);
        var linkedProp = so.FindProperty("linkedPrefab");
        linkedProp.objectReferenceValue = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
        so.ApplyModifiedProperties();

        // Ensure CopyMagic is in MagicEnabler.magicModules
        var eso = new SerializedObject(enabler);
        var listProp = eso.FindProperty("magicModules");

        bool alreadyIn = false;
        for (int i = 0; i < listProp.arraySize; i++)
        {
            if (listProp.GetArrayElementAtIndex(i).objectReferenceValue == copyMagic)
            {
                alreadyIn = true;
                break;
            }
        }

        if (!alreadyIn)
        {
            int index = listProp.arraySize;
            listProp.InsertArrayElementAtIndex(index);
            listProp.GetArrayElementAtIndex(index).objectReferenceValue = copyMagic;
            eso.ApplyModifiedProperties();
        }

        // Save changes
        PrefabUtility.SaveAsPrefabAsset(prefabRoot, prefabPath);
        PrefabUtility.UnloadPrefabContents(prefabRoot);

        Debug.Log($"[Prefab] Attached Magic to: {Path.GetFileName(prefabPath)}");
    }
}
