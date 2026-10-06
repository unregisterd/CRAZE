using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 任务管理器。每个任务由一组目标组成，每个目标独立记录进度。
/// 目标有前置依赖时形成线性；没有前置时形成网状。
/// </summary>
public class QuestManager : MonoBehaviour, ISaveable
{
    public static QuestManager Instance { get; private set; }

    [Header("任务配置表")]
    [SerializeField] private QuestDefinition[] questDefinitions;

    // questId -> (objectiveId -> 当前计数)
    private Dictionary<string, Dictionary<string, int>> questProgress
        = new Dictionary<string, Dictionary<string, int>>();

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

    // 监听 Collect 类型的物品变化事件，实时刷新进度
    void OnEnable()
    {
        EventManager<string>.StartListening(GameEvents.ItemAdded, OnItemChanged);
        EventManager<string>.StartListening(GameEvents.ItemRemoved, OnItemChanged);
    }

    void OnDisable()
    {
        EventManager<string>.StopListening(GameEvents.ItemAdded, OnItemChanged);
        EventManager<string>.StopListening(GameEvents.ItemRemoved, OnItemChanged);
    }

    private void OnItemChanged(string itemId)
    {
        foreach (var questId in questProgress.Keys)
        {
            EventManager<string>.TriggerEvent(GameEvents.QuestStateChanged, questId);
            // 如果某个任务已经完成，再触发一次 QuestCompleted 事件
            if (IsQuestComplete(questId))
                EventManager<string>.TriggerEvent(GameEvents.QuestCompleted, questId);
        }
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

    public int GetObjectiveProgress(string questId, string objectiveId)
    {
        if (questProgress.TryGetValue(questId, out var dict))
        {
            return dict.TryGetValue(objectiveId, out int count) ? count : 0;
        }
        return 0;
    }

    private void SetObjectiveProgress(string questId, string objectiveId, int count)
    {
        if (!questProgress.ContainsKey(questId))
            questProgress[questId] = new Dictionary<string, int>();

        questProgress[questId][objectiveId] = count;
    }

    // ---- 触发入口 ----

    /// <summary>按类型和目标 ID 推进所有已开始任务中匹配且已解锁的目标。</summary>
    public void NotifyObjectiveProgress(string type, string targetId)
    {
        foreach (var questId in questProgress.Keys)
        {
            var def = GetDefinition(questId);
            if (def == null || def.objectives == null) continue;

            foreach (var obj in def.objectives)
            {
                if (obj.type.ToString() != type) continue;
                if (obj.targetId != targetId) continue;
                if (IsObjectiveComplete(questId, obj)) continue;
                if (!IsPrerequisiteMet(questId, obj)) continue;

                AddProgress(questId, obj);

                if (IsQuestComplete(questId))
                    EventManager<string>.TriggerEvent(GameEvents.QuestCompleted, questId);
            }
        }
    }

    /// <summary>只推进指定任务中的指定目标。</summary>
    /// <summary>完成指定的目标（不做 type/targetId 匹配）。</summary>
    public void CompleteSpecificObjective(string questId, string objectiveId)
    {
        var def = GetDefinition(questId);
        if (def == null || def.objectives == null) return;

        foreach (var obj in def.objectives)
        {
            if (obj.objectiveId != objectiveId) continue;
            if (IsObjectiveComplete(questId, obj)) return;

            AddProgress(questId, obj);

            if (IsQuestComplete(questId))
                EventManager<string>.TriggerEvent(GameEvents.QuestCompleted, questId);
            return;
        }
    }
    /// <summary> 负责增加任务目标的进度。</summary>
    private void AddProgress(string questId, QuestObjective obj)
    {
        // Collect 类型是 “动态计算” ，而 Dialogue 和 Interact 是 “存储计数”
        if (obj.type == ObjectiveType.Collect)
        {
            EventManager<string>.TriggerEvent(GameEvents.QuestStateChanged, questId);
            return;
        }

        // 读取当前目标进度
        int current = GetObjectiveProgress(questId, obj.objectiveId);

        // 添加进度
        SetObjectiveProgress(questId, obj.objectiveId, current + 1);

        // 给物品（如果配置了 giveItem，则给玩家奖励）
        if (!string.IsNullOrEmpty(obj.giveItem) && ItemInventory.Instance != null)
            ItemInventory.Instance.Add(obj.giveItem);

        EventManager<string>.TriggerEvent(GameEvents.QuestStateChanged, questId);
    }

    // ---- 完成判断 ----
    /// <summary>判断某个目标是否完成</summary>
    public bool IsObjectiveComplete(string questId, QuestObjective obj)
    {
        // Collect 类型的目标不存进度，实时查背包
        if (obj.type == ObjectiveType.Collect)
        {
            if (ItemInventory.Instance == null) return false;
            return ItemInventory.Instance.GetCount(obj.targetId) >= obj.requiredCount;
        }
        // Dialogue 和 Interact 类型的目标存进度（字典存储了进度）
        int progress = GetObjectiveProgress(questId, obj.objectiveId);
        return progress >= obj.requiredCount;
    }

    public bool IsPrerequisiteMet(string questId, QuestObjective obj)
    {
        if (string.IsNullOrEmpty(obj.prerequisiteObjectiveId)) return true;

        // 找到当前任务的定义
        var def = GetDefinition(questId);
        if (def == null) return true;

        // 在当前任务定义的步骤中找到对应目标
        foreach (var other in def.objectives)
        {
            // 看前置目标是否完成
            if (other.objectiveId == obj.prerequisiteObjectiveId)
                return IsObjectiveComplete(questId, other);
        }

        return true;
    }

    /// <summary>判断某个任务是否完成</summary>
    public bool IsQuestComplete(string questId)
    {
        // 读取这个任务的定义
        var def = GetDefinition(questId);
        if (def == null || def.objectives == null) return false;

        // 遍历这个任务的所有目标
        foreach (var obj in def.objectives)
        {
            if (!obj.required) continue;
            if (!IsObjectiveComplete(questId, obj)) return false;
        }
        return true;
    }

    // ---- 状态查询 ----

    public bool IsQuestStarted(string questId)
    {
        return questProgress.ContainsKey(questId);
    }

    public List<string> GetActiveQuestIds()
    {
        return new List<string>(questProgress.Keys);
    }

    /// <summary>拿到某个任务所有未完成目标的描述。</summary>
    public List<string> GetActiveObjectiveDescriptions(string questId)
    {
        var list = new List<string>();
        var def = GetDefinition(questId);
        if (def == null || def.objectives == null) return list;

        foreach (var obj in def.objectives)
        {
            if (IsObjectiveComplete(questId, obj)) continue;
            if (!IsPrerequisiteMet(questId, obj)) continue;
            list.Add(obj.description);
        }
        return list;
    }

    /// <summary>拿到某个目标的进度文字，requiredCount <= 1 时不显示进度。</summary>
    public string GetObjectiveProgressText(string questId, QuestObjective obj)
    {
        if (obj.requiredCount <= 1)
            return obj.description;

        if (obj.type == ObjectiveType.Collect && ItemInventory.Instance != null)
        {
            int count = ItemInventory.Instance.GetCount(obj.targetId);
            return $"{obj.description} ({count}/{obj.requiredCount})";
        }

        int progress = GetObjectiveProgress(questId, obj.objectiveId);
        return $"{obj.description} ({progress}/{obj.requiredCount})";
    }

    /// <summary>拿到某个任务所有未完成目标的进度文字列表。</summary>
    public List<string> GetActiveObjectiveProgressTexts(string questId)
    {
        var list = new List<string>();
        var def = GetDefinition(questId);
        if (def == null || def.objectives == null) return list;

        foreach (var obj in def.objectives)
        {
            if (IsObjectiveComplete(questId, obj)) continue;
            if (!IsPrerequisiteMet(questId, obj)) continue;
            list.Add(GetObjectiveProgressText(questId, obj));
        }
        return list;
    }

    // ---- 开始任务 ----

    /// <summary>手动开始一个任务。</summary>
    public void StartQuest(string questId)
    {
        if (questProgress.ContainsKey(questId)) return;
        questProgress[questId] = new Dictionary<string, int>();
        EventManager<string>.TriggerEvent(GameEvents.QuestStateChanged, questId);
    }

    // ---- ISaveable ----

    public void OnSave(SaveData data)
    {
        var questList = new List<QuestEntry>();
        foreach (var kv in questProgress)
        {
            var objList = new List<ObjectiveEntry>();
            foreach (var objKv in kv.Value)
            {
                objList.Add(new ObjectiveEntry
                {
                    objectiveId = objKv.Key,
                    count = objKv.Value
                });
            }
            questList.Add(new QuestEntry
            {
                questId = kv.Key,
                objectives = objList.ToArray()
            });
        }
        data.questStates = questList.ToArray();
    }

    public void OnLoad(SaveData data)
    {
        questProgress.Clear();

        if (data.questStates != null)
        {
            foreach (var questEntry in data.questStates)
            {
                var dict = new Dictionary<string, int>();
                if (questEntry.objectives != null)
                {
                    foreach (var objEntry in questEntry.objectives)
                    {
                        dict[objEntry.objectiveId] = objEntry.count;
                    }
                }
                questProgress[questEntry.questId] = dict;
            }
        }

        foreach (var questId in questProgress.Keys)
        {
            EventManager<string>.TriggerEvent(GameEvents.QuestStateChanged, questId);
        }
    }
}