using UnityEngine;

/// <summary>
/// 可交互物体的通用基类。
/// 处理玩家转向、对话中禁止交互等公共逻辑，子类只实现 OnInteract。
/// </summary>
public abstract class InteractableBase : MonoBehaviour, IInteractable
{
    [Header("交互设置")]
    [SerializeField] private bool faceInteractorOnInteract = true;

    public virtual string GetPrompt() => "F";

    public virtual bool CanInteract(GameObject interactor)
    {
        if (DialogueManager.Instance != null && DialogueManager.Instance.IsTalking)
            return false;
        return true;
    }

    public void Interact(GameObject interactor)
    {
        if (faceInteractorOnInteract)
            FaceInteractor(interactor);
        OnInteract(interactor);
    }

    /// <summary>子类实现具体交互行为。</summary>
    protected abstract void OnInteract(GameObject interactor);

    /// <summary>让物体面向交互者，忽略 Y 轴。</summary>
    protected void FaceInteractor(GameObject interactor)
    {
        if (interactor == null) return;

        Vector3 dir = interactor.transform.position - transform.position;
        dir.y = 0;
        if (dir != Vector3.zero)
            transform.rotation = Quaternion.LookRotation(dir);
    }
}