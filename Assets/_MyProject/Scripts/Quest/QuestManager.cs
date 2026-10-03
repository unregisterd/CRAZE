using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 任务状态管理器。内部记录每个任务当前进行到第几步。
/// -1 = 未开始，0~steps.Length-1 = 进行中，steps.Length = 已完成。
/// </summary>
public class QuestManager : MonoBehaviour, ISaveable
{
    public static QuestManager Instance { get; private set; }

    [Header("任务配置表")]
    [SerializeField] private QuestDefinition[] questDefinitions;

    public enum QuestState
    {
        NotStarted,
        InProgress,
        Completed
    }

    // questId -> 当前步骤索引
    private Dictionary<string, int> questProgress = new Dictionary<string, int>();

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    void Start()
    {
        if (SaveManager.Instance != null)
            SaveManager.Instance.Register(this);
    }

    void OnDestroy()
    {
        if (SaveManager.Instance != null)
            SaveManager.Instance.Unregister(this);
    }

    // ---- 配置查询 ----

    public QuestDefinition GetDefinition(string questId)
    {
        if (questDefinitions == null) return null;
        foreach (var def in questDefinitions)
        {
            if (def != null && def.questId == questId)
                return def;
        }
        return null;
    }

    // ---- 进度读写 ----

    public int GetProgress(string questId)
    {
        return questProgress.TryGetValue(questId, out int p) ? p : -1;
    }

    public void SetProgress(string questId, int progress)
    {
        questProgress[questId] = progress;
        EventManager<string>.TriggerEvent(GameEvents.QuestStateChanged, questId);
    }

    public QuestState GetState(string questId)
    {
        var def = GetDefinition(questId);
        if (def == null || def.steps == null || def.steps.Length == 0)
            return QuestState.NotStarted;

        int progress = GetProgress(questId);
        if (progress < 0) return QuestState.NotStarted;
        if (progress >= def.steps.Length) return QuestState.Completed;
        return QuestState.InProgress;
    }

    // ---- 对话完成：推进任务 ----

    public void NotifyDialogueComplete(string questId, string npcName)
    {
        var def = GetDefinition(questId);
        if (def == null || def.steps == null || def.steps.Length == 0) return;

        int progress = GetProgress(questId);

        // 未开始：检查第一步
        if (progress < 0)
        {
            if (def.steps[0].npcName == npcName)
                AdvanceStep(questId, def, 0);
            return;
        }

        // 已完成
        if (progress >= def.steps.Length) return;

        // 进行中：检查当前步骤的 npcName
        var step = def.steps[progress];
        if (step.npcName != npcName) return;

        AdvanceStep(questId, def, progress);
    }

    /// <summary>执行当前步骤的物品收发，然后推进到下一步。</summary>
    private void AdvanceStep(string questId, QuestDefinition def, int currentStep)
    {
        var step = def.steps[currentStep];

        // 检查 requireItem
        if (!string.IsNullOrEmpty(step.requireItem) && ItemInventory.Instance != null)
        {
            if (!ItemInventory.Instance.Has(step.requireItem))
            {
                Debug.Log($"[QuestManager] 缺少物品 {step.requireItem}，任务不推进");
                return;
            }
        }

        // 消耗
        if (!string.IsNullOrEmpty(step.consumeItem) && ItemInventory.Instance != null)
            ItemInventory.Instance.Remove(step.consumeItem);

        // 给物品
        if (!string.IsNullOrEmpty(step.giveItem) && ItemInventory.Instance != null)
            ItemInventory.Instance.Add(step.giveItem);

        // 推进
        int next = currentStep + 1;
        SetProgress(questId, next);

        Debug.Log($"[QuestManager] 任务 {questId} 推进到步骤 {next}");
    }

    // ---- ISaveable ----

    public void OnSave(SaveData data)
    {
        var list = new List<QuestEntry>();
        foreach (var kv in questProgress)
        {
            list.Add(new QuestEntry { questId = kv.Key, progress = kv.Value });
        }
        data.questStates = list.ToArray();
    }

    public void OnLoad(SaveData data)
    {
        questProgress.Clear();

        if (data.questStates != null)
        {
            foreach (var entry in data.questStates)
            {
                questProgress[entry.questId] = entry.progress;
            }
        }

        // 通知 UI 刷新
        foreach (var kv in questProgress)
        {
            EventManager<string>.TriggerEvent(GameEvents.QuestStateChanged, kv.Key);
        }
    }

    /// <summary>拿到所有已接任务的 id（未开始的不返回）。</summary>
    public List<string> GetActiveQuestIds()
    {
        var list = new List<string>();
        foreach (var kv in questProgress)
        {
            if (kv.Value >= 0)   // -1 是未开始
                list.Add(kv.Key);
        }
        return list;
    }

    /// <summary>拿到某个任务当前步骤的描述文字，已完成返回"已完成"。</summary>
    public string GetCurrentStepDescription(string questId)
    {
        var def = GetDefinition(questId);
        if (def == null || def.steps == null || def.steps.Length == 0)
            return "";

        int progress = GetProgress(questId);
        if (progress < 0) return "";
        if (progress >= def.steps.Length) return "已完成";

        return def.steps[progress].description;
    }
}