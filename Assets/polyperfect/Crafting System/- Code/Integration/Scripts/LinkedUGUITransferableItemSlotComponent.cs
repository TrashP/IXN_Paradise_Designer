using System;
using Polyperfect.Crafting.Integration.UGUI;
using UnityEngine.EventSystems;

namespace Polyperfect.Crafting.Integration
{
    public class LinkedUGUITransferableItemSlotComponent : LinkedItemSlotComponent,IPointerDownHandler
    {
        public event Action OnPreClick;
        public event Action OnPostClick;
        public void OnPointerDown(PointerEventData eventData)
        {
            OnPreClick?.Invoke();
            UGUIItemTransfer.AssertInstanceExists();
            UGUIItemTransfer.Instance.HandleSlotClick(gameObject,eventData);
            OnPostClick?.Invoke();
        }
    }
}