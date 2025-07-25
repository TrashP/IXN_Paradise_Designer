using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Polyperfect.Common;
using Polyperfect.Common.Edit;
using Polyperfect.Crafting.Framework;
using Polyperfect.Crafting.Integration;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Profiling;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

namespace Polyperfect.Crafting.Edit
{
    public class ItemFragmentView : VisualElement
    {
        const string MAIN_PATH = "Assets/polyperfect/Crafting System/- UI/- UXML/ItemWorldView.uxml";
        const float iconUpdateTimeAllowed = .005f;
        static readonly List<Type> creatableItemTypes;
        static readonly List<Type> recipeTypes;
        static readonly List<Type> categoryTypes;
        readonly Dictionary<VisualElement, Object> itemLookup = new Dictionary<VisualElement, Object>();

        readonly LinkedList<KeyValuePair<VisualElement, BaseObjectWithID>> remainingIcons = new LinkedList<KeyValuePair<VisualElement, BaseObjectWithID>>();

        readonly List<string> additionalFilters = new List<string>();
        IReadOnlyDictionary<RuntimeID, IconData> _iconAccessor;
        
        [SerializeField] ItemWorldFragment activeFragment;

        static ItemFragmentView()
        {
            var types = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(a => a.GetTypes()).ToList();
            creatableItemTypes = types
                .Where(t => !t.IsAbstract && (t == typeof(BaseItemObject) || t.IsSubclassOf(typeof(BaseItemObject)))).ToList();
            recipeTypes = types.Where(t => !t.IsAbstract && t.IsSubclassOf(typeof(BaseRecipeObject)))
                .ToList();
            categoryTypes = types
                .Where(t => !t.IsAbstract && t.IsSubclassOf(typeof(BaseCategoryObject))).ToList();
        }


        public ItemFragmentView(ItemWorldFragment frag)
        {
            activeFragment = frag;
            var vte = EditorGUIUtility.Load(MAIN_PATH) as VisualTreeAsset;
            if (vte == null)
            {
                Add(new Label($"Did not find uxml file at {MAIN_PATH}"));
                return;
            }

            vte.CloneTree(this);
            Init();
        }

        VisualElement rootVisualElement => this;
        void Init()
        {
            var gridContainer = rootVisualElement.Q("display-grid-view").contentContainer;
            rootVisualElement.schedule.Execute(HandleIconUpdate).Every(16);
            _iconAccessor = EditorItemWorld.Instance.GetReadOnlyAccessor<IconData>(StaticCategories.Icons);

            var searchField = rootVisualElement.Q<ToolbarSearchField>();
            searchField.RegisterValueChangedCallback(e=>HandleQueryChange());

            var gridView =
                CreateGridView(() =>
                {
                    var additionalStr = " ";
                    foreach (var item in additionalFilters)
                    {
                        additionalStr += item;
                        additionalStr += " ";
                    }

                    var useFilter = searchField.value + additionalStr;
                    return !activeFragment
                        ? Common.CollectionExtensions.Empty<BaseItemObject>()
                        : FilterByString(activeFragment.ValidObjects, useFilter);
                });
            gridContainer.Add(gridView);
            
            var newItemMenu = rootVisualElement.Q<ToolbarMenu>("new-item-menu");
            
            foreach (var type in creatableItemTypes)
            {
                var attribute = Attribute.GetCustomAttribute(type, typeof(CreateMenuTitleAttribute),false) as CreateMenuTitleAttribute;
                var displayText = attribute?.Title ?? type.Name;
                newItemMenu.menu.AppendAction(displayText, e => DoCreate(type, "__DirectoryForNewItems"));
            }
            foreach (var type in categoryTypes)
            {
                var attribute = Attribute.GetCustomAttribute(type, typeof(CreateMenuTitleAttribute),false) as CreateMenuTitleAttribute;
                var displayText = attribute?.Title ?? type.Name;
                newItemMenu.menu.AppendAction(displayText, e => DoCreate(type, "__DirectoryForNewCategories"));
            }
            foreach (var type in recipeTypes)
            {
                var attribute = Attribute.GetCustomAttribute(type, typeof(CreateMenuTitleAttribute),false) as CreateMenuTitleAttribute;
                var displayText = attribute?.Title ?? type.Name;
                newItemMenu.menu.AppendAction(displayText, e => DoCreate(type, "__DirectoryForNewRecipes"));
            }

            var allFilter = rootVisualElement.Q<ToolbarToggle>("filter-all");
            var itemFilter = rootVisualElement.Q<ToolbarToggle>("filter-items");
            var categoryFilter = rootVisualElement.Q<ToolbarToggle>("filter-categories");
            var recipeFilter = rootVisualElement.Q<ToolbarToggle>("filter-recipes");
            var filterButtons = new List<ToolbarToggle>(){allFilter,itemFilter,categoryFilter,recipeFilter};

            void SetFilter(string typeName,ToolbarToggle newActive)
            {
                if (!newActive.value)
                    newActive.SetValueWithoutNotify(true);//return;
                
                foreach (var item in filterButtons)
                {
                    if (item ==newActive)
                        continue;
                    item.SetValueWithoutNotify(false);
                }
                additionalFilters.Clear();
                if (!string.IsNullOrEmpty(typeName))
                    additionalFilters.Add($"t:{typeName}");
                
                HandleQueryChange();
            }
            allFilter.RegisterValueChangedCallback(e =>SetFilter("",allFilter));
            itemFilter.RegisterValueChangedCallback(e =>SetFilter("item",itemFilter));
            categoryFilter.RegisterValueChangedCallback(e =>SetFilter("category",categoryFilter));
            recipeFilter.RegisterValueChangedCallback(e =>SetFilter("recipe",recipeFilter));
            
            
            rootVisualElement.AddManipulator(new DataDropManipulator<BaseObjectWithID>(HandleAddToFragment));

            rootVisualElement.schedule.Execute(()=>itemFilter.value = true);
            Undo.undoRedoPerformed += DoRefresh;


            void DoCreate(Type type, string pathLocatorName)
            {
                Assert.IsNotNull(activeFragment);

                var obj = AssetUtility.CreateAsset(type, type.Name, PathLocatorObject.FindDirectory(pathLocatorName));

                AddObjectToActiveWorld(obj);

                var window = ObjectEditWindow.CreateForObject(obj, DoRefresh);
                window.FocusNameEditor();
            }
        }

