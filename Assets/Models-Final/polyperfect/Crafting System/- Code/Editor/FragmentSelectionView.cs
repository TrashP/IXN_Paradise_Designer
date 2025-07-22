using System;
using System.IO;
using System.Linq;
using Polyperfect.Common.Edit;
using Polyperfect.Crafting.Integration;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

namespace Polyperfect.Crafting.Edit
{
    public class FragmentSelectionView : VisualElement
    {
        public event Action<ItemWorldFragment> OnFragmentSelected;
        
        const string MAIN_PATH = "Assets/polyperfect/Crafting System/- UI/- UXML/SelectFragmentView.uxml";
        const string README_PATH = "Assets/polyperfect/Crafting System/R E A D M E.pdf";
        
        public FragmentSelectionView()
        {
            var vte = EditorGUIUtility.Load(MAIN_PATH) as VisualTreeAsset;
            if (vte == null)
            {
                Add(new Label($"Did not find uxml file at {MAIN_PATH}"));
                return;
            }

            vte.CloneTree(this);

            var fragmentField = new ObjectField() { objectType = typeof(ItemWorldFragment), allowSceneObjects = false};
            fragmentField.RegisterValueChangedCallback(HandleSelected);
            this.Q("fragment-field-container").Add(fragmentField);

            this.Q<Button>("new-fragment-button").clicked += () =>
            {
                var path = EditorUtility.SaveFilePanel("Item Fragment", Application.dataPath, "My New Fragment", "asset");
                if (string.IsNullOrWhiteSpace(path))
                    return;
                path = path.AbsoluteToRelativePath();
                path = Path.ChangeExtension(path, "asset");
                var fragment = ScriptableObject.CreateInstance<ItemWorldFragment>();
                var iconsCategory = AssetUtility.FindAssetsOfType<IconsCategory>().FirstOrDefault(c => c.ID == StaticCategories.Icons);
                if (iconsCategory)
                    fragment.Objects.Add(iconsCategory);
                AssetDatabase.CreateAsset(fragment,path);
                ChangeFragment(fragment);
            };
            this.Q<Button>("open-readme-button").clicked += () =>
            {
                EditorUtility.OpenWithDefaultApp(README_PATH);
            };

        }

        void HandleSelected(ChangeEvent<Object> evt)
        {
            var selectedFragment = evt.newValue as ItemWorldFragment;
            if (!selectedFragment)
                return;
            ChangeFragment(selectedFragment);
        }

        void ChangeFragment(ItemWorldFragment selectedFragment)
        {
            OnFragmentSelected?.Invoke(selectedFragment);
        }
    }
}