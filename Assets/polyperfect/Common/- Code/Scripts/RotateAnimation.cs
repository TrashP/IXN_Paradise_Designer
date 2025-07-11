using System;
using UnityEngine;

namespace Polyperfect.Common
{
    public class RotateAnimation:PolyMono
    {
        public override string __Usage => "Makes an item rotate based on an animation curve.";
        [SerializeField] AnimateMode Mode;
        public float Duration = .5f;
        public float Magnitude = 360f;
        public Vector3 Axis = Vector3.up;
        public AnimationCurve Curve = AnimationCurve.EaseInOut(0f,0f,1f,1f);
        public bool Unscaled = true;
        Quaternion startingRotation;
        public enum AnimateMode
        {
            InOut,
            Continuous
        }
        void Awake()
        {
            startingRotation = transform.localRotation;
        }

        void OnEnable()
        {
            switch (Mode)
            {
                case AnimateMode.InOut:
                    AnimateIn();
                    break;
                case AnimateMode.Continuous:
                    this.TweenPingPong(Duration,t => transform.localRotation = startingRotation*Quaternion.AngleAxis(Magnitude*Curve.Evaluate(t),Axis),Unscaled);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }                
        }

        void OnDisable()
        {
            StopAllCoroutines();
        }

        public void AnimateIn()
        {
            StopAllCoroutines();
            this.Tween(Duration,
                t => transform.localRotation = startingRotation*Quaternion.AngleAxis(Magnitude*Curve.Evaluate(t),Axis),Unscaled);
        }

        public void AnimateOut()
        {
            StopAllCoroutines();
            var curStartRotation = transform.localRotation;
            this.Tween(Duration,
                t => transform.localRotation = curStartRotation*Quaternion.AngleAxis(Magnitude*Curve.Evaluate(1-t),Axis),Unscaled);
        }
    }
}