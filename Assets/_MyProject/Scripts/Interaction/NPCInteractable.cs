using UnityEngine;

public class NpcInteractable : MonoBehaviour, IInteractable
{
    [SerializeField] private string npcDisplayName = "SwimBoy";

    [Header("参与的任务 ID（可多个）")]
    [SerializeField] private string[] questIds = { "SwimBoyQuest" };

    [Header("没有任何可触发目标时的兜底对话")]
    [SerializeField] private string defaultDialogue = "";

    public string GetPrompt() => "F";
    public string NpcDisplayName => npcDisplayName;
    public string[] QuestIds => questIds;

    public bool CanInteract(GameObject interactor)
    {
        return DialogueManager.Instance != null && !DialogueManager.Instance.IsTalking;
    }

    public void Interact(GameObject interactor)
    {
        // 让 npc 面向玩家
        Vector3 dir = interactor.transform.position - transform.position;
        dir.y = 0;
        if (dir != Vector3.zero)
            transform.rotation = Quaternion.LookRotation(dir);

        // 解析：用哪段对话，对应哪个任务的哪个目标
        var resolution = ResolveDialogue();

        DialogueManager.Instance.StartDialogue(resolution.path, () =>
        {
            // 只有任务对话才推进目标
            if (resolution.isQuestDialogue && QuestManager.Instance != null)
                QuestManager.Instance.CompleteSpecificObjective(
                    resolution.questId, resolution.objectiveId);
        });
    }

    // ---- 内部 ----

    private struct DialogueResolution
    {
        public string path;
        public string questId;
        public string objectiveId;
        public bool isQuestDialogue;   // true=任务对话，false=默认对话
    }

    // 根据 npc 当前所进行的任务目标选择对话路径
    private DialogueResolution ResolveDialogue()
    {
        if (QuestManager.Instance != null)
        {
            var (qid, obj) = QuestManager.Instance.FindActiveDialogueObjective(questIds, npcDisplayName);
            if (obj != null && !string.IsNullOrEmpty(obj.dialoguePath))
            {
                return new DialogueResolution
                {
                    path = obj.dialoguePath,
                    questId = qid,
                    objectiveId = obj.objectiveId,
                    isQuestDialogue = true
                };
            }
        }

        return new DialogueResolution
        {
            path = defaultDialogue,
            questId = null,
            objectiveId = null,
            isQuestDialogue = false
        };
    }
}