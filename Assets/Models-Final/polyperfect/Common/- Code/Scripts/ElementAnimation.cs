using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

namespace Polyperfect.Common
{
    public abstract class ElementAnimation : PolyMono
    {
        public enum AnimateMode
        {
            InOut,
            Continuous,
        }

        public enum AnimationState
        {
            None,
            Showing,
            Shown,
            Hiding,
            Hidden
        }

        public AnimateMode Mode = AnimateMode.InOut;
        [SerializeField] protected AnimationState InitialState = AnimationState.None;

        [SerializeField] protected AnimationState StateOnEnable = AnimationState.None;

        //[SerializeField]protected AnimationState StateOnDisable = AnimationState.Hidden;
        protected AnimationState CurrentState { get; set; }

        public float Duration = .5f;
        public AnimationCurve Curve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

        [FormerlySerializedAs("OnShowBegin")] [LowPriority]
        public UnityEvent OnInBegin = new UnityEvent();

        [FormerlySerializedAs("OnShowComplete")] [LowPriority]
        public UnityEvent OnInComplete = new UnityEvent();

        [FormerlySerializedAs("OnHideBegin")] [LowPriority]
        public UnityEvent OnOutBegin = new UnityEvent();

        [FormerlySerializedAs("OnHideComplete")] [LowPriority]
        public UnityEvent OnOutComplete = new UnityEvent();

        public bool Unscaled = true;


        protected virtual void OnEnable()
        {
            if (InitialState != AnimationState.None && CurrentState == AnimationState.None)
                ApplyState(InitialState);
            else if (StateOnEnable != AnimationState.None)
                ApplyState(StateOnEnable);
        }

        void ApplyState(AnimationState state)
        {
            CurrentState = state;
            if (Mode == AnimateMode.Continuous)
            {
                if (enabled && gameObject.activeInHierarchy)
                {
                    if (state == AnimationState.Hiding || state == AnimationState.Shown)
                        BeginPingPong(false);
                    else
                        BeginPingPong(true);
                }
            }
            else
            {
                switch (state)
                {
                    case AnimationState.Showing:
                        CompleteOutImmediate();
                        AnimateIn();
                        break;
                    case AnimationState.Hiding:
                        CompleteInImmediate();
                        AnimateOut();
                        break;
                    case AnimationState.Shown:
                        CompleteInImmediate();
                        break;
                    case AnimationState.Hidden:
                        CompleteOutImmediate();
                        break;
                }
            }
        }

        protected virtual void OnDisable()
        {
            //if (StateOnDisable!=AnimationState.None)
            //ApplyState(StateOnDisable);
            if (CurrentState == AnimationState.Hiding)
                ApplyState(AnimationState.Hidden);
            else if (CurrentState == AnimationState.Showing)
                ApplyState(AnimationState.Shown);
            StopAllCoroutines();
        }


        public void BeginPingPong(bool startInwards)
        {
            StopAllCoroutines();

            if (startInwards)
            {
                CompleteOutImmediate();
                InitAnimateIn();
                this.Tween(Duration, EvaluateIn, Unscaled, ActuallyPingPong);

                void ActuallyPingPong()
                {
                    InitAnimateOut();
                    this.TweenPingPong(Duration, EvaluateOut, Unscaled);
                }
            }
            else
            {
                CompleteInImmediate();
                InitAnimateOut();
                this.Tween(Duration, EvaluateOut, Unscaled, ActuallyPingPong);

                void ActuallyPingPong()
                {
                    InitAnimateIn();
                    this.TweenPingPong(Duration, EvaluateIn, Unscaled);
                }
            }
        }

        protected abstract void InitAnimateIn();

        protected abstract void InitAnimateOut();

        public virtual void AnimateIn()
        {
            CurrentState = AnimationState.Showing;
            StopAllCoroutines();
            InitAnimateIn();
            OnInBegin?.Invoke();
            this.Tween(Duration, EvaluateIn, Unscaled, CompleteIn);
        }

        public virtual void AnimateOut()
        {
            CurrentState = AnimationState.Hiding;
            StopAllCoroutines();
            InitAnimateOut();
            OnOutBegin?.Invoke();
            this.Tween(Duration, EvaluateOut, Unscaled, CompleteOut);
        }

        public void CompleteInImmediate()
        {
            InitAnimateIn();
            EvaluateIn(1);
            CompleteIn();
        }

        public void CompleteOutImmediate()
        {
            InitAnimateOut();
            EvaluateOut(1);
            CompleteOut();
        }

        public virtual void CompleteIn()
        {
            CurrentState = AnimationState.Shown;
            OnInComplete?.Invoke();
        }

        public virtual void CompleteOut()
        {
            CurrentState = AnimationState.Hidden;
            OnOutComplete?.Invoke();
        }

        protected abstract void EvaluateIn(float t);

        protected abstract void EvaluateOut(float t);

        protected void OnValidate()
        {
            var startKey = Curve.keys[0];
            var endKey = Curve.keys[Curve.keys.Length-1];
            startKey.time = 0f;
            startKey.value = 0f;
            endKey.time = 1f;
            endKey.value = 1f;
            Curve.MoveKey(0, startKey);
            Curve.MoveKey(Curve.keys.Length - 1, endKey);
        }

        public void Toggle()
        {
            if (CurrentState == AnimationState.Hidden || CurrentState == AnimationState.Hiding)
                AnimateIn();
            else
                AnimateOut();
        }
    }
}