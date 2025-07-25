using Polyperfect.Common;
using UnityEngine;
using UnityEngine.Events;

namespace Polyperfect.Crafting.Demo
{
    public class EquippableEvents : PolyMono
    {
        public override string __Usage => "Will receive and forward messages when the slot is equipped or unequipped.";

        public UnityEvent OnEquipped, OnUnequipped;

        public bool Equipped { get; private set; }
        public void SendEquipped()
        {
            Equipped = true;
            OnEquipped.Invoke();
        }

        public void SendUnequipped()
        {
            Equipped = false;
            OnUnequipped.Invoke();
        }
    }
}