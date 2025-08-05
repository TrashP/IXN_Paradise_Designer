using UnityEngine;

namespace MagicSystem
{
    public class CopyMagic : BaseMagic
    {
        [SerializeField] private GameObject linkedPrefab;

        public override void Execute(Vector3 position)
        {
            if (linkedPrefab != null)
            {
                GameObject obj = Instantiate(linkedPrefab, position, Quaternion.identity);

                // Debug: check for Collider
                if (obj.TryGetComponent<Collider>(out var col))
                {
                    Debug.Log($"[CopyMagic] Spawned object has collider: {col.GetType().Name}");
                }
                else
                {
                    Debug.LogWarning("[CopyMagic] Spawned object has NO collider!");
                }
            }
            else
            {
                Debug.LogWarning($"[CopyMagic] No prefab linked on {gameObject.name}");
            }
        }

        public GameObject ExecuteFromObject(Vector3 pos, Quaternion rot)
        {
            if (!linkedPrefab)
                return null;

            var obj = Instantiate(linkedPrefab, pos, rot);
            return obj;
        }
    }
}
