using System;
using UnityEngine;

namespace Craze.UI
{
    /// <summary>
    /// 缓动函数库（Easing Functions）。
    /// 输入 t(0~1) 标准化进度，返回应用缓动后的值。
    /// P5 风格的「弹跳感」主要靠 Back / Elastic / Bounce 三类。
    /// </summary>
    public static class Easing
    {
        public delegate float Function(float t);

        static float Clamp01(float t) => t < 0f ? 0f : (t > 1f ? 1f : t);
        static float Clamp01Multiply(float t, float a) => a * t;

        // ---------- 基础 ----------
        public static float Linear(float t) => t;

        public static float InQuad(float t) => t * t;
        public static float OutQuad(float t) => 1f - (1f - t) * (1f - t);
        public static float InOutQuad(float t) => t < 0.5f ? 2f * t * t : 1f - Mathf.Pow(-2f * t + 2f, 2f) / 2f;

        public static float InCubic(float t) => t * t * t;
        public static float OutCubic(float t) => 1f - Mathf.Pow(1f - t, 3f);
        public static float InOutCubic(float t) => t < 0.5f ? 4f * t * t * t : 1f - Mathf.Pow(-2f * t + 2f, 3f) / 2f;

        // ---------- Back（回拉过头，P5 标志性「弹一下」） ----------
        // s：回拉幅度，默认 1.70158
        public static float InBack(float t) => InBack(t, 1.70158f);
        public static float InBack(float t, float s)
        {
            t = Clamp01(t);
            return t * t * ((s + 1f) * t - s);
        }

        public static float OutBack(float t) => OutBack(t, 1.70158f);
        public static float OutBack(float t, float s)
        {
            t = Clamp01(t) - 1f;
            return t * t * ((s + 1f) * t + s) + 1f;
        }

        public static float InOutBack(float t) => InOutBack(t, 1.70158f);
        public static float InOutBack(float t, float s)
        {
            t = Clamp01(t);
            s *= 1.525f;
            if (t < 0.5f)
            {
                t *= 2f;
                return 0.5f * (t * t * ((s + 1f) * t - s));
            }
            else
            {
                t = t * 2f - 1f;
                return 0.5f * (t * t * ((s + 1f) * t + s) + 2f);
            }
        }

        // ---------- Elastic（弹簧往复，适合强调性缩放） ----------
        public static float InElastic(float t)
        {
            t = Clamp01(t);
            if (t == 0f || t == 1f) return t;
            float p = 0.3f;
            return -Mathf.Pow(2f, 10f * t - 10f) * Mathf.Sin((t * 10f - 0.75f) * (2f * Mathf.PI) / p);
        }

        public static float OutElastic(float t)
        {
            t = Clamp01(t);
            if (t == 0f || t == 1f) return t;
            float p = 0.3f;
            return Mathf.Pow(2f, -10f * t) * Mathf.Sin((t * 10f - 0.75f) * (2f * Mathf.PI) / p) + 1f;
        }

        public static float InOutElastic(float t)
        {
            t = Clamp01(t);
            if (t == 0f || t == 1f) return t;
            float p = 0.45f;
            if (t < 0.5f)
            {
                return -(Mathf.Pow(2f, 20f * t - 10f) * Mathf.Sin((20f * t - 11.125f) * (2f * Mathf.PI) / p)) / 2f;
            }
            else
            {
                return (Mathf.Pow(2f, -20f * t + 10f) * Mathf.Sin((20f * t - 11.125f) * (2f * Mathf.PI) / p)) / 2f + 1f;
            }
        }

        // ---------- Bounce（落地弹跳，适合落地/按钮反馈） ----------
        public static float OutBounce(float t)
        {
            t = Clamp01(t);
            const float n1 = 7.5625f;
            const float d1 = 2.75f;
            if (t < 1f / d1) return n1 * t * t;
            if (t < 2f / d1) { t -= 1.5f / d1; return n1 * t * t + 0.75f; }
            if (t < 2.5f / d1) { t -= 2.25f / d1; return n1 * t * t + 0.9375f; }
            t -= 2.625f / d1;
            return n1 * t * t + 0.984375f;
        }

        public static float InBounce(float t)
        {
            t = Clamp01(t);
            return 1f - OutBounce(1f - t);
        }

        public static float InOutBounce(float t)
        {
            t = Clamp01(t);
            return t < 0.5f
                ? (1f - OutBounce(1f - 2f * t)) / 2f
                : (1f + OutBounce(2f * t - 1f)) / 2f;
        }

        /// <summary>通过名称取缓动函数（供 Inspector / 配置用）。</summary>
        public static Function Get(string name)
        {
            switch (name)
            {
                case "Linear": return Linear;
                case "InQuad": return InQuad;
                case "OutQuad": return OutQuad;
                case "InOutQuad": return InOutQuad;
                case "InCubic": return InCubic;
                case "OutCubic": return OutCubic;
                case "InOutCubic": return InOutCubic;
                case "InBack": return InBack;
                case "OutBack": return OutBack;
                case "InOutBack": return InOutBack;
                case "InElastic": return InElastic;
                case "OutElastic": return OutElastic;
                case "InOutElastic": return InOutElastic;
                case "OutBounce": return OutBounce;
                case "InBounce": return InBounce;
                case "InOutBounce": return InOutBounce;
                default: return Linear;
            }
        }
    }
}
