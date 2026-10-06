using UnityEngine;

/// <summary>
/// 通用交互物体。可以给物品、可以通知任务系统，也可以两者都做。
/// - 只填 itemName：拾取物品
/// - 只填 interactTargetId：交互触发任务
/// - 两个都填：拾取物品 + 触发任务
/// </summary>
public class InteractableObject : MonoBehaviour, IInteractable
{
    [Header("给物品（可选）")]
    [SerializeField] private string itemName = "";

    [Header("任务交互目标（可选）")]
    [SerializeField] private string interactTargetId = "";

    [Header("拾取后隐藏")]
    [SerializeField] private bool hideAfterInteract = true;

    private bool isUsed = false;

    public string GetPrompt()
    {
        return "F";
    }

    public bool CanInteract(GameObject interactor)
    {
        if (isUsed) return false;
        if (DialogueManager.Instance != null && DialogueManager.Instance.IsTalking) return false;
        return true;
    }

    public void Interact(GameObject interactor)
    {
        if (isUsed) return;
        isUsed = true;

        // 给物品
        if (!string.IsNullOrEmpty(itemName) && ItemInventory.Instance != null)
            ItemInventory.Instance.Add(itemName);

        // 通知任务系统
        if (!string.IsNullOrEmpty(interactTargetId) && QuestManager.Instance != null)
            QuestManager.Instance.NotifyObjectiveProgress("Interact", interactTargetId);

        // 隐藏自己
        if (hideAfterInteract)
            gameObject.SetActive(false);
    }
}