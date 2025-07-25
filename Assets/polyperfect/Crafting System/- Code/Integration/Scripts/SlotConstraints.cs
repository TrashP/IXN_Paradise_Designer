using System;
using System.Collections.Generic;
using System.Linq;
using Polyperfect.Common;
using Polyperfect.Crafting.Framework;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Assertions;

namespace Polyperfect.Crafting.Integration
{
    public class MaxCapacityConstraint:StackInsertionConstraint
    {
        public int Capacity = 64;

        public override void Process(ref SlotInsertionContext context) => context.ToInsertStack.Value =   Mathf.Min(Capacity - context.ContainedStack.Value, context.ToInsertStack.Value);
    }

    public class InstancesOnlyConstraint : StackInsertionConstraint
    {
        public bool ForceCreateInstances;
        public override void Process(ref SlotInsertionContext context)
        {
            if (context.ToInsertStack.IsDefault())
                return;
            if (!context.World.IsInstance(context.ToInsertStack.ID))
                context.ToInsertStack = ForceCreateInstances
                    ? new ItemStack(context.World.CreateInstance(context.ToInsertStack.ID), context.ToInsertStack.Value)
                    : new ItemStack();
        }
    }

    public class InstantiateItemsWithCategory : StackInsertionConstraint
    {
        public List<BaseCategoryObject> InstantiateIfContained;
        public override void Process(ref SlotInsertionContext context)
        {
            if (context.ToInsertStack.IsDefault() || context.World.IsInstance(context.ToInsertStack.ID))
                return;
            foreach (var cat in InstantiateIfContained)
            {
                if (context.World.CategoryContains(cat.ID, context.ToInsertStack.ID))
                {
                    Debug.Log("Instantiating");
                    context.ToInsertStack = new ItemStack(context.World.CreateInstance(context.ToInsertStack.ID), context.ToInsertStack.Value);
                    return;
                }
            }
        }
    }

    public class CapacityByCategoryConstraint : StackInsertionConstraint
    {
        [HighlightNull] public CategoryWithInt Category; 
        public override void Process(ref SlotInsertionContext context)
        {
            if (context.World.GetReadOnlyAccessor<int>(Category.ID).TryGetValue(context.ToInsertStack.ID, out var maxCapacity))
                context.ToInsertStack.Value = Mathf.Min(maxCapacity - context.ContainedStack.Value, context.ToInsertStack.Value);
        }
    }

    public class RequiresAllCategoriesConstraint : StackInsertionConstraint
    {
        public List<BaseCategoryObject> RequiredCategories;
        public override void Process(ref SlotInsertionContext context)
        {
            foreach (var cat in RequiredCategories)
            {
                if (!context.World.CategoryContains(cat, context.ToInsertStack.ID))
                {
                    context.ToInsertStack = default;
                    return;
                }
            }
        }
    }
    public class RequiresAnyCategoryConstraint : StackInsertionConstraint
    {
        public List<BaseCategoryObject> RequiredCategories;
        public override void Process(ref SlotInsertionContext context)
        {
            if (RequiredCategories.Count<=0)
                Debug.LogWarning($"The constraint has a {nameof(RequiresAnyCategoryConstraint)} without any given categories. Will always reject inserted items.");
            
            foreach (var cat in RequiredCategories)
            {
                if (context.World.CategoryContains(cat, context.ToInsertStack.ID))
                    return;
            }
            context.ToInsertStack = default;
        }
    }

    public class RequiresItemConstraint : StackInsertionConstraint
    {
        public List<BaseObjectWithID> AcceptedItems;
        public bool AcceptsInstances = true;
        public override void Process(ref SlotInsertionContext context)
        {
            var insertedID = context.ToInsertStack.ID;
            foreach (var accepted in AcceptedItems)
            {
                if (accepted.ID.IsDefault())
                    Debug.LogWarning($"The constraint has a {nameof(RequiresItemConstraint)} without a specified item. Will always reject inserted items.");

                if (insertedID == accepted.ID)
                    return;

                if (AcceptsInstances && context.World.IsInstance(insertedID) && context.World.GetArchetypeFromInstance(insertedID) == accepted.ID)
                    return;
            }

            context.ToInsertStack = default;
        }
    }
}