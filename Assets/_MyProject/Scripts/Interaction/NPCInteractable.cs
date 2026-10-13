using UnityEngine;

public class NpcInteractable : InteractableBase
{
    [SerializeField] private string npcDisplayName = "SwimBoy";

    [Header("参与的任务 ID（可多个）")]
    [SerializeField] private string[] questIds = { "SwimBoyQuest" };

    [Header("所有任务都完成后的对话")]
    [SerializeField] private string completedDialogue = "";

    [Header("没有可触发目标时的兜底对话")]
    [SerializeField] private string defaultDialogue = "";

    public string NpcDisplayName => npcDisplayName;
    public string[] QuestIds => questIds;

    protected override void OnInteract(GameObject interactor)
    {
        var resolution = ResolveDialogue();

        DialogueManager.Instance.StartDialogue(resolution.path, () =>
        {
            if (!resolution.isQuestDialogue) return;
            if (QuestManager.Instance == null) return;

            // 任务还没开始？不推进（发布对话结束时）
            if (!QuestManager.Instance.IsQuestStarted(resolution.questId)) return;

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
        public bool isQuestDialogue;   // true=任务对话，结束会推进目标
    }

    private DialogueResolution ResolveDialogue()
    {
        if (QuestManager.Instance == null)
            return Default();

        // 1. 有任务未开始 → 走发布对话
        var publishDef = QuestManager.Instance.FindPublishableQuest(questIds, npcDisplayName);
        if (publishDef != null)
        {
            return new DialogueResolution
            {
                path = publishDef.PublishDialogue,
                questId = publishDef.questId,
                objectiveId = null,
                isQuestDialogue = false
            };
        }

        // 2. 有可触发的任务对话目标 → 走那个目标的 dialoguePath
        var (qid2, obj) = QuestManager.Instance.FindActiveDialogueObjective(questIds, npcDisplayName);
        if (obj != null && !string.IsNullOrEmpty(obj.dialoguePath))
        {
            return new DialogueResolution
            {
                path = obj.dialoguePath,
                questId = qid2,
                objectiveId = obj.objectiveId,
                isQuestDialogue = true
            };
        }

        // 3. 所有任务都完成 → 走完成对话
        if (AllQuestsCompleted() && !string.IsNullOrEmpty(completedDialogue))
        {
            return new DialogueResolution
            {
                path = completedDialogue,
                questId = null,
                objectiveId = null,
                isQuestDialogue = false
            };
        }

        // 4. 兜底
        return Default();
    }

    private DialogueResolution Default()
    {
        return new DialogueResolution
        {
            path = defaultDialogue,
            questId = null,
            objectiveId = null,
            isQuestDialogue = false
        };
    }

    private bool AllQuestsCompleted()
    {
        if (questIds == null || questIds.Length == 0) return false;

        foreach (var qid in questIds)
        {
            if (!QuestManager.Instance.IsQuestComplete(qid))
                return false;
        }
        return true;
    }
}