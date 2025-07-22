using System.Collections.Generic;
using System.Linq;
using Polyperfect.Crafting.Framework;
using UnityEngine;

namespace Polyperfect.Crafting.Integration
{
    public class ItemSlotWithProcessor : ISlot<Quantity, ItemStack>
    {
        Quantity amount;

        RuntimeID id;
        readonly IEnumerable<StackInsertionConstraint> insertionProcessors;
        readonly IItemWorld world;

        public ItemSlotWithProcessor(IEnumerable<StackInsertionConstraint> processors,IItemWorld world)
        {
            insertionProcessors = processors ?? Enumerable.Empty<StackInsertionConstraint>();
            this.world = world;
        }
        
        public ItemStack RemainderIfInserted(ItemStack toInsert)
        {
            if (id.IsDefault() || id.Equals(toInsert.ID))
            {
                var context = CreateContext(toInsert);
                foreach (var processor in insertionProcessors)
                    processor.Process(ref context);

                return new ItemStack(toInsert.ID, toInsert.Value- context.ToInsertStack.Value);
            }

            return toInsert;
        }

        StackInsertionConstraint.SlotInsertionContext CreateContext(ItemStack toInsert)
        {
            var context = new StackInsertionConstraint.SlotInsertionContext(world, Peek(), toInsert);
            return context;
        }

        public ItemStack InsertPossible(ItemStack toInsert)
        {
            if (toInsert.Value <= 0)
                return default;
            var current = Peek();
            if (!current.ID.IsDefault() && !toInsert.ID.Equals(current.ID))
                return toInsert;
            id = toInsert.ID;
            var originalAmount = amount;
            var context = CreateContext(toInsert);
            foreach (var processor in insertionProcessors) 
                 processor.Process(ref context);
            id = context.ToInsertStack.ID;
            amount += context.ToInsertStack.Value;
            if (amount>originalAmount)
                Changed?.Invoke();
            return new ItemStack(toInsert.ID, toInsert.Value - context.ToInsertStack.Value);
        }

        public ItemStack ExtractAll()
        {
            var ret = new ItemStack(id, amount);
            id = default;
            amount = 0;
            if (ret.Value>0)
                Changed?.Invoke();
            return ret;
        }

        public bool CanExtract()
        {
            return !id.IsDefault();
        }

        public ItemStack Peek()
        {
            return new ItemStack(id, amount);
        }

        public ItemStack ExtractAmount(Quantity arg)
        {
            var ret = new ItemStack(id, Mathf.Min(arg, amount));
            amount -= ret.Value;
            
            if (amount <= 0)
                id = default;
            if (ret.Value>0)
                Changed?.Invoke();
            return ret;
        }

        public ItemStack Peek(Quantity arg)
        {
            return new ItemStack(id, Mathf.Min(amount, arg));
        }

        public event PolyChangeEvent Changed;

        
    }
    public static class SlotExtensions
    {
        public static Quantity GetMaxCapacity(this ISlot<Quantity,ItemStack> that)
        {
            if (that.IsDefault())
                return 0;
            var maxedStack = new ItemStack(that.Peek().ID, int.MaxValue);
            var remainder = that.RemainderIfInserted(maxedStack);
            return maxedStack.Value - remainder.Value + that.Peek().Value;
        }
    }
}