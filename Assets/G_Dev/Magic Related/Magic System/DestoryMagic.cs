using UnityEngine;
using System.Collections;

namespace MagicSystem
{
    public class DestroyMagic : BaseMagic
    {
        [Header("Destruction Settings")]
        [SerializeField] private float delayBeforeDestroy = 0f;

        [Header("Drop Settings")]
        [SerializeField] private GameObject dropItemPrefab;
        [SerializeField] private int dropCount = 1;

        // Prevent multiple execution
        bool hasExecuted = false;

        public override void Execute(Vector3 position)
        {
            if (hasExecuted)
                return;

            hasExecuted = true;
            StartCoroutine(DoDestroy());
        }

        IEnumerator DoDestroy()
        {
            yield return new WaitForSeconds(delayBeforeDestroy);

            // Only drop items if prefab is assigned
            if (dropItemPrefab)
            {
                for (int i = 0; i < dropCount; i++)
                {
                    Vector3 offset = Random.insideUnitSphere * 0.05f;
                    offset.y = Mathf.Abs(offset.y);

                    Vector3 spawnPoint = transform.position + offset;

                    if (TryGetComponent<Renderer>(out var rend))
                    {
                        var topY = rend.bounds.max.y;
                        var rayOrigin = new Vector3(spawnPoint.x, topY + 0.5f, spawnPoint.z);

                        if (Physics.Raycast(rayOrigin, Vector3.down, out RaycastHit hitInfo, 5f, LayerMask.GetMask("Default", "Ground")))
                        {
                            spawnPoint = hitInfo.point;
                        }
                    }

                    Instantiate(dropItemPrefab, spawnPoint, Quaternion.identity);
                }
            }

            Debug.Log($"[DestroyMagic] Destroyed {gameObject.name}, dropped {(dropItemPrefab ? dropCount : 0)} item(s).");
            Destroy(gameObject);
        }
    }
}
