using UnityEngine;

namespace Polyperfect.Common
{
    [RequireComponent(typeof(RectTransform))]
    public class PivotAnimation : ElementAnimation
    {
        public override string __Usage => $"Animates the Pivot of the attached {nameof(RectTransform)}.";
        public Vector2 TargetPivot = Vector2.one;
        Vector2 originalPivot;
        Vector2 startPivot;
        RectTransform trans;
        void Awake()
        {
            trans = GetComponent<RectTransform>();
            originalPivot = trans.pivot;
        }

        protected override void InitAnimateIn() => startPivot = trans.pivot;

        protected override void InitAnimateOut() => startPivot = trans.pivot;

        protected override void EvaluateIn(float t) => trans.pivot = Vector3.LerpUnclamped(startPivot, TargetPivot, Curve.Evaluate(t));

        protected override void EvaluateOut(float t) => trans.pivot = Vector3.LerpUnclamped(originalPivot, startPivot, Curve.Evaluate(1-t));
    }
}