using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using Craze.UI;   // 引用 CrazeTween

public class SaveSlotItem : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("UI 引用")]
    [SerializeField] private Button slotButton;
    [SerializeField] private TextMeshProUGUI numberText;
    [SerializeField] private TextMeshProUGUI timeText;
    [SerializeField] private Button btnDelete;

    [Header("悬停缩放")]
    [SerializeField] private float hoverScale = 1.1f;
    [SerializeField] private float scaleDuration = 0.25f;

    private int slotIndex;
    private System.Action<int> onClick;
    private System.Action<int> onDelete;

    private Coroutine scaleCoroutine;   // 记录当前缩放协程
    private RectTransform rt;

    void Awake()
    {
        rt = transform as RectTransform;

        if (slotButton == null)
            slotButton = GetComponent<Button>();

        if (slotButton != null)
            slotButton.onClick.AddListener(HandleClick);
        if (btnDelete != null)
            btnDelete.onClick.AddListener(HandleDelete);
    }

    public void Setup(int index, SaveData info, bool allowClickWhenEmpty,
                      System.Action<int> clickCallback, System.Action<int> deleteCallback)
    {
        slotIndex = index;
        onClick = clickCallback;
        onDelete = deleteCallback;

        if (numberText != null)
            numberText.text = index.ToString();

        bool hasSave = info != null;

        if (hasSave)
        {
            if (timeText != null) timeText.text = info.saveTime;
            if (btnDelete != null) btnDelete.gameObject.SetActive(true);
            if (slotButton != null) slotButton.interactable = true;
        }
        else
        {
            if (timeText != null) timeText.text = "";
            if (btnDelete != null) btnDelete.gameObject.SetActive(false);
            if (slotButton != null) slotButton.interactable = allowClickWhenEmpty;
        }
    }

    // ---- 悬停事件 ----

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (slotButton != null && !slotButton.interactable) return;
        PlayScale(Vector3.one * hoverScale);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        PlayScale(Vector3.one);
    }

    // ---- 缩放 ----

    private void PlayScale(Vector3 target)
    {
        if (rt == null) return;

        // 停掉旧的缩放协程，避免冲突
        if (scaleCoroutine != null)
            scaleCoroutine = null;

        scaleCoroutine = CrazeTween.ScaleTo(rt, target, scaleDuration, Easing.OutBack);
    }

    // ---- 点击 ----

    private void HandleClick() => onClick?.Invoke(slotIndex);
    private void HandleDelete() => onDelete?.Invoke(slotIndex);
}