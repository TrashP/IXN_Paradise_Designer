using Polyperfect.Common;
using Polyperfect.Crafting.Framework;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace Polyperfect.Crafting.Integration.UGUI
{
    [RequireComponent(typeof(BaseItemSlotComponent))]
    public class UGUIItemDisplay : ItemUserBase
    {
        public Text NameLabel;
        public Text QuantityLabel;
        [FormerlySerializedAs("Icon")] public Image ImageComponent;
        public IconsCategory Icons;
        public string QuantityDisplayFormat = "{0}";
        public bool OnlyShowTextIfNoIcon;
        public bool OnlyShowNumberIfMultiple = true;
        public bool NewlineSpaces = true;
        protected BaseItemSlotComponent _slot;

        public override string __Usage => $"Shows the icon, name, and quantity of the item stack in the attached {nameof(BaseItemSlotComponent)}.";

        protected void Start()
        {
            if (!Icons) 
                Debug.LogError("An Icons category should be assigned within the script as a default reference.");
            _slot = GetComponent<BaseItemSlotComponent>();
            _slot.Changed += HandleChange;

            HandleChange();
        }

        protected virtual void HandleChange()
        {
            var hasItem = !_slot.Peek().ID.IsDefault();
            if (NameLabel)
            {
                var nameLabelText = hasItem ? World.GetName(_slot.Peek().ID) : "";
                nameLabelText = NewlineSpaces ? nameLabelText.NewlineSpaces() : nameLabelText;
                NameLabel.text = nameLabelText;
            }

            if (QuantityLabel)
            {
                var showNumber = _slot.Peek().Value > (OnlyShowNumberIfMultiple?1:0);
                if (showNumber)
                    QuantityLabel.text = string.Format(QuantityDisplayFormat,_slot.Peek().Value);

                QuantityLabel.enabled = showNumber;
            }

            if (ImageComponent)
            {
                var world = ItemWorldReference.Instance.World;
                var accessor = world.GetReadOnlyAccessor<IconData>(Icons);
                ImageComponent.sprite = hasItem ? accessor.GetDataOrDefault(_slot.Peek().ID).Icon : null;
                ImageComponent.enabled = ImageComponent.sprite;

                if (NameLabel && OnlyShowTextIfNoIcon)
                    NameLabel.gameObject.SetActive(!ImageComponent.sprite);
            }
        }
    }
}