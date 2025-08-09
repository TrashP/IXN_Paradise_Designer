using System.Collections; // for IEnumerator
using System.Collections.Generic;
using System.Linq;
using Polyperfect.Common;
using Polyperfect.Crafting.Integration;
using UnityEngine;

namespace Polyperfect.Crafting.Demo
{
    [RequireComponent(typeof(BaseItemStackInventory))]
    public class DefaultInventory : PolyMono
    {
        public override string __Usage => "Fills the target inventory with the provided items on Start";
        public List<ObjectItemStack> ToInsert;

        
        void Start()
        {
            StartCoroutine(DelayedInsert());
        }

        IEnumerator DelayedInsert()
        {
            
            yield return null;
            CreateAndInsert();
        }

        public void CreateAndInsert()
        {
            if (!TryGetComponent<BaseItemStackInventory>(out var inv))
            {
                Debug.LogWarning("[DefaultInventory] Missing BaseItemStackInventory.");
                return;
            }

            if (ToInsert == null || ToInsert.Count == 0)
            {
                
                return;
            }

            inv.InsertPossible(ToInsert.Select(i => (ItemStack)i));
        }
    }
}