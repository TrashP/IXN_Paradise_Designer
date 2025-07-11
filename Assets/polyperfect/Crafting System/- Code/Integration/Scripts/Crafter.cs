using System;
using System.Collections.Generic;
using System.Linq;
using Polyperfect.Crafting.Framework;
using UnityEngine;
using UnityEngine.Events;
using TInventoryType =
    Polyperfect.Crafting.Integration.ISlottedInventory<
        Polyperfect.Crafting.Framework.ISlot<Polyperfect.Crafting.Framework.Quantity, Polyperfect.Crafting.Integration.ItemStack>>;

namespace Polyperfect.Crafting.Integration
{
    public class Crafter : ItemUserBase
    {
        public override string __Usage => "Exposes methods for crafting to Unity Events, for things like UGUI Buttons";

        public class ItemListEvent : UnityEvent<List<ItemStack>>
        {
        }

        public UnityEvent
            OnRecipeChange = new UnityEvent(),
            OnSourceUpdate = new UnityEvent(),
            OnDestinationUpdate = new UnityEvent();

        public ItemStackEvent OnItemStackCrafted = new ItemStackEvent();
        public ItemListEvent OnCraftingSuccess = new ItemListEvent();
        public bool ErrorIfInvalidOnCraft = true;

        public SimpleRecipe Recipe
        {
            get => recipe;
            set
            {
                recipe = value;
                OnRecipeChange.Invoke();
            }
        }


        public TInventoryType Source
        {
            get => source;
            set
            {
                if (source is IChangeable changeableOld)
                    changeableOld.Changed -= SendSourceUpdated;
                source = value;
                if (source is IChangeable changeableNew)
                    changeableNew.Changed += SendSourceUpdated;
                OnSourceUpdate.Invoke();
            }
        }

        public TInventoryType Destination
        {
            get => destination;
            set
            {
                if (destination is IChangeable changeableOld)
                    changeableOld.Changed -= SendDestinationUpdated;
                destination = value;
                if (destination is IChangeable changeableNew)
                    changeableNew.Changed += SendDestinationUpdated;
                OnDestinationUpdate.Invoke();
            }
        }

        TInventoryType source;
        TInventoryType destination;
        SimpleRecipe recipe;
        void SendDestinationUpdated() => OnDestinationUpdate.Invoke();

        ISatisfier<IEnumerable<ItemStack>, Quantity> Satisfier; // => internalCrafter.Satisfier;
        Action<TInventoryType, IEnumerable<ItemStack>> extractFromSourceFunction;

        void Awake()
        {
            if (Satisfier == null)
            {
                SetCraftingMethod(new AnyPositionItemQuantitySatisfier<ItemStack>(),
                    (inventory, items) => InventoryOps.ExtractCompletelyFromCollection(inventory.Slots, items));
            }
        }

        public void SetSource(GameObject go) => Source = go.GetComponentInChildren<BaseItemStackInventory>();
        public void ClearSource() => Source = null;
        public void SetDestination(GameObject go) => Destination = go.GetComponentInChildren<BaseItemStackInventory>();
        public void ClearDestination() => Destination = null;
        public void SetRecipe(BaseRecipeObject obj) => Recipe = new SimpleRecipe(obj.Ingredients, obj.Outputs);
        public void ClearRecipe() => Recipe = null;

        public void CraftAmount(int craftAmount)
        {
            if (ErrorIfInvalidOnCraft && !CheckIfValid())
                Debug.LogError("Make sure that Source, Destination, and Recipe are all assigned to the Crafter on " + gameObject.name);

            craftAmount = Mathf.Min(craftAmount, GetMaxCraftAmount(Source.Peek(), Recipe.Requirements));
            if (craftAmount < 1)
                return;
            var factory = new MultiItemFactory(Recipe.Output);
            var ghostCreation = factory.Create(craftAmount).ToList();

            if (
                !InventoryOps.CanInsertCollectionCompletely(ghostCreation,
                    Destination.Slots)) //(!(Destination.Slots.CanInsertCompletelyIntoCollection(ghostCreation)))//InventoryOps.CanInsertCollectionCompletely(ghostCreation, Destination.GetSlots()))
                return;

            extractFromSourceFunction(Source, recipe.Requirements.Multiply(craftAmount));

            InventoryOps.InsertCompletelyIntoCollection(ghostCreation, Destination.Slots);
            foreach (var item in ghostCreation)
                OnItemStackCrafted?.Invoke(item);
            OnCraftingSuccess?.Invoke(ghostCreation);
        }


        public void SetCraftingMethod(ISatisfier<IEnumerable<ItemStack>, Quantity> satisfier, Action<TInventoryType, IEnumerable<ItemStack>> extractionFunction)
        {
            Satisfier = satisfier;
            extractFromSourceFunction = extractionFunction;
        }

        public bool CheckIfValid()
        {
            return !(Source.IsDefault() || Destination.IsDefault() || Satisfier.IsDefault() || Recipe.IsDefault());
        }

        public int GetMaxCraftAmount(IEnumerable<ItemStack> inventory, IEnumerable<ItemStack> inputs)//SimpleRecipe testRecipe)
        {
            if (inputs==null || inventory == null)
                return 0;
            var satisfier = Satisfier;
            
            var suppliedItems = inventory;
            var maxCraftAmount = satisfier.SatisfactionWith(inputs, suppliedItems);
            return maxCraftAmount;
        }

        public int GetMaxCraftAmount(IEnumerable<ItemStack> testRecipe) => GetMaxCraftAmount(Source.Peek(), testRecipe);

        void SendSourceUpdated()
        {
            OnSourceUpdate.Invoke();
        }
    }
}