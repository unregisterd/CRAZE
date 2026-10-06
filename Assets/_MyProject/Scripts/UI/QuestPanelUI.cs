using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// 任务面板。打开时动态生成任务列表，点击任务条目显示详情。
/// </summary>
public class QuestPanelUI : MonoBehaviour
{
    [Header("面板")]
    [SerializeField] private GameObject panelRoot;
    [SerializeField] private Button btnClose;

    [Header("任务列表")]
    [SerializeField] private Transform content;              // ScrollView 的 Content
    [SerializeField] private Button questItemPrefab;         // QuestItemPrefab

    [Header("任务详情")]
    [SerializeField] private TextMeshProUGUI detailNameText;
    [SerializeField] private TextMeshProUGUI detailDescText;
    [SerializeField] private TextMeshProUGUI detailObjectiveText;

    [Header("状态颜色")]
    [SerializeField] private Color inProgressColor = Color.white;
    [SerializeField] private Color completedColor = Color.gray;

    private bool isOpen = false;

    // 运行时生成的任务条目
    private readonly List<GameObject> spawnedItems = new List<GameObject>();

    void Start()
    {
        if (panelRoot != null)
            panelRoot.SetActive(false);

        if (btnClose != null)
            btnClose.onClick.AddListener(Close);
    }

    void OnEnable()
    {
        EventManager<string>.StartListening(GameEvents.QuestStateChanged, OnQuestStateChanged);
    }

    void OnDisable()
    {
        EventManager<string>.StopListening(GameEvents.QuestStateChanged, OnQuestStateChanged);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.J))
            Toggle();
    }

    public void Toggle()
    {
        if (isOpen) Close();
        else Open();
    }

    public void Open()
    {
        // 如果已经有其他全屏面板打开，则不允许打开
        if (UIState.IsAnyPanelOpen) return;
        // 对话进行中，不能打开面板
        if (DialogueManager.Instance != null && DialogueManager.Instance.IsTalking) return;

        isOpen = true;
        UIState.IsAnyPanelOpen = true;

        if (panelRoot != null) panelRoot.SetActive(true);

        Time.timeScale = 0f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        RefreshList();
        ClearDetail();
    }

    public void Close()
    {
        isOpen = false;
        UIState.IsAnyPanelOpen = false;

        if (panelRoot != null) panelRoot.SetActive(false);

        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    // ---- 列表刷新 ----

    private void RefreshList()
    {
        ClearList();

        if (QuestManager.Instance == null) return;
        if (content == null || questItemPrefab == null) return;

        var questIds = QuestManager.Instance.GetActiveQuestIds();

        foreach (var questId in questIds)
        {
            CreateQuestItem(questId);
        }
    }

    private void CreateQuestItem(string questId)
    {
        var def = QuestManager.Instance.GetDefinition(questId);
        if (def == null) return;

        Button item = Instantiate(questItemPrefab, content);
        item.gameObject.SetActive(true);

        // 任务名
        var nameText = item.GetComponentInChildren<TextMeshProUGUI>();
        if (nameText != null)
        {
            nameText.text = def.questName;

            // 根据完成状态设置颜色
            bool completed = QuestManager.Instance.IsQuestComplete(questId);
            nameText.color = completed ? completedColor : inProgressColor;
        }

        // 点击显示详情
        string capturedId = questId;
        item.onClick.AddListener(() => ShowDetail(capturedId));

        spawnedItems.Add(item.gameObject);
    }

    private void ClearList()
    {
        foreach (var item in spawnedItems)
        {
            if (item != null)
                Destroy(item);
        }
        spawnedItems.Clear();
    }

    // ---- 详情 ----

    private void ShowDetail(string questId)
    {
        var def = QuestManager.Instance.GetDefinition(questId);
        if (def == null) return;

        if (detailNameText != null) detailNameText.text = def.questName;
        if (detailDescText != null) detailDescText.text = def.description;

        if (detailObjectiveText != null)
        {
            var objectives = QuestManager.Instance.GetActiveObjectiveProgressTexts(questId);
            if (objectives.Count == 0)
                detailObjectiveText.text = "已完成";
            else
                detailObjectiveText.text = string.Join("\n", objectives);
        }
    }

    private void ClearDetail()
    {
        if (detailNameText != null) detailNameText.text = "";
        if (detailDescText != null) detailDescText.text = "";
        if (detailObjectiveText != null) detailObjectiveText.text = "";
    }

    // ---- 事件 ----

    private void OnQuestStateChanged(string questId)
    {
        // 面板打开时才刷新
        if (isOpen)
            RefreshList();
    }
}