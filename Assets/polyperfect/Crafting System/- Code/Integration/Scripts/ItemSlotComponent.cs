using System;
using System.Collections.Generic;
using Polyperfect.Crafting.Edit;
using Polyperfect.Crafting.Framework;
using UnityEngine;
using UnityEngine.Events;

namespace Polyperfect.Crafting.Integration
{
    public class ItemSlotComponent : BaseItemSlotComponent
    {
        public override string __Usage => "Simple item slot for any use.";

        [Obsolete("Readonly has been changed to AllowInsert and AllowExtract.")]
        public bool IsReadonly => throw new System.NotImplementedException();

        [ReplaceWithPeekInPlaymode(nameof(Slot))] [SerializeField] ObjectItemStack InitialObject;
        public bool AllowInsert = true, AllowExtract = true;


        [SerializeField] ItemStackConstraintCollection ConstraintCollection;
        readonly List<StackInsertionConstraint> constraints = new List<StackInsertionConstraint>();
        [SerializeField] UnityEvent OnChanged;

        protected ItemSlotWithProcessor Slot
        {
            get
            {
                if (slot.IsDefault())
                    InitSlot();
                return slot;
            }
        }

        void InitSlot()
        {
            if (ConstraintCollection)
                constraints.AddRange(ConstraintCollection.Constraints);
            slot = new ItemSlotWithProcessor(constraints,World);
            slot.Changed += FireChange;
        }

        ItemSlotWithProcessor slot;

        protected void Start()
        {
            if (!InitialObject?.ID.IsDefault() ?? false)
            {
                var allowInsert = AllowInsert;
                AllowInsert = true;
                InsertPossible(new ItemStack(InitialObject.ID, InitialObject.Value));
                AllowInsert = allowInsert;
            }
        }

        public override event PolyChangeEvent Changed;

        public override ItemStack RemainderIfInserted(ItemStack toInsert)
        {
            if (!AllowInsert)
                return toInsert;
            return Slot.RemainderIfInserted(toInsert);
        }


        public override ItemStack InsertPossible(ItemStack toInsert)
        {
            if (!AllowInsert)
                return toInsert;
            
            var ret = Slot.InsertPossible(toInsert);
            return ret;
        }

        public override ItemStack Peek()
        {
            return Slot.Peek();
        }

        public override ItemStack Peek(Quantity arg)
        {
            return Slot.Peek(arg);
        }


        public override ItemStack ExtractAmount(Quantity arg)
        {
            if (!AllowExtract)
                return default;

            var ret = Slot.ExtractAmount(arg);
            return ret;
        }


        public override ItemStack ExtractAll()
        {
            if (!AllowExtract)
                return default;

            var ret = Slot.ExtractAll();
            return ret;
        }

        public override bool CanExtract()
        {
            return AllowExtract&& Slot.CanExtract();
        }

        void FireChange()
        {
            Changed?.Invoke();
            OnChanged?.Invoke();
        }

        public void Discard()
        {
            Slot.ExtractAll();
        }

        public void AddConstraint(StackInsertionConstraint constraint) => constraints.Add(constraint);
        public void RemoveConstraint(StackInsertionConstraint constraint) => constraints.Remove(constraint);
    }
}