        void HandleAddToFragment(BaseObjectWithID obj)
        {
            if (activeFragment.Objects.Contains(obj))
                return;
            activeFragment.Objects.Add(obj);
            EditorUtility.SetDirty(activeFragment);
            DoRefresh();
        }

        void AddObjectToActiveWorld(Object obj)
        {
            Assert.IsNotNull(activeFragment);
            var so = new SerializedObject(activeFragment);
            var prop = so.FindProperty(nameof(ItemWorldFragment.Objects));
            var index = prop.arraySize;
            prop.InsertArrayElementAtIndex(index);
            prop.GetArrayElementAtIndex(index).objectReferenceValue = obj;
            so.ApplyModifiedProperties();
        }


        void HandleQueryChange()
        {
            DoRefresh();
        }

        void HandleIconUpdate()
        {
            if (!remainingIcons.Any())
                return;
            var startTime = Time.realtimeSinceStartup;
            while (Time.realtimeSinceStartup - startTime < iconUpdateTimeAllowed && remainingIcons.Any())
            {
                var item = remainingIcons.First.Value;
                remainingIcons.RemoveFirst();
                var label = item.Key.Q<Label>("entry-name");
                var image = item.Key.Q<Image>("entry-icon");
                var sprite = _iconAccessor.GetDataOrDefault(item.Value).Icon;
                image.image = sprite ? sprite.texture : null;
                label.DisplayIf(!sprite);
                image.DisplayIf(sprite);
            }
        }

        IEnumerable<BaseObjectWithID> FilterByString(IEnumerable<BaseObjectWithID> objects, string filterString)
        {
            
            filterString = filterString.ToLower();
            var queries = filterString.Split(new[]{' '}, StringSplitOptions.RemoveEmptyEntries);

            if (!queries.Any())
            {
                foreach (var item in objects)
                    yield return item;
                yield break;
            }

            foreach (var obj in objects)
            {
                var shouldShow = true;
                foreach (var query in queries)
                {
                    var splitByColon = query.Split(':');
                    if (splitByColon.Length==1)
                    {
                        if (!obj.name.ToLower().Contains(splitByColon[0]))
                        {
                            shouldShow = false;
                            break;
                        }
                    }
                    else if (splitByColon.Length == 2)
                    {
                        switch (splitByColon[0])
                        {
                            case "t":
                                if (!obj.GetType().Name.ToLower().Contains(splitByColon[1])) 
                                    shouldShow = false;

                                break;
                        }
                    }
                }

                if (shouldShow)
                    yield return obj;
            }
        }
        GridView<BaseObjectWithID> CreateGridView(Func<IEnumerable<BaseObjectWithID>> getItems)
        {
            var grid = new GridView<BaseObjectWithID>(new Vector2(68, 48),
                () =>
                {
                    var ve = new VisualElement().CenterContents();

                    ve.style.flexDirection = FlexDirection.Row;
                    var iconElement = new Image { name = "entry-icon" };
                    iconElement.style.width = 32f;
                    iconElement.style.height = 32f;
                    ve.Add(iconElement);
                    var labelElement = new Label("Item Name") { name = "entry-name" };
                    labelElement.style.flexGrow = 1f;
                    ve.Add(labelElement);
                    labelElement.style.unityTextAlign = TextAnchor.MiddleCenter;
                    var dragManip = new DataDragManipulator<Object>(() => itemLookup[ve]);
                    var menuManip = new ContextualMenuManipulator(e => PopulateItemContextMenu(e, ve));
                    var openManipulator = new OpenForEditManipulator(() => itemLookup[ve], 1, DoRefresh);
                    var labelContainer = VisualElementPresets.CreateHoverText(ve, "");
                    labelContainer.name = "hover-container";
                    labelContainer.Q<HoverLabel>().OnEnter += () => labelContainer.Q<Label>().text = labelElement.text;//itemLookup[ve] ? itemLookup[ve].name.NewlineSpaces() : "";
                    ve.Add(labelContainer);
                    ve.AddManipulator(dragManip);
                    ve.AddManipulator(openManipulator);
                    ve.AddManipulator(menuManip);
                    return ve;
                },
                (v, o) =>
                {
                    itemLookup[v] = o;
                    var label = v.Q<Label>("entry-name");
                    label.text = o.name.SpacifyCamelCaps().WordwrapOrTruncate(9,3);
                    var image = v.Q<Image>("entry-icon");
                    KeyValuePair<VisualElement, BaseObjectWithID> existing = default;
                    foreach (var pair in remainingIcons)
                        if (pair.Key == v)
                        {
                            existing = pair;
                            break;
                        }

                    if (!existing.Equals(default(KeyValuePair<VisualElement, BaseObjectWithID>)))
                        remainingIcons.Remove(existing);
                    label.Show();
                    image.Hide();
                    remainingIcons.AddLast(new KeyValuePair<VisualElement, BaseObjectWithID>(v, o));
                },
                getItems);

            grid.style.flexGrow = 1f;
            return grid;
        }

