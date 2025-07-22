using System;
using System.Collections.Generic;
using Polyperfect.Common;
using Polyperfect.Crafting.Framework;
using UnityEngine;
using UnityEngine.Assertions;

namespace Polyperfect.Crafting.Integration
{
    public static class ItemWorldExtensions
    {
        public static IReadOnlyDictionary<RuntimeID, string> GetNameLookup(this IItemWorld that)
        {
            var accessor = that.GetReadOnlyAccessor<string>(StaticCategories.Names);
            Assert.IsTrue(accessor!=null);
            return accessor;
        }

        public static string GetName(this IItemWorld that,RuntimeID id) =>
            that.GetNameLookup()[id];

        public static RuntimeID GetIDSlow(this IItemWorld that, string name)
        {
            var accessor = that.GetNameLookup();
            foreach (var item in accessor)
            {
                if (item.Value == name)
                    return item.Key;
            }

            throw new KeyNotFoundException();
        }


        public static RuntimeID GetArchetypeFromInstance(this IItemWorld that, RuntimeID instancedItem)
        {
            return that.GetReadOnlyAccessor<RuntimeID>(StaticCategories.Archetypes)[instancedItem];
        }

        public static bool IsInstance(this IItemWorld that, RuntimeID id)
        {
            return that.CategoryContains(StaticCategories.Archetypes,id);
        }
        
        public static T GetInstanceData<T>(this IItemWorld that, RuntimeID category, RuntimeID instance)
        {
            return that.GetReadOnlyAccessor<T>(category).GetDataOrDefault(instance);
        }

        public static IEnumerable<ItemStack> GetRecipeInputs(this IItemWorld that, RuntimeID recipeID)
        {
            if (!that.CategoryContains(StaticCategories.RecipeInputs, recipeID))
            {
                Debug.LogError($"No Inputs for id {recipeID}");
                yield break;
            }

            that.GetReadOnlyAccessor<IEnumerable<ItemStack>>(StaticCategories.RecipeInputs).TryGetValue(recipeID, out var ret);
            
            if (ret == null)
                yield break;
            
            foreach (var item in ret)
                yield return item;
        }
        public static IEnumerable<ItemStack> GetRecipeOutputs(this IItemWorld that, RuntimeID recipeID)
        {
            if (!that.CategoryContains(StaticCategories.RecipeOutputs, recipeID))
            {
                Debug.LogError($"No Outputs for id {recipeID}");
                yield break;
            }

            that.GetReadOnlyAccessor<IEnumerable<ItemStack>>(StaticCategories.RecipeOutputs).TryGetValue(recipeID, out var ret);
            
            if (ret == null)
                yield break;
            
            foreach (var item in ret)
                yield return item;
        }

        public static SimpleRecipe GetSimpleRecipe(this IItemWorld that, RuntimeID recipeID)
        {
            var inputs = that.GetRecipeInputs(recipeID);
            var outputs = that.GetRecipeOutputs(recipeID);

            if (inputs == null || outputs == null)
                return null;
            
            return new SimpleRecipe(inputs, outputs);
        }

        [Obsolete("The recipes access process has been replaced with a category-based method. There are some extension methods you can use however: GetRecipeInputs and GetRecipeOutputs")]
        public static void Recipes(this IItemWorld that) { }
    }
}