using System;
using System.Collections;
using UnityEngine;

namespace Polyperfect.Common
{
    public class PositionAnimation : ElementAnimation
    {
        public override string __Usage => "Makes an item translate based on an animation curve.";
        
        public Vector3 Vector = Vector3.up * 10f;
        Vector3 startPos;
        Vector3 fromPos;
        Vector3 toPos;
        bool initted = false;

        void Awake()
        {
            TryInitPosition();
        }

        void TryInitPosition()
        {
            if (initted)
                return;
            initted = true;
            startPos = standardPosition;
        }

       protected override void OnDisable()
        {
            base.OnDisable();
            standardPosition = startPos;
        }

        protected override void InitAnimateIn()
        {
            TryInitPosition();
            fromPos = standardPosition;
            toPos = startPos + Vector;
        }

        protected override void InitAnimateOut()
        {
            TryInitPosition();
            fromPos = startPos;
            toPos = standardPosition;
        }

        protected override void EvaluateIn(float t) => standardPosition = Vector3.LerpUnclamped(fromPos, toPos, Curve.Evaluate(t));
        protected override void EvaluateOut(float t)
        {
            standardPosition = Vector3.LerpUnclamped(fromPos, toPos, Curve.Evaluate(1 - t));
        }

        Vector3 standardPosition
        {
            get
            {
                
                if (transform is RectTransform rect)
                    return rect.anchoredPosition3D;
                return transform.localPosition;
            }
            set
            {
                if (transform is RectTransform rect)
                    rect.anchoredPosition3D = value;
                else 
                    transform.localPosition = value;
            }
        }
    }
}