        void PopulateItemContextMenu(ContextualMenuPopulateEvent obj, VisualElement ve)
        {
            obj.menu.AppendAction("Copy ID to Clipboard", e => GUIUtility.systemCopyBuffer = (itemLookup[ve] as BaseObjectWithID).ID.NumericID.ToString());
            obj.menu.AppendAction("Clone", e => HandleDuplicate(itemLookup[ve]));
            obj.menu.AppendAction("Delete", e => HandleDelete(itemLookup[ve]));
            obj.menu.AppendAction("Remove From Fragment", e => HandleRemoveFromFragment(itemLookup[ve]));
        }

        void HandleRemoveFromFragment(Object o)
        {
            var so = new SerializedObject(activeFragment);
            var prop = so.FindProperty(nameof(activeFragment.Objects));
            prop.DeleteFromArray(o);
            so.ApplyModifiedPropertiesWithoutUndo();
            DoRefresh();
        }

        void HandleDelete(Object o)
        {
            Assert.IsNotNull(activeFragment);
            if (!EditorUtility.DisplayDialog("Confirm Delete", $"Are you sure you want to delete {o.name}? This cannot be undone.", "Delete", "Cancel"))
                return;
            var so = new SerializedObject(activeFragment);
            var prop = so.FindProperty(nameof(activeFragment.Objects));
            prop.DeleteFromArray(o);
            so.ApplyModifiedPropertiesWithoutUndo();
            foreach (var item in AssetUtility.FindAssetsOfType<BaseCategoryObject>())
            {
                var categorySO = new SerializedObject(item);
                var membersProp = categorySO.FindProperty("members");
                var deleted = membersProp.DeleteFromArray(o);
                if (item is BaseCategoryWithData)
                {
                    var dataProp = categorySO.FindProperty("data");
                    dataProp.DeleteElementSequence(deleted);
                }

                categorySO.ApplyModifiedPropertiesWithoutUndo();
            }

            AssetDatabase.DeleteAsset(AssetDatabase.GetAssetPath(o));

            DoRefresh();
        }


        void HandleDuplicate(Object original)
        {
            var newPath = Path.ChangeExtension(
                Path.Combine(
                    Path.GetDirectoryName(
                        AssetDatabase.GetAssetPath(original)) ?? throw new InvalidOperationException(), original.name + "_Copy"), "asset");
            if (!AssetDatabase.CopyAsset(AssetDatabase.GetAssetPath(original), newPath))
                throw new Exception($"Failed to copy {(original ? original.name : "NULL")}");
            var obj = AssetDatabase.LoadAssetAtPath<Object>(newPath);
            if (obj is BaseObjectWithID withID)
                withID.RandomizeID();
            if (obj is BaseObjectWithID item)
                foreach (var category in AssetUtility.FindAssetsOfType<BaseCategoryObject>().Where(c => c.Contains(original as BaseObjectWithID)))
                    category.DuplicateMemberData(original as BaseObjectWithID, item);
            AddObjectToActiveWorld(obj);
            DoRefresh();
            ObjectEditWindow.CreateForObject(obj);
        }


        void HandleItemCreatePopulate(MouseDownEvent e, IEventHandler b)
        {
            rootVisualElement.panel.contextualMenuManager.DisplayMenu(e, b);
        }

        void DoRefresh()
        {
            Profiler.BeginSample("Refreshing Item Window");
            _iconAccessor = EditorItemWorld.Instance.GetReadOnlyAccessor<IconData>(StaticCategories.Icons);
            rootVisualElement.Query<GridView<BaseObjectWithID>>().ForEach(g => g.Refresh());
            Profiler.EndSample();
        }
    }
}