using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// 读档加载界面。全屏遮罩 + 旋转图标 + 提示文字。
/// </summary>
public class LoadOverlayUI : MonoBehaviour
{
    [Header("面板")]
    [SerializeField] private GameObject panelRoot;
    [SerializeField] private CanvasGroup canvasGroup;

    [Header("旋转图标")]
    [SerializeField] private RectTransform loadingIcon;
    [SerializeField] private float rotateSpeed = 180f;   // 每秒转多少度

    [Header("文字")]
    [SerializeField] private TextMeshProUGUI loadingText;

    [Header("淡入淡出")]
    [SerializeField] private float fadeDuration = 1f;


    void Start()
    {
        if (panelRoot != null)
            panelRoot.SetActive(false);
        enabled = false;  // 默认不启用 Update
    }

    void Update()
    {
        if (loadingIcon != null)
        {
            // 每帧旋转
            loadingIcon.localEulerAngles -= new Vector3(0, 0, rotateSpeed * Time.unscaledDeltaTime);
        }
    }

    /// <summary>显示加载界面。</summary>
    public void Show(string message = "读取中...")
    {
        //Debug.Log($"[LoadOverlayUI] panelRoot 名字：{(panelRoot != null ? panelRoot.name : "null")}");
        if (loadingText != null)
            loadingText.text = message;

        if (panelRoot != null)
            panelRoot.SetActive(true);

        

        if (canvasGroup != null)
        {
            canvasGroup.alpha = 0f;
            Craze.UI.CrazeTween.Fade(canvasGroup, 1f, fadeDuration, Craze.UI.Easing.OutCubic);
        }
        enabled = true;  // 启用 Update
    }

    /// <summary>隐藏加载界面。</summary>
    public void Hide()
    {

        if (canvasGroup != null)
        {
            Craze.UI.CrazeTween.Fade(canvasGroup, 0f, fadeDuration, Craze.UI.Easing.OutCubic,
                onComplete: () =>
                {
                    if (panelRoot != null)
                        panelRoot.SetActive(false);
                });
        }
        else
        {
            if (panelRoot != null)
                panelRoot.SetActive(false);
        }
        enabled = false;  // 禁用 Update
    }
}