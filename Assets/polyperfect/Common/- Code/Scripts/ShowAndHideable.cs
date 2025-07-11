using System.Collections;
using UnityEngine;

namespace Polyperfect.Common
{
    public class ShowAndHideable : ElementAnimation
    {
        public override string __Usage => "Handling of showing and hiding...something.";
        public bool Shown => CurrentState == AnimationState.Shown || CurrentState == AnimationState.Showing;//{ get; private set; }

        public bool AutoActivation = true;

        public void Show() => AnimateIn();

        public void Hide() => AnimateOut();
        
        protected override void InitAnimateIn()
        {
            if (AutoActivation && !gameObject.activeSelf) 
                gameObject.SetActive(true);
        }

        protected override void InitAnimateOut()
        {
        }

        public override void AnimateIn()
        {
            if (Shown)
                return;
            if (AutoActivation)
                gameObject.SetActive(true);
            base.AnimateIn();
        }

        public override void CompleteOut()
        {
            base.CompleteOut();
            if (AutoActivation)
                gameObject.SetActive(false);
        }

        protected override void EvaluateIn(float t) { }

        protected override void EvaluateOut(float t) { }
    }

    
}