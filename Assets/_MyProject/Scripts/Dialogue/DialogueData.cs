using System;

/// <summary>
/// 单个对话选项。
/// </summary>
[Serializable]
public class DialogueOption
{
    public string text;   // 选项文字
    public string next;   // 选了这个跳到哪个节点
}

/// <summary>
/// 单个对话节点。
/// </summary>
[Serializable]
public class DialogueNode
{
    public string id;                    // 节点唯一标识
    public string text;                  // 台词
    public string next;                  // 下一节点 id，空表示结束
    public DialogueOption[] options;     // 选项，空表示线性推进

     // 条件分支
    public string conditionItem;    // 判断这个物品
    public string nextIfHas;        // 有物品跳这里
    public string nextIfNo;         // 没有跳这里
}

/// <summary>
/// 一个 NPC 的完整对话数据。
/// </summary>
[Serializable]
public class DialogueData
{
    public string npcName;               // NPC 名字
    public string startNodeId;           // 起始节点 id
    public DialogueNode[] nodes;         // 所有节点

    /// <summary>按 id 查找节点，找不到返回 null。</summary>
    public DialogueNode GetNode(string id)
    {
        if (nodes == null) return null;
        foreach (var node in nodes)
        {
            if (node.id == id) return node;
        }
        return null;
    }

    /// <summary>获取起始节点。</summary>
    public DialogueNode GetStartNode()
    {
        return GetNode(startNodeId);
    }
}