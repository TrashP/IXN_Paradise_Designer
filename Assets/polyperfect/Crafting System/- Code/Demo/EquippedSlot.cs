using System;
using System.Linq;
using Polyperfect.Common;
using Polyperfect.Crafting.Framework;
using Polyperfect.Crafting.Integration;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

namespace Polyperfect.Crafting.Demo
{
    public class EquippedSlot : PolyMono
    {
        public override string __Usage => "Keeps track of which slot is actively equipped.";

        [SerializeField] ItemSlotComponent InitialSlot;

        

        [FormerlySerializedAs("OnEquippedChange")] public ItemStackEvent OnContentsChanged = new ItemStackEvent();
        public UnityEvent OnSlotChanged = new UnityEvent();
        ISlot<Quantity,ItemStack> slot;

        public ISlot<Quantity,ItemStack> Slot
        {
            get => slot;
            set
            {
                var old = slot;
                if (slot!=null)
                    slot.Changed -= HandleSlotUpdate;
                if (slot is BaseItemSlotComponent oldSlotComponent && oldSlotComponent)
                {
                    if (oldSlotComponent.gameObject.TryGetComponent(out EquippableEvents equippable))
                        equippable.SendUnequipped();
                }

                var changed = slot != value;
                slot = value;
                if (slot !=null)
                    slot.Changed += HandleSlotUpdate;
                
                if (slot is BaseItemSlotComponent newSlotComponent && newSlotComponent)
                {
                    if (newSlotComponent.gameObject.TryGetComponent(out EquippableEvents equippable))
                        equippable.SendEquipped();
                }
                if (old != slot) 
                    OnSlotChanged?.Invoke();
                if (changed)
                    HandleSlotUpdate();
            }
        }

        void HandleSlotUpdate() => OnContentsChanged.Invoke(Slot?.Peek() ?? default);

        void Awake()
        {
            if (InitialSlot)
                Slot = InitialSlot;
        }

        void Start()
        {
            if (InitialSlot)
                return;
                
            //var inv = GetComponent<BaseItemStackInventory>();
            //if (inv)
                //Slot = inv.Slots.FirstOrDefault();
        }

        void Update()
        {
            //lifetime check for components
            if (Slot is ItemSlotComponent && !(Slot as ItemSlotComponent)) 
                Slot = null;
        }
    }
}