using Polyperfect.Common;
using UnityEngine;

namespace Polyperfect.Crafting.Integration
{
    public abstract class ItemUserBase : PolyMono
    {
        protected IItemWorld World => ItemWorldReference.Instance.World;

    }
}