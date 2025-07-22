using System;

namespace Polyperfect.Crafting.Integration
{
    [Serializable]
    public abstract class StackInsertionConstraint
    {
        public struct SlotInsertionContext
        {
            public readonly ItemStack ContainedStack;
            public readonly IItemWorld World;
            public ItemStack ToInsertStack;

            public SlotInsertionContext(IItemWorld world,ItemStack containedStack, ItemStack toInsertStack)
            {
                ContainedStack = containedStack;
                World = world;
                ToInsertStack = toInsertStack;
            }
        }
        public abstract void Process(ref SlotInsertionContext context);
    }
}