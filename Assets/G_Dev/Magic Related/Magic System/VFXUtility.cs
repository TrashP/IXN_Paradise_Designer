using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MagicSystem
{
    public static class VFXUtility
    {
        public static IEnumerator PlayVFXSequence(List<VFXSequence> vfxList, Vector3 position)
        {
            foreach (var vfx in vfxList)
            {
                if (vfx.delay > 0)
                    yield return new WaitForSeconds(vfx.delay);

                if (vfx.effectPrefab)
                {
                    var fx = Object.Instantiate(vfx.effectPrefab, position, Quaternion.identity);
                    Object.Destroy(fx, vfx.duration);
                }
            }
        }
    }
}
