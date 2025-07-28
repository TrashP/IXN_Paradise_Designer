using Polyperfect.Common;
using Polyperfect.Crafting.Integration;
using UnityEngine;

namespace Polyperfect.Crafting.Demo
{
    [RequireComponent(typeof(BaseItemSlotComponent))]
    public class ItemGenerator : PolyMono
    {
        public override string __Usage => "Creates and inserts an item into the attached slot regularly.";

        public float CreationInterval = 1f;
        public ObjectItemStack Generated;
        

        float time;

        BaseItemSlotComponent slot;
        void Start()
        {
            slot = GetComponent<BaseItemSlotComponent>();
        }

        void Update()
        {
            time += Time.deltaTime;
            if (time > CreationInterval)
            {
                time -= CreationInterval;
                slot.InsertPossible(Generated);
            }
        }
    }
}