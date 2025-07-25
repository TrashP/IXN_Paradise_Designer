using System;
using System.Collections;
using UnityEngine;

namespace Polyperfect.Common
{
    /// <summary>
    /// Contains methods for simple tweening to be used via StartCoroutine
    /// </summary>
    public static class TweenExtensions
    {
        /// <summary>
        /// Starts a tweening coroutine on the calling script that oscillates between . 
        /// </summary>
        /// <param name="that">The script to have the coroutine started on.</param>
        /// <param name="duration">Seconds for the tween to take.</param>
        /// <param name="apply">Applies the tween. The value provided will be in the range 0-1.</param>
        /// <param name="onComplete">Invoked after the final apply() call</param>
        public static Coroutine Tween(this MonoBehaviour that, float duration, Action<float> apply,bool unscaled, Action onComplete = null)
        {
            return that.StartCoroutine(Tween(duration, apply, onComplete,unscaled));
        }

        /// <summary>
        /// Starts a tweening coroutine on the calling script that oscillates between . 
        /// </summary>
        /// <param name="that">The script to have the coroutine started on.</param>
        /// <param name="duration">Seconds for one direction of the tween to take.</param>
        /// <param name="apply">Applies the tween. The value provided will be in the range 0-1, and it oscillates back and forth forever.</param>
        /// <param name="phaseOffset">How far to shift T. Generally in the range 0-2, where 1 is a half-cycle</param>
        public static Coroutine TweenPingPong(this MonoBehaviour that, float duration, Action<float> apply,bool unscaled, float phaseOffset = 0f)
        {
            return that.StartCoroutine(TweenPingPong(duration, apply,unscaled,phaseOffset));
        }

        static IEnumerator Tween(float duration,Action<float> act, Action onComplete,bool unscaled)
        {
            var startTime = unscaled?Time.unscaledTime:Time.time;
            var endTime = startTime + duration;
            while ((unscaled?Time.unscaledTime:Time.time) < endTime)
            {
                act(Mathf.InverseLerp(startTime, endTime, unscaled?Time.unscaledTime:Time.time));
                yield return null;
            }

            act(1f);
            onComplete?.Invoke();
        }
        static IEnumerator TweenPingPong(float oneWayDuration,Action<float> apply,bool unscaled, float phaseOffset)
        {
            var val = phaseOffset;
            var durationMul = 1f / oneWayDuration;
            while (true)
            {
                apply(Mathf.PingPong(val,1f));
                yield return null;
                val += (unscaled?Time.unscaledDeltaTime:Time.deltaTime)*durationMul;
            }
        }
    }
    
}