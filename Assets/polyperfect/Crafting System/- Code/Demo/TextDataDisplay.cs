using Polyperfect.Crafting.Integration;
using UnityEngine;
using UnityEngine.UI;

namespace Polyperfect.Crafting.Demo
{
    [RequireComponent(typeof(BaseItemSlotComponent))]
    public class TextDataDisplay : ItemUserBase
    {
        public override string __Usage => "Displays the text in a Text component.";

        public CategoryWithText Category;
        public Text TargetElement;
        public string DefaultText = "No Description";

        void Start()
        {
            var slot = GetComponent<BaseItemSlotComponent>();
            slot.Changed += HandleSlotChanged;
            
            void HandleSlotChanged()
            {
                var textAccessor = World.GetReadOnlyAccessor<string>(Category.ID);
                if (textAccessor.TryGetValue(slot.Peek().ID, out var text))
                    TargetElement.text = text;
                else
                    TargetElement.text = DefaultText;
            }
        }
    }
}