using System;
using Polyperfect.Crafting.Framework;
using UnityEngine;

namespace Polyperfect.Crafting.Integration
{
    public class LinkedItemSlotComponent : BaseItemSlotComponent
    {
        public override string __Usage => "A slot that references the contents of another directly.";
        [SerializeField] ItemSlotComponent InitialTarget;
        ISlot<Quantity, ItemStack> _slot;

        public ISlot<Quantity, ItemStack> Slot
        {
            get => _slot;
            set
            {
                if (_slot!=null)
                    _slot.Changed -= FireChange;
                
                _slot = value;
                
                if (_slot!=null)
                    _slot.Changed += FireChange;
            }
        }

        public override event PolyChangeEvent Changed;

        void FireChange() => Changed?.Invoke();

        void Awake()
        {
            if (InitialTarget)
                Slot = InitialTarget;
        }

        void OnDestroy()
        {
            Slot = null;
        }
        /*void OnEnable()
        {
            if (Slot != null)
            {
                //just making sure there's no doubled shenanigans
                Slot.Changed -= FireChange;
                Slot.Changed += FireChange;
            }
        }

        void OnDisable()
        {
            if (Slot != null)
                Slot.Changed -= FireChange;
        }*/

        public override ItemStack RemainderIfInserted(ItemStack toInsert) => Slot.RemainderIfInserted(toInsert);

        public override ItemStack InsertPossible(ItemStack toInsert) => Slot.InsertPossible(toInsert);

        public override ItemStack Peek(Quantity arg) => Slot.Peek(arg);

        public override ItemStack Peek() => Slot.Peek();

        public override ItemStack ExtractAll() => Slot.ExtractAll();

        public override bool CanExtract() => Slot.CanExtract();

        public override ItemStack ExtractAmount(Quantity arg) => Slot.ExtractAmount(arg);
    }
}