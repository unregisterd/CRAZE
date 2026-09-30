using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// 通用确认面板。用于删除存档等需要二次确认的操作。
/// </summary>
public class ConfirmPanel : MonoBehaviour
{
    [Header("面板")]
    [SerializeField] private GameObject panelRoot;
    [SerializeField] private TextMeshProUGUI messageText;

    [Header("按钮")]
    [SerializeField] private Button btnConfirm;
    [SerializeField] private Button btnCancel;
    [SerializeField] private Button btnQuit;

    private System.Action onConfirm;

    void Start()
    {
        if (panelRoot != null)
            panelRoot.SetActive(false);

        if (btnConfirm != null) btnConfirm.onClick.AddListener(HandleConfirm);
        if (btnCancel != null) btnCancel.onClick.AddListener(Close);
        if (btnQuit != null) btnQuit.onClick.AddListener(Close);
    }

    /// <summary>打开确认面板。</summary>
    public void Open(string message, System.Action confirmCallback)
    {
        onConfirm = confirmCallback;

        if (messageText != null)
            messageText.text = message;

        if (panelRoot != null)
            panelRoot.SetActive(true);
    }

    /// <summary>关闭。</summary>
    public void Close()
    {
        onConfirm = null;
        if (panelRoot != null)
            panelRoot.SetActive(false);
    }

    private void HandleConfirm()
    {
        var callback = onConfirm;
        Close();              // 先关面板，再执行回调
        callback?.Invoke();
    }
}