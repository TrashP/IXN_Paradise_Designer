using Polyperfect.Common;
using Polyperfect.Crafting.Framework;
using UnityEngine;

namespace Polyperfect.Crafting.Integration.UGUI
{
    public class RecipeDisplay : ItemUserBase
    {
        public override string __Usage => "An easy way of displaying items. They are added as children. Created children can be cleared via Unity Events.";
        public ChildConstructor RequirementsConstructor;
        public ChildConstructor OutputConstructor;
        public void DisplayRecipe(RuntimeID recipeID)
        {
            if (RequirementsConstructor)
            {
                var requirements = World.GetRecipeInputs(recipeID);
                RequirementsConstructor.Construct(requirements,
                        (go, stack) =>
                        {
                            var insertable = go.GetComponent<IInsert<ItemStack>>();
                            if (insertable.IsDefault())
                                Debug.LogError($"No insertable on {go.gameObject}");
                            insertable.InsertPossible(stack);
                        });
            }

            if (OutputConstructor)
            {
                OutputConstructor.Construct(World.GetRecipeOutputs(recipeID), 
                        (go, stack) => go.GetComponent<IInsert<ItemStack>>().InsertPossible(stack));
                
            }
        }

        public void ClearChildren()
        {
            if (RequirementsConstructor)
                RequirementsConstructor.ClearConstructed();
            if (OutputConstructor)
                OutputConstructor.ClearConstructed();
        }
    }
}