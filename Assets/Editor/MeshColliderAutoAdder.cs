using UnityEngine;
using UnityEditor;

public class MeshColliderAutoAdder
{
    [MenuItem("Tools/Magic System/Add MeshColliders to All MeshRenderers (Selected)")]
    public static void AddMeshCollidersToChildren()
    {
        foreach (var root in Selection.gameObjects)
        {
            int count = 0;
            foreach (var mr in root.GetComponentsInChildren<MeshRenderer>(true))
            {
                var go = mr.gameObject;
                if (!go.GetComponent<Collider>())
                {
                    var mf = go.GetComponent<MeshFilter>();
                    if (mf && mf.sharedMesh)
                    {
                        var mc = Undo.AddComponent<MeshCollider>(go);
                        mc.sharedMesh = mf.sharedMesh;
                        mc.convex = false;
                        count++;
                    }
                }
            }

            Debug.Log($"[MagicTool] Added MeshColliders to {count} child object(s) under {root.name}");
        }
    }
}
