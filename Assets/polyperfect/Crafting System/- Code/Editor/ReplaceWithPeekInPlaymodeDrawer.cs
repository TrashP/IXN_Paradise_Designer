using System;
using Polyperfect.Common;
using Polyperfect.Crafting.Framework;
using Polyperfect.Crafting.Integration;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Polyperfect.Crafting.Edit
{
    [CustomPropertyDrawer(typeof(ReplaceWithPeekInPlaymodeAttribute))]
    public class ReplaceWithPeekInPlaymodeDrawer : PropertyDrawer
    {
        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            if (!Application.isPlaying)
                return property.GetFieldInfo().FieldType == typeof(ObjectItemStack)?new ItemObjectWithQuantityDrawer().CreatePropertyGUI(property):new PropertyField(property);//base.CreatePropertyGUI(property);

            var ve = new VisualElement().SetRow();
            try
            {
                var attr = (ReplaceWithPeekInPlaymodeAttribute)attribute;
                var owningObject = property.GetOwningObject();
                var propAccessor = owningObject.GetType().GetProperty(attr.IPeekableMemberName, TypeExtensions.GetBindingFlags());
                var fieldAccessor = owningObject.GetType().GetField(attr.IPeekableMemberName, TypeExtensions.GetBindingFlags());
                var targetSlot = (propAccessor?.GetValue(owningObject) ?? fieldAccessor?.GetValue(owningObject)) as IPeek<ItemStack>;
                if (targetSlot == null)
                    ve.Add(new Label(
                        $"Target slot for property {property.propertyPath} must have a Field or Property with the name {attr.IPeekableMemberName} that implements {nameof(IPeek<ItemStack>)}"));
                else
                {
                    var iconDisplay = new VisualElement
                    {
                        name = "icon-display",
                        style =
                        {
                            width = 64,
                            height = 64,
                            alignContent = Align.Center,
                            justifyContent = Justify.Center
                        }
                    };
                    var nameDisplay = new Label
                    {
                        name = "name-display",
                        style = 
                        {
                            unityTextAlign = TextAnchor.MiddleCenter
                        }
                    };
                    var quantityDisplay = new Label { name = "quantity-display" };
                    iconDisplay.Add(nameDisplay);
                    ve.Add(iconDisplay);
                    ve.Add(quantityDisplay);
                    iconDisplay.style.unityBackgroundScaleMode = ScaleMode.ScaleToFit;
                    //ve.Add(new Label(owningObject.ToString()));
                    UpdateDisplay(targetSlot,ve);
                    ve.schedule.Execute(() => UpdateDisplay(targetSlot, ve)).Every(100);
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"Error while drawing {nameof(ReplaceWithPeekInPlaymodeDrawer)}\n{e}");
                ve.Add(new Label("Error. Check the console."));
            }


            return ve;
        }

        void UpdateDisplay(IPeek<ItemStack> extractable, VisualElement ve)
        {
            //ve.Q<Label>().text = extractable.Peek().ToString();
            var world = ItemWorldReference.Instance?ItemWorldReference.Instance.World:null;
            if (world==null)
                return;
            var iconDisplay = ve.Q<VisualElement>("icon-display");
            var peeked = extractable.Peek();
            world.GetReadOnlyAccessor<IconData>(StaticCategories.Icons).TryGetValue(peeked.ID, out var iconData);
            iconDisplay.style.backgroundImage = iconData.Icon ? iconData.Icon.texture : null;
            world.GetReadOnlyAccessor<string>(StaticCategories.Names).TryGetValue(peeked.ID, out var name);
            var nameDisplay = ve.Q<Label>("name-display");
            nameDisplay.text = $"{name.SpacifyCamelCaps().WordwrapOrTruncate(9,3)}";
            nameDisplay.DisplayIf(!iconData.Icon);
            ve.Q<Label>("quantity-display").text = $"x{peeked.Value}";
        }
    }
}