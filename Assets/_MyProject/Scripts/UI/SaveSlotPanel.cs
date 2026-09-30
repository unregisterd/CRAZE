using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// 存档槽位选择面板。管理多个 SaveSlotItem，支持存档和读档两种模式。
/// </summary>
public class SaveSlotPanel : MonoBehaviour
{
    public enum Mode { Save, Load }

    [Header("面板")]
    [SerializeField] private GameObject panelRoot;

    [Header("提示")]
    [SerializeField] private TextMeshProUGUI titleText; // 面板提示
    [SerializeField] private TextMeshProUGUI emptyClueText; // 空槽提示文本

    [Header("槽位")]
    [SerializeField] private SaveSlotItem[] slots;   // 4 个槽位，按顺序拖进来

    [Header("关闭")]
    [SerializeField] private Button btnClose;

    [Header("确认面板")]
    [SerializeField] private ConfirmPanel confirmPanel;

    private Mode currentMode;                  // 确认当前模式为存档 or 读档
    private System.Action<int> onSlotSelected;   // 选中槽位后的回调
    
    void Start()
    {
        if (panelRoot != null) panelRoot.SetActive(false);
        // 注册按钮的点击事件
        if (btnClose != null) btnClose.onClick.AddListener(Close);
    }

    /// <summary>打开面板。callback 是选中槽位后的回调 </summary>
    public void Open(Mode mode, System.Action<int> callback = null)
    {
        currentMode = mode;
        onSlotSelected = callback;

        if (titleText != null)
            titleText.text = mode == Mode.Save ? "选择存档位置" : "选择读取存档";

        if (panelRoot != null)
            panelRoot.SetActive(true);

        RefreshSlots();
    }

    public void Close()
    {
        if (panelRoot != null)
            panelRoot.SetActive(false);
    }

    /// <summary>刷新所有槽位的显示。</summary>
    private void RefreshSlots()
    {
        if (slots == null || slots.Length == 0)
        {
            if(emptyClueText != null)
            {
                emptyClueText.text = "空空如也~";
            }
                
            return;
        }

        // 存档模式下，空槽位也可点；读档模式下，空槽位不可点
        bool allowEmpty = currentMode == Mode.Save;

        // 判断槽位是否全部为空（显示提示信息）
        bool hasAnySave = false;

        // 遍历全部槽位，刷新显示
        for (int i = 0; i < slots.Length; i++)
        {
            if (slots[i] == null) continue;

            // 获取该槽位的信息
            var info = SaveManager.Instance != null ? SaveManager.Instance.GetSaveInfo(i) : null;

            if (info != null) hasAnySave = true;

            slots[i].Setup(i, info, allowEmpty, OnSlotClicked, OnSlotDeleteClicked);
        }

        if(emptyClueText != null)
        {
            emptyClueText.text = hasAnySave ? "" : "空空如也~";
        }
    }   

    /// <summary>点击槽位。</summary>
    private void OnSlotClicked(int slot)
    {
        //Debug.Log($"[SaveSlotPanel] 选中槽位 {slot}，executeLoad = {executeLoad}");
        if (SaveManager.Instance == null) return;

        // 存档模式
        if (currentMode == Mode.Save)
        {
            if (confirmPanel != null)
            {
                confirmPanel.Open($"确定存储档位 {slot} 吗？", () =>
                {
                    SaveManager.Instance.Save(slot);
                    RefreshSlots();
                });
            }
            else
            {
                // 直接存档到该槽位
                SaveManager.Instance.Save(slot);
                RefreshSlots();   // 存档后刷新显示
            }
            
        }
        // 读档模式
        else
        {
            if (!SaveManager.Instance.HasSave(slot))
            {
                Debug.Log($"[SaveSlotPanel] 槽 {slot} 没有存档");
                return;
            }
            // 如果确认框不为空，显示确认框
            if (confirmPanel != null)
            {
                confirmPanel.Open($"确定读取档位 {slot} 吗？", () =>
                {
                    Close();                        // 关存档面板
                    onSlotSelected?.Invoke(slot);   // 通知 GamePanelUI
                });
            }
            else
            {
                Close();
                onSlotSelected?.Invoke(slot);
            }
        }
    }

    /// <summary>点击删除。暂时直接删，下一步接确认面板。</summary>
    private void OnSlotDeleteClicked(int slot)
    {
        if (SaveManager.Instance == null) return;

        if (confirmPanel == null)
        {
            // 没配确认面板就直接删
            SaveManager.Instance.DeleteSave(slot);
            RefreshSlots();
            return;
        }

        // 弹确认提示
        confirmPanel.Open($"确定删除存档位 {slot} 吗？", () =>
        {
            SaveManager.Instance.DeleteSave(slot);
            RefreshSlots();
        });
    }
}