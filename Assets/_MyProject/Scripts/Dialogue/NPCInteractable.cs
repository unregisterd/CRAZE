using UnityEngine;

public class NpcInteractable : MonoBehaviour, IInteractable
{
    [SerializeField] private string npcDisplayName = "SwimBoy";

    [Header("任务 ID")]
    [SerializeField] private string questId = "SwimBoyQuest";

    [Header("不同状态下的对话 JSON 路径")]
    [SerializeField] private string defaultDialogue = "Dialogues/SwimBoyNPC";
    [SerializeField] private string inProgressDialogue = "Dialogues/SwimBoyNPC_InProgress";
    [SerializeField] private string completedDialogue = "Dialogues/SwimBoyNPC_Completed";


    public string GetPrompt()
    {
        return "F";
    }

    public bool CanInteract(GameObject interactor)
    {
        return DialogueManager.Instance != null && !DialogueManager.Instance.IsTalking;
    }

    public void Interact(GameObject interactor)
    {
        // 转向玩家
        Vector3 dir = interactor.transform.position - transform.position;
        dir.y = 0;
        if (dir != Vector3.zero)
            transform.rotation = Quaternion.LookRotation(dir);

         string path = GetDialoguePath();

        DialogueManager.Instance.StartDialogue(path, () =>
        {
            // 只通知任务系统，不给物品
            if (QuestManager.Instance != null && !string.IsNullOrEmpty(questId))
                QuestManager.Instance.NotifyDialogueComplete(questId, npcDisplayName);
        });
    }
    private string GetDialoguePath()
    {
        if (QuestManager.Instance == null) return defaultDialogue;

        var state = QuestManager.Instance.GetState(questId);

        if (state == QuestManager.QuestState.Completed)
            return string.IsNullOrEmpty(completedDialogue) ? defaultDialogue : completedDialogue;

        if (state == QuestManager.QuestState.InProgress)
        {
            // 判断当前步骤是不是这个 NPC
            var def = QuestManager.Instance.GetDefinition(questId);
            int progress = QuestManager.Instance.GetProgress(questId);

            if (def != null && def.steps != null
                && progress >= 0 && progress < def.steps.Length
                && def.steps[progress].npcName == npcDisplayName)
            {
                return string.IsNullOrEmpty(inProgressDialogue) ? defaultDialogue : inProgressDialogue;
            }

            // 当前步骤不是这个 NPC，走默认对话
            return defaultDialogue;
        }

        // NotStarted
        return defaultDialogue;
    }
}