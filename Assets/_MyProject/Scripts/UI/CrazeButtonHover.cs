using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace Craze.UI
{
    /// <summary>
    /// 按钮悬停反馈（P5 风格）：鼠标移入 → 放大 + 微旋转 + 颜色变亮；
    /// 移出 → 平滑还原。由 CrazeMenuController 运行时自动挂到按钮上。
    /// </summary>
    [RequireComponent(typeof(RectTransform))]
    public class CrazeButtonHover : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, ISelectHandler, IDeselectHandler
    {
        [Header("Hover 效果")]
        public float hoverScale = 1.1f;     // 悬停放大倍数
        //public float hoverAngle = -3f;      // 悬停微旋转角度
        public float duration = 0.15f;      // 过渡时长
        public float colorBoost = 1.25f;    // 颜色提亮倍数

        [Header("悬停事件")]
        public int index = -1;                        // 按钮序号（供背景切换等使用）
        public System.Action<int> onHoverEnter;       // 悬停时回调（携带序号）
        public System.Action<int> onHoverExit;        // 移出时回调（携带序号）

        RectTransform _rt;
        Image _img;
        Color _normalColor;
        Vector3 _normalScale = Vector3.one;
        float _normalAngle;

        void Awake()
        {
            _rt = GetComponent<RectTransform>();
            _img = GetComponent<Image>();
            if (_img != null) _normalColor = _img.color;
            if (_rt != null)
            {
                _normalScale = _rt.localScale;
                _normalAngle = _rt.localEulerAngles.z;
            }
        }

        public void OnPointerEnter(PointerEventData eventData) => Focus();
        public void OnSelect(BaseEventData eventData) => Focus();
        public void OnPointerExit(PointerEventData eventData) => Unfocus();
        public void OnDeselect(BaseEventData eventData) => Unfocus();

        void Focus()
        {
            if (_rt == null) return;
            CrazeTween.ScaleTo(_rt, Vector3.one * hoverScale, duration, Easing.OutBack);
            //CrazeTween.RotateTo(_rt, _normalAngle + hoverAngle, duration, Easing.OutCubic);
            if (_img != null)
            {
                _img.color = new Color(
                    Mathf.Min(_normalColor.r * colorBoost, 1f),
                    Mathf.Min(_normalColor.g * colorBoost, 1f),
                    Mathf.Min(_normalColor.b * colorBoost, 1f),
                    _normalColor.a);
            }

            // 通知外部（如背景切换）
            onHoverEnter?.Invoke(index);
        }

        void Unfocus()
        {
            if (_rt == null) return;
            CrazeTween.ScaleTo(_rt, _normalScale, duration, Easing.OutCubic);
            CrazeTween.RotateTo(_rt, _normalAngle, duration, Easing.OutCubic);
            if (_img != null) _img.color = _normalColor;

            onHoverExit?.Invoke(index);
        }
    }
}
