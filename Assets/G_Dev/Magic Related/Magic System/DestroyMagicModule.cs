using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace MagicSystem
{
    public class DestroyMagicModule : MonoBehaviour, ISpellModule
    {
        [Header("Single Target Effects (Left Click)")]
        [SerializeField] List<VFXSequence> singleTargetVFX;
        [SerializeField] float singleTargetDelay = 0f;

        [Header("AOE Effects (Right Click)")]
        [SerializeField] List<VFXSequence> aoeVFX;
        [SerializeField] float aoeRadius = 5f;
        [SerializeField] float aoeDelay = 0f;


        public void OnLeftClick(RaycastHit hit, WandSpellCaster context)
        {
            var magic = FindDestroyMagicInParents(hit.collider.transform);
            if (magic != null)
            {
                context.StartCoroutine(VFXUtility.PlayVFXSequence(singleTargetVFX, hit.point));
                context.StartCoroutine(DelayedExecute(magic, hit.point, singleTargetDelay));
                Debug.Log($"[DestroyMagic] Target will be destroyed: {magic.gameObject.name}");
            }
            else
            {
                Debug.LogWarning("[DestroyMagic] No destroyable target found.");
            }
        }

        public void OnRightClick(RaycastHit hit, WandSpellCaster context)
        {
            Vector3 center = hit.point;
            context.StartCoroutine(VFXUtility.PlayVFXSequence(aoeVFX, center));


            Collider[] hits = Physics.OverlapSphere(center, aoeRadius);
            foreach (var col in hits)
            {
                var magic = FindDestroyMagicInParents(col.transform);
                if (magic != null)
                {
       
                    context.StartCoroutine(DelayedExecute(magic, col.transform.position, aoeDelay));
                }
            }

            Debug.Log($"[DestroyMagic] AOE triggered at {center}, radius: {aoeRadius}");
        }

        IEnumerator DelayedExecute(DestroyMagic magic, Vector3 position, float delay)
        {
            yield return new WaitForSeconds(delay);
            if (magic)
                magic.Execute(position);
        }

        DestroyMagic FindDestroyMagicInParents(Transform t)
        {
            while (t != null)
            {
                var magic = t.GetComponent<DestroyMagic>();
                if (magic)
                    return magic;
                t = t.parent;
            }
            return null;
        }
    }
}
