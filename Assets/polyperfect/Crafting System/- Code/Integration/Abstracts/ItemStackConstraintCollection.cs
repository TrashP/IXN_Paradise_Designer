using System.Collections.Generic;
using Polyperfect.Common;
using UnityEngine;

namespace Polyperfect.Crafting.Integration
{
    [CreateAssetMenu(menuName = "Polyperfect/Constraint Collection")]
    public class ItemStackConstraintCollection:PolyObject
    {
        public override string __Usage => "A collection of insertion constraints/processors for slots";
        [SerializeReference] public List<StackInsertionConstraint> Constraints;
        
        
    }
}