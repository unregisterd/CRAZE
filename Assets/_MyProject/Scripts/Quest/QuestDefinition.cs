using System;
using UnityEngine;

public enum ObjectiveType
{
    Dialogue,
    Collect,
    Interact
}

[Serializable]
public class QuestObjective
{
    public string objectiveId;              // 唯一标识
    public ObjectiveType type;              // 触发类型
    public string targetId;                 // NPC 名 / 物品 ID / 物体 ID
    public string giveItem;   // 完成这个目标时给玩家的物品
    public int requiredCount = 1;           // 需要几次
    public bool required = true;            // 是否必须完成
    public string prerequisiteObjectiveId;  // 前置目标 id，空表示无前置
    [TextArea] public string description;   // 面板显示

    [Header("对话 JSON（仅 Dialogue 类型用）")]
    public string dialoguePath;
}

[CreateAssetMenu(fileName = "QuestDefinition", menuName = "Game/Quest Definition")]
public class QuestDefinition : ScriptableObject
{
    [Header("基本信息")]
    public string questId;
    public string questName;
    [TextArea] public string description;

    [Header("任务目标")]
    public QuestObjective[] objectives;
}