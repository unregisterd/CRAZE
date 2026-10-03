using UnityEngine;
using System;


/// <summary>
/// 任务中的一步。描述和谁对话、给什么、要什么。
/// </summary>
[Serializable]
public class QuestStep
{
    public string npcName;        // 和哪个 NPC 对话触发这一步
    public string giveItem;       // 这一步给玩家的物品（可选）
    public string requireItem;    // 这一步需要玩家持有的物品（可选）
    public string consumeItem;    // 这一步消耗的物品（可选）
    [TextArea] public string description;   // 任务面板显示的这一步描述
}

// QuestDefinition 为步骤制
[CreateAssetMenu(fileName = "QuestDefinition", menuName = "Game/Quest Definition")]
public class QuestDefinition : ScriptableObject
{
    [Header("基本信息")]
    public string questId;
    public string questName;
    [TextArea] public string description;

    [Header("任务步骤")]
    public QuestStep[] steps;
}