using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Craze.UI
{
    /// <summary>
    /// 轻量补间系统（Tween）。基于协程实现，无外部依赖。
    /// 为 P5 风格的 UI 动效设计：
    ///   - MoveTo / ScaleTo / RotateTo / Fade 四大基础补间
    ///   - 可配缓动函数（Easing），配合 OutBack / OutElastic 获得弹跳感、扫入感
    /// 示例：CrazeTween.ScaleTo(rect, new Vector3(1.1f,1.1f,1f), 0.35f, Easing.OutBack);
    /// </summary>
    public static class CrazeTween
    {
        static MonoBehaviour _runner;
        static MonoBehaviour Runner
        {
            get
            {
                if (_runner == null)
                {
                    var go = new GameObject("[CrazeTween]");
                    UnityEngine.Object.DontDestroyOnLoad(go);
                    _runner = go.AddComponent<CrazeTweenRunner>();
                }
                return _runner;
            }
        }

        // 记录每个 target 当前的缩放协程，防止快速重复调用时冲突
        private static readonly Dictionary<RectTransform, Coroutine> scaleRoutines
            = new Dictionary<RectTransform, Coroutine>();

        // ---------------- 位置 anchoredPosition ----------------
        public static Coroutine MoveTo(RectTransform target, Vector2 endValue, float duration,
            Easing.Function ease = null, float delay = 0f, Action onComplete = null)
        {
            if (target == null) return null;
            ease ??= Easing.Linear;
            return Runner.StartCoroutine(MoveRoutine(target, endValue, duration, ease, delay, onComplete));
        }

        // ---------------- 缩放 localScale ----------------
        public static Coroutine ScaleTo(RectTransform target, Vector3 endValue, float duration,
            Easing.Function ease = null, float delay = 0f, Action onComplete = null)
        {
            if (target == null) return null;

            // 停掉同一个 target 上一次的缩放协程，避免多个协程抢着改 localScale
            if (scaleRoutines.TryGetValue(target, out var old) && old != null)
            {
                Runner.StopCoroutine(old);
            }

            ease ??= Easing.Linear;
            var coroutine = Runner.StartCoroutine(
                ScaleRoutine(target, endValue, duration, ease, delay, onComplete));

            scaleRoutines[target] = coroutine;
            return coroutine;
        }

        // ---------------- 角度 localEulerAngles.z ----------------
        public static Coroutine RotateTo(RectTransform target, float endValue, float duration,
            Easing.Function ease = null, float delay = 0f, Action onComplete = null)
        {
            if (target == null) return null;
            ease ??= Easing.Linear;
            return Runner.StartCoroutine(RotateRoutine(target, endValue, duration, ease, delay, onComplete));
        }

        // ---------------- 透明度 CanvasGroup.alpha ----------------
        public static Coroutine Fade(CanvasGroup group, float endValue, float duration,
            Easing.Function ease = null, float delay = 0f, Action onComplete = null)
        {
            if (group == null) return null;
            ease ??= Easing.Linear;
            return Runner.StartCoroutine(FadeRoutine(group, endValue, duration, ease, delay, onComplete));
        }

        // ---------------- 私有协程实现 ----------------
        static IEnumerator MoveRoutine(RectTransform target, Vector2 end, float duration,
            Easing.Function ease, float delay, Action onComplete)
        {
            if (target == null) yield break;
            Vector2 start = target.anchoredPosition;
            yield return DelayThenAnimate(duration, ease, delay, t =>
            {
                if (target == null) return;   // 目标被销毁，跳过
                target.anchoredPosition = Vector2.LerpUnclamped(start, end, t);
            });
            if (target != null) onComplete?.Invoke();
        }

        static IEnumerator ScaleRoutine(RectTransform target, Vector3 end, float duration,
            Easing.Function ease, float delay, Action onComplete)
        {
            if (target == null) yield break;
            Vector3 start = target.localScale;
            yield return DelayThenAnimate(duration, ease, delay, t =>
            {
                if (target == null) return;   // 目标被销毁，跳过
                target.localScale = Vector3.LerpUnclamped(start, end, t);
            });
            if (target != null) onComplete?.Invoke();
        }

        static IEnumerator RotateRoutine(RectTransform target, float end, float duration,
            Easing.Function ease, float delay, Action onComplete)
        {
            if (target == null) yield break;
            float start = target.localEulerAngles.z;
            yield return DelayThenAnimate(duration, ease, delay, t =>
            {
                if (target == null) return;   // 目标被销毁，跳过
                var e = target.localEulerAngles;
                target.localEulerAngles = new Vector3(e.x, e.y, Mathf.LerpUnclamped(start, end, t));
            });
            if (target != null) onComplete?.Invoke();
        }

        static IEnumerator FadeRoutine(CanvasGroup group, float end, float duration,
            Easing.Function ease, float delay, Action onComplete)
        {
            if (group == null) yield break;
            float start = group.alpha;
            yield return DelayThenAnimate(duration, ease, delay, t =>
            {
                if (group == null) return;   // 目标被销毁，跳过
                group.alpha = Mathf.LerpUnclamped(start, end, t);
            });
            if (group != null) onComplete?.Invoke();
        }

        /// <summary>统一处理延迟 + 时长 + 缓动 + 逐帧回调的底层协程。</summary>
        static IEnumerator DelayThenAnimate(float duration, Easing.Function ease, float delay, Action<float> apply)
        {
            if (delay > 0f)
                yield return new WaitForSecondsRealtime(delay);

            if (duration <= 0f)
            {
                apply(1f);
                yield break;
            }

            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                apply(ease(t));
                yield return null;
            }
            apply(1f);
        }
    }

    /// <summary>供补间系统挂载协程的执行器（自动单例）。</summary>
    public class CrazeTweenRunner : MonoBehaviour { }
}