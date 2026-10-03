using UnityEngine;

/// <summary>
/// 玩家交互检测器。用 OverlapSphere 找最近的可交互物体，
/// 玩家按 F 时触发交互。
/// 对话中通过事件系统禁用自己，避免对话时误触发交互。
/// </summary>
public class PlayerInteractor : MonoBehaviour
{
    [SerializeField] private float detectRadius = 2.5f;
    [SerializeField] private LayerMask interactableLayer;

    private bool interactEnabled = true; // 标记当前是否能够交互

    private IInteractable currentInteractable;

    void OnEnable()
    {
        EventManager<string>.StartListening(GameEvents.DialogueStarted, OnDialogueStarted);
        EventManager<string>.StartListening(GameEvents.DialogueEnded, OnDialogueEnded);
    }

    void OnDisable()
    {
        EventManager<string>.StopListening(GameEvents.DialogueStarted, OnDialogueStarted);
        EventManager<string>.StopListening(GameEvents.DialogueEnded, OnDialogueEnded);
    }

    void Update()
    {
        if (!interactEnabled) return;
        if (UIState.IsAnyPanelOpen) return;   // 面板开着，不检测交互
        
        FindNearestInteractable();

        if (currentInteractable != null && Input.GetKeyDown(KeyCode.F))
        {
            if (currentInteractable.CanInteract(gameObject))
            {
                currentInteractable.Interact(gameObject);
            }
        }
    }

    private void FindNearestInteractable()
    {
        Collider[] hits = Physics.OverlapSphere(
            transform.position,
            detectRadius,
            interactableLayer
        );

        IInteractable nearest = null;
        float nearestDist = float.MaxValue;

        foreach (var hit in hits)
        {
            var interactable = hit.GetComponent<IInteractable>();
            if (interactable == null) continue;

            float dist = Vector3.Distance(transform.position, hit.transform.position);
            if (dist < nearestDist)
            {
                nearestDist = dist;
                nearest = interactable;
            }
        }

        if (nearest != currentInteractable)
        {
            currentInteractable = nearest;

            if (currentInteractable != null)
                EventManager<string>.TriggerEvent(GameEvents.InteractableEnter, currentInteractable.GetPrompt());
            else
                EventManager<string>.TriggerEvent(GameEvents.InteractableExit, "");
        }
    }

    // ---- 对话事件 ----

    private void OnDialogueStarted(string data)
    {
        // 清掉当前交互提示
        if (currentInteractable != null)
        {
            currentInteractable = null;
            EventManager<string>.TriggerEvent(GameEvents.InteractableExit, "");
        }

        // 关闭所有的交互
        interactEnabled = false;
    }

    private void OnDialogueEnded(string data)
    {
        // 恢复检测
        interactEnabled = true;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectRadius);
    }
}