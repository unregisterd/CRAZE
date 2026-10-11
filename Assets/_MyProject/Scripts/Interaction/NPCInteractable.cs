using UnityEngine;

public class NpcInteractable : InteractableBase
{
    [SerializeField] private string npcDisplayName = "SwimBoy";

    [Header("参与的任务 ID（可多个）")]
    [SerializeField] private string[] questIds = { "SwimBoyQuest" };

    [Header("没有任何可触发目标时的兜底对话")]
    [SerializeField] private string defaultDialogue = "";

    public string NpcDisplayName => npcDisplayName;
    public string[] QuestIds => questIds;

    protected override void OnInteract(GameObject interactor)
    {
        var resolution = ResolveDialogue();

        DialogueManager.Instance.StartDialogue(resolution.path, () =>
        {
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
        public bool isQuestDialogue;
    }

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