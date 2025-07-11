using UnityEngine;

namespace Polyperfect.Crafting.Edit
{
    public class ReplaceWithPeekInPlaymodeAttribute : PropertyAttribute
    {
        public string IPeekableMemberName { get; }

        public ReplaceWithPeekInPlaymodeAttribute(string iPeekableMemberName)
        {
            IPeekableMemberName = iPeekableMemberName;
        }
    }
}