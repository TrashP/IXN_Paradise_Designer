using System;
using System.Collections.Generic;
using Polyperfect.Common;
using Polyperfect.Crafting.Framework;
using Polyperfect.Crafting.Integration;
using UnityEngine;
using UnityEngine.Events;
using SlotType = Polyperfect.Crafting.Framework.ISlot<Polyperfect.Crafting.Framework.Quantity,Polyperfect.Crafting.Integration.ItemStack>;

namespace Polyperfect.Crafting.Integration
{
    public static class ItemSlotLink// : PolyMono
    {
        /*public override string __Usage => "Links this slot to another so their contents match";
        [SerializeField] [HighlightNull] ItemSlotComponent LinkedTo;

        ItemSlotComponent slot;
        DisposableSlotLink link;
        void Awake()
        {
            slot = GetComponent<ItemSlotComponent>();
            link = LinkSlots(slot,LinkedTo);
        }

        void OnDestroy() => link.Dispose();*/

        public class DisposableSlotLink : IDisposable
        {
            Action unlinkMethod;
            public bool Locked { get; set; }


            public void SetUnlinkMethod(Action unlink) => unlinkMethod = unlink;

            public void Dispose() => unlinkMethod();
        }
        public static DisposableSlotLink LinkSlots(SlotType original, SlotType copy)
        {
            var ret = new DisposableSlotLink();
            void SourceChanged() => HandleLinkedSlotUpdated(original, copy,ret);
            void CopyChanged() => HandleLinkedSlotUpdated(copy, original,ret);

            SourceChanged();
            RegisterCallbacks();
            
            ret.SetUnlinkMethod(UnregisterCallbacks);
            return ret;

            void RegisterCallbacks()
            {
                original.Changed += SourceChanged;
                copy.Changed += CopyChanged;
            }
            void UnregisterCallbacks()
            {
                original.Changed -= SourceChanged;
                copy.Changed -= CopyChanged;
                copy.ExtractAll();
            }
        }

        static void HandleLinkedSlotUpdated(SlotType source, SlotType destination,DisposableSlotLink link)
        {
            if (link.Locked)
                return;

            link.Locked = true;
            destination.ExtractAll();
            destination.InsertCompletely(source.Peek());
            link.Locked = false;
        }

    } 
}