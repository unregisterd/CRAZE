using UnityEngine;

public class NpcInteractable : MonoBehaviour, IInteractable
{
    //[SerializeField] private string npcDisplayName = "SwimBoy";
    [SerializeField] private string dialogueJsonPath = "Dialogues/SwimBoyNPC";

    public string GetPrompt()
    {
        return "F";
    }

    public bool CanInteract(GameObject interactor)
    {
        // 已经在对话中就不能再触发
        return !DialogueManager.Instance.IsTalking;
    }

    public void Interact(GameObject interactor)
    {
        // 转向玩家
        Vector3 dir = interactor.transform.position - transform.position;
        dir.y = 0;
        if (dir != Vector3.zero)
            transform.rotation = Quaternion.LookRotation(dir);

        DialogueManager.Instance.StartDialogue(dialogueJsonPath);
    }
}