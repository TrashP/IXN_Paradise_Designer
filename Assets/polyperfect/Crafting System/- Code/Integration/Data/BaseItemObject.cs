using UnityEngine;

namespace Polyperfect.Crafting.Integration
{
    /// <summary>
    ///     A standard item with an ID
    /// </summary>
    [CreateMenuTitle("Items/Simple")]
    [CreateAssetMenu(menuName = "Polyperfect/Items/Simple")]
    public class BaseItemObject : BaseObjectWithID
    {
        public override string __Usage => "A simple implementation of an Item.";
    }
}