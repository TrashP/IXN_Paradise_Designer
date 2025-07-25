using System;
using System.Collections.Generic;
using Polyperfect.Common;
using Polyperfect.Common.Edit;
using Polyperfect.Crafting.Integration;
using UnityEditor;
using UnityEngine;

namespace Polyperfect.Crafting.Edit
{
    public class ItemsWindow : EditorWindow
    {
        
        [MenuItem("Window/Item World")]
        static void CreateWindow() => CreateWindow<ItemsWindow>();

        [SerializeField] ItemWorldFragment persistedFragment;
        static Dictionary<ItemWorldFragment, int> fragmentUsers = new Dictionary<ItemWorldFragment, int>();
        void OnEnable()
        {
            Init();
        }

        void OnDisable()
        {
            if (persistedFragment)
            {
                fragmentUsers[persistedFragment]--;
                HandleUsersUpdated();
            }
        }

        static void HandleUsersUpdated()
        {
            foreach (var item in fragmentUsers)
            {
                var frag = item.Key;
                var count = item.Value;
                
                if (count == 0)
                {
                    if (EditorItemWorld.Instance.ActiveFragments.Contains(frag))
                        EditorItemWorld.Instance.ActiveFragments.Remove(frag);
                }
                else
                {
                    if (!EditorItemWorld.Instance.ActiveFragments.Contains(frag))
                        EditorItemWorld.Instance.ActiveFragments.Add(frag);
                }
            }
        }

        void Init()
        {
            titleContent = new GUIContent("Item World");

            if (persistedFragment)
                SelectFragment(persistedFragment);
            else
            {
                var selection = new FragmentSelectionView();
                selection.OnFragmentSelected += f =>
                {
                    if (!persistedFragment)
                        SelectFragment(f);
                    else
                        Debug.LogError("Avoided catastrophe");
                };
                rootVisualElement.Add(selection);
            } 
        }
        

        void SelectFragment(ItemWorldFragment fragment)
        {
            if (!fragmentUsers.ContainsKey(fragment))
                fragmentUsers.Add(fragment,0);
            fragmentUsers[fragment]++;
            HandleUsersUpdated();
            
            persistedFragment = fragment;
            titleContent = new GUIContent(fragment.name);
            rootVisualElement.Clear();
            rootVisualElement.Add(new ItemFragmentView(fragment).SetGrow());
        }
    }
}