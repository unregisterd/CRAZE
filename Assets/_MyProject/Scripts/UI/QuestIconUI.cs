using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// NPC 头顶的任务图标。根据任务状态显示黄色感叹号和灰色感叹号
/// </summary>
public class QuestIconUI : MonoBehaviour
{
    public enum IconState
    {
        None,       // 无任务
        Available,  // 可接任务（黄色感叹号）
        InProgress, // 任务进行中（灰色感叹号）
        TurnIn      // 可交付（问号）
    }

    [Header("UI 引用")]
    [SerializeField] private GameObject iconRoot;
    [SerializeField] private Image iconImage;

    [Header("图标")]
    [SerializeField] private Sprite exclamationSprite;   // 感叹号

    [Header("面向相机")]
    [SerializeField] private bool faceCamera = true;

    private string npcName;
    private string[] questIds;

    void Start()
    {
        // 从 NpcInteractable 拿 NPC 名和任务 id
        var npc = GetComponentInParent<NpcInteractable>();
        if (npc != null)
        {
            npcName = npc.NpcDisplayName;
            questIds = npc.QuestIds;
        }

        Refresh();   // 初始化后再刷新
    }

    void OnEnable()
    {
        EventManager<string>.StartListening(GameEvents.QuestStateChanged, OnQuestChanged);
        EventManager<string>.StartListening(GameEvents.QuestStarted, OnQuestChanged);
        EventManager<string>.StartListening(GameEvents.QuestCompleted, OnQuestChanged);
    }

    void OnDisable()
    {
        EventManager<string>.StopListening(GameEvents.QuestStateChanged, OnQuestChanged);
        EventManager<string>.StopListening(GameEvents.QuestStarted, OnQuestChanged);
        EventManager<string>.StopListening(GameEvents.QuestCompleted, OnQuestChanged);
    }

    void Update()
    {
        if (faceCamera && Camera.main != null)
            transform.forward = Camera.main.transform.forward;
    }

    void OnQuestChanged(string questId)
    {
        Refresh();
    }

    private void Refresh()
    {
        var state = GetIconState();
        switch (state)
        {
            case IconState.Available:
                Show(exclamationSprite, Color.yellow);
                break;
            case IconState.TurnIn:
                Show(exclamationSprite, Color.yellow);
                break;
            case IconState.InProgress:
                Show(exclamationSprite, Color.gray);
                break;
            default:
                Hide();
                break;
        }
    }

    private void Show(Sprite sprite, Color color)
    {
        if (iconImage != null)
        {
            iconImage.enabled = true;
            iconImage.sprite = sprite;
            iconImage.color = color;
        }
    }

    private void Hide()
    {
        if (iconImage != null)
            iconImage.enabled = false;
    }

    private IconState GetIconState()
    {
        if (QuestManager.Instance == null || questIds == null || questIds.Length == 0)
            return IconState.None;

        // 1. 有可发布的任务 → 感叹号
        if (QuestManager.Instance.FindPublishableQuest(questIds, npcName) != null)
            return IconState.Available;
            

        // 2. 有可触发的 Dialogue 目标
        var (qid, obj) = QuestManager.Instance.FindActiveDialogueObjective(questIds, npcName);
        if (obj != null)
        {
            bool started = QuestManager.Instance.IsQuestStarted(qid);
            return started ? IconState.TurnIn : IconState.Available;
        }

        // 3. 没有可触发目标 → 看有没有进行中的任务
        foreach (var questId in questIds)
        {
            if (QuestManager.Instance.IsQuestComplete(questId)) continue;
            if (!QuestManager.Instance.IsQuestPrerequisiteMet(questId)) continue;
            if (QuestManager.Instance.IsQuestStarted(questId))
                return IconState.InProgress;
        }

        return IconState.None;
    }
}