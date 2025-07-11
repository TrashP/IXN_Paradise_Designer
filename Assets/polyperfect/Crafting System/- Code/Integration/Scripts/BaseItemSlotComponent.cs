using Polyperfect.Crafting.Framework;

namespace Polyperfect.Crafting.Integration
{
    public abstract class BaseItemSlotComponent : ItemUserBase, ISlot<Quantity, ItemStack>
    {
        public abstract ItemStack RemainderIfInserted(ItemStack toInsert);
        public abstract ItemStack InsertPossible(ItemStack toInsert);
        public abstract ItemStack Peek(Quantity arg);
        public abstract ItemStack Peek();
        public abstract ItemStack ExtractAll();
        public abstract bool CanExtract();
        public abstract ItemStack ExtractAmount(Quantity arg);
        public abstract event PolyChangeEvent Changed;
    }
}