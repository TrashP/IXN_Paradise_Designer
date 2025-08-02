using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using MagicSystem;

public class CopyMagicModule : MonoBehaviour, ISpellModule
{
    [Header("Absorb Effects (Right Click)")]
    [SerializeField] List<VFXSequence> absorbVFXSequence;

    [Header("Release Effects (Left Click)")]
    [SerializeField] List<VFXSequence> releaseVFXSequence;

    [Header("Timing")]
    [SerializeField] float releaseDelay = 1.5f;

    GameObject copiedTarget;
    CopyMagic cachedMagic;

    public void OnRightClick(RaycastHit hit, WandSpellCaster context)
    {
        // Traverse upward to find MagicEnabler
        MagicEnabler enabler = null;
        Transform current = hit.collider.transform;
        while (current != null)
        {
            enabler = current.GetComponent<MagicEnabler>();
            if (enabler != null)
                break;
            current = current.parent;
        }

        var copy = enabler?.GetMagic<CopyMagic>();

        if (copy != null)
        {
            cachedMagic = copy;
            copiedTarget = enabler.gameObject;

            // Play absorb effects
            context.StartCoroutine(VFXUtility.PlayVFXSequence(absorbVFXSequence, hit.point));
            Debug.Log($"[CopyMagic] Absorbed from: {copiedTarget.name}");
        }
        else
        {
            cachedMagic = null;
            copiedTarget = null;
            Debug.LogWarning("[CopyMagic] No CopyMagic found on target or parent.");
        }
    }

    public void OnLeftClick(RaycastHit hit, WandSpellCaster context)
    {
        if (cachedMagic == null || copiedTarget == null)
        {
            Debug.LogWarning("[CopyMagic] No copied object to place.");
            return;
        }

        // Play release VFX immediately, but destruction is delayed
        context.StartCoroutine(DelayedPlace(hit.point, context));
    }

    IEnumerator DelayedPlace(Vector3 position, WandSpellCaster context)
    {
        // Play release effect immediately
        context.StartCoroutine(VFXUtility.PlayVFXSequence(releaseVFXSequence, position));

        yield return new WaitForSeconds(releaseDelay);

        // Ensure consistent forward direction
        Vector3 forward = new Vector3(transform.forward.x, 0f, transform.forward.z);
        if (forward.sqrMagnitude < 0.001f)
            forward = Vector3.forward;

        Quaternion rotation = Quaternion.LookRotation(forward);

        GameObject created = cachedMagic.ExecuteFromObject(position, rotation);
        if (created)
        {
            MagicModuleCloner.CloneMagicModules(copiedTarget, created);
            Debug.Log($"[CopyMagic] Object cloned: {created.name}");
        }
        else
        {
            Debug.LogWarning("[CopyMagic] Instantiation failed: no object created.");
        }
    }
}
