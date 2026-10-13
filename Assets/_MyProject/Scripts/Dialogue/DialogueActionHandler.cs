using UnityEngine;

/// <summary>
/// 对话选项的动作处理器。根据 action 字符串分发到对应系统。
/// </summary>
public static class DialogueActionHandler
{
    public static void Execute(string action, string param)
    {
        if (string.IsNullOrEmpty(action)) return;

        switch (action)
        {
            case "AcceptQuest":
                if (QuestManager.Instance != null)
                    QuestManager.Instance.StartQuest(param);
                break;

            case "GiveItem":
                if (ItemInventory.Instance != null)
                    ItemInventory.Instance.Add(param);
                break;

            default:
                Debug.LogWarning($"[Dialogue] 未知动作：{action}");
                break;
        }
    }
}