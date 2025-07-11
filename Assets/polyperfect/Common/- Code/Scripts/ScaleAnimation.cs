using System;
using UnityEditor;
using UnityEngine;

namespace Polyperfect.Common
{
    public class ScaleAnimation:ElementAnimation
    {
        public override string __Usage => "Makes an item scale based on an animation curve.";
        public float  OutScaleMultiplier = 0f;
        public float InScaleMultiplier = 1f;

        Vector3 startScale, endScale;
        bool reverse;
        Vector3 baseScale = Vector3.one;

        void Awake()
        {
            baseScale = transform.localScale;
        }

        protected override void InitAnimateIn()
        {
            startScale = transform.localScale;
            endScale = baseScale*InScaleMultiplier;
        }

        protected override void InitAnimateOut()
        {
            startScale = baseScale*OutScaleMultiplier;
            endScale = transform.localScale;
        }

        protected override void EvaluateOut(float t)
        {
            transform.localScale = Vector3.LerpUnclamped(startScale, endScale, Curve.Evaluate(1-t));
        }

        protected override void EvaluateIn(float t)
        {
            transform.localScale = Vector3.LerpUnclamped(startScale, endScale, Curve.Evaluate(t));
        }
    }
}