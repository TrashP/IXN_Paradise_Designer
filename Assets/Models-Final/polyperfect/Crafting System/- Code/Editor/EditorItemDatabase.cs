using System;
using System.Collections.Generic;
using System.Linq;
using Polyperfect.Crafting.Framework;
using Polyperfect.Crafting.Integration;
using UnityEngine;

namespace Polyperfect.Common.Edit
{

    public class EditorItemWorld:IItemWorld
    {
        public static EditorItemWorld Instance
        {
            get
            {
                if (instance == null)
                    instance = new EditorItemWorld();
                return instance;
            }

        }

        static EditorItemWorld instance;

        
        public List<ItemWorldFragment> ActiveFragments = new List<ItemWorldFragment>();
        public EditorItemWorld()
        {
            var fragment = ScriptableObject.CreateInstance<ItemWorldFragment>();
            var iconsCategory = AssetUtility.FindAssetsOfType<IconsCategory>().FirstOrDefault(f=>f.ID == StaticCategories.Icons);
            if (!iconsCategory)
                Debug.LogError($"The required {nameof(IconsCategory)} is missing. Please ensure there is one with the id {StaticCategories.Icons.NumericID}, or reimport the package.");
            else
                fragment.Objects.Add(iconsCategory);
            
            ActiveFragments.Add(fragment);
        }
        public IReadOnlyDictionary<RuntimeID, VALUE> GetReadOnlyAccessor<VALUE>(RuntimeID arg)
        {
            var categoryObject = ActiveFragments.SelectMany(f => f.CategoryObjects).OfType<BaseCategoryWithData>().FirstOrDefault(c => c.ID == arg);
            if (categoryObject.IsDefault())
                throw new Exception($"The category {arg} does not have data attached or is not one of the active fragments.");
            var lookup = categoryObject.ConstructDictionary();
            return lookup as IReadOnlyDictionary<RuntimeID, VALUE>;
        }

        public bool CategoryContains(RuntimeID category, RuntimeID itemID) => CategoryMembers(category).Contains(itemID);

        public bool HasItem(RuntimeID id)=> ActiveFragments.SelectMany(f => f.ItemObjects).Any(o => o.ID == id);

        public bool HasCategory(RuntimeID id) => ActiveFragments.SelectMany(f => f.CategoryObjects).Any(o => o.ID == id);

        public IEnumerable<RuntimeID> CategoryMembers(RuntimeID categoryID)
        {
            return ActiveFragments.SelectMany(f => f.CategoryObjects).SelectMany(o => o.ValidMembers).Select(i => i.ID).Distinct();
        }  

        public RuntimeID CreateInstance(RuntimeID archetype) => throw new NotSupportedException("Instances not supported at Edit time.");

        public void DeleteInstance(RuntimeID instance) => throw new NotSupportedException("Instances not supported at Edit time.");

        public void SetInstanceData<T>(RuntimeID category, RuntimeID instance, T data) => throw new NotSupportedException("Instances not supported at Edit time.");

        public IEnumerable<RuntimeID> ItemIDs => ActiveFragments.SelectMany(f=>f.ItemObjects).Select(i => i.ID).Distinct();
        public IEnumerable<RuntimeID> CategoryIDs => ActiveFragments.SelectMany(f=>f.CategoryObjects).Select(i => i.ID).Distinct();
        public IEnumerable<RuntimeID> RecipeIDs => ActiveFragments.SelectMany(f=>f.RecipeObjects).Select(i => i.ID).Distinct();
        
        public IEnumerable<BaseItemObject> ItemObjects=>ActiveFragments.SelectMany(f=>f.ItemObjects).Distinct();
        public IEnumerable<BaseCategoryObject> CategoryObjects => ActiveFragments.SelectMany(f => f.CategoryObjects).Distinct();
        public IEnumerable<BaseRecipeObject> RecipeObjects => ActiveFragments.SelectMany(f=>f.RecipeObjects).Distinct();
    }
}