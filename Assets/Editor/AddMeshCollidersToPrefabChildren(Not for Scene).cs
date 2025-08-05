using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using System.IO;

public class AddMeshCollidersToPrefabChildren
{
    [MenuItem("Tools/Magic System/Add MeshColliders to Prefab Children")]
    public static void AddToSelectedPrefabs()
    {
        var selected = Selection.GetFiltered<GameObject>(SelectionMode.Assets);

        foreach (var prefab in selected)
        {
            string path = AssetDatabase.GetAssetPath(prefab);
            if (!path.EndsWith(".prefab"))
                continue;

            GameObject prefabRoot = PrefabUtility.LoadPrefabContents(path);
            int added = 0;

            foreach (var mr in prefabRoot.GetComponentsInChildren<MeshRenderer>(true))
            {
                var go = mr.gameObject;
                if (!go.GetComponent<Collider>())
                {
                    var mf = go.GetComponent<MeshFilter>();
                    if (mf && mf.sharedMesh)
                    {
                        var mc = go.AddComponent<MeshCollider>();
                        mc.sharedMesh = mf.sharedMesh;
                        mc.convex = false;
                        added++;
                    }
                }
            }

            PrefabUtility.SaveAsPrefabAsset(prefabRoot, path);
            PrefabUtility.UnloadPrefabContents(prefabRoot);

            Debug.Log($"[MagicTool] Added {added} MeshColliders to prefab: {Path.GetFileName(path)}");
        }
    }
}
