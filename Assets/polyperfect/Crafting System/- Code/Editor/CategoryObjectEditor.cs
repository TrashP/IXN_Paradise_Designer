using System.Collections.Generic;
using System.Linq;
using Polyperfect.Common;
using Polyperfect.Common.Edit;
using Polyperfect.Crafting.Framework;
using Polyperfect.Crafting.Integration;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace Polyperfect.Crafting.Edit
{
    [CustomEditor(typeof(BaseCategoryObject), true)]
    public class CategoryObjectEditor : Editor
    {
        IReadOnlyDictionary<RuntimeID, IconData> _iconLookup;

        void OnEnable()
        {
            RefreshIconLookup();
        }

        void RefreshIconLookup() => _iconLookup = EditorItemWorld.Instance.GetReadOnlyAccessor<IconData>(StaticCategories.Icons);

        public override VisualElement CreateInspectorGUI()
        {
            var edited = (BaseCategoryObject) target;
            var ve = new VisualElement();
            var searchField = new ToolbarSearchField().SetGrow();
            searchField.style.width = StyleKeyword.Auto;
            var groupControl = new MemberGroupControl<BaseObjectWithID>(
                () => EditorItemWorld.Instance.ActiveFragments.SelectMany(f=>f.Objects).Where(o => edited.Criteria(o)).Where(o => !edited.Contains(o.ID)).Distinct(),
                () => EditorItemWorld.Instance.ActiveFragments.SelectMany(f=>f.Objects).Where(o => edited.Criteria(o)).Where(o => edited.Contains(o.ID)).Distinct(),
                m => { edited.AddMember(m); },
                m => { edited.RemoveMember(m); },
                (v, item) =>
                {
                    v.Clear();
                    var sprite = _iconLookup.GetDataOrDefault(item).Icon;
                    var displayName = item?item.name.SpacifyCamelCaps().WordwrapOrTruncate(20, 2):"no name";
                    var simpleIconElement = new SimpleIconElement(sprite ? sprite.texture : null, displayName).SetGrow();
                    v.Add(simpleIconElement);
                    simpleIconElement.Add(VisualElementPresets.CreateHoverText(simpleIconElement, displayName));
                },
                (v, item) =>
                {
                    v.Clear();
                    var sprite = _iconLookup.GetDataOrDefault(item).Icon;
                    
                    var displayName = item?item.name.SpacifyCamelCaps().WordwrapOrTruncate(20, 2):"no name";
                    var simpleIconElement = new SimpleIconElement(sprite ? sprite.texture : null, displayName).SetGrow();
                    simpleIconElement.style.width = 64f;
                    v.Add(simpleIconElement);
                    simpleIconElement.Add(VisualElementPresets.CreateHoverText(simpleIconElement, displayName));
                    var inlineEditor = edited.CreateInlineEditor(item, "");
                    v.Add(inlineEditor);
                    inlineEditor.Query<ObjectField>().ForEach(o => o.RegisterValueChangedCallback(e =>
                    {
                        RefreshIconLookup();
                        o.QP<FilterableListview<BaseObjectWithID>>().Rebuild();
                    }));
                },
                o => string.IsNullOrEmpty(searchField.value) || o.name.ToLower().Contains(searchField.value.ToLower())
            );
            groupControl.SetGrow();
            searchField.RegisterValueChangedCallback(e => groupControl.UpdateLists());
            
            var usedIn = new FilterableListview<BaseRecipeObject>(e => true,
                () => EditorItemWorld.Instance.RecipeObjects.Where(r => r.Ingredients.Select(o => o.ID).Contains(edited)).Distinct());
            ItemObjectEditor.SetupRecipeList(usedIn);
            ve.Add(searchField);
            ve.Add(groupControl);
            ve.Add(new Label("USED IN"));
            ve.Add(usedIn);
            return ve;
        }
    }
}