using UnityEngine;

/// <summary>
/// 可拾取的物品，比如钥匙。
/// 玩家按 F 拾取后，物体消失，广播 ItemPickedUp 事件。
/// </summary>
public class PickupItem : MonoBehaviour, IInteractable
{
    [SerializeField] private string itemName = "钥匙";

    private bool isPicked = false;

    public string GetPrompt()
    {
        return "F";
    }

    public bool CanInteract(GameObject interactor)
    {
        return !isPicked;
    }

    public void Interact(GameObject interactor)
    {
        if (isPicked) return;

        isPicked = true;

        // 广播拾取事件，把物品名传出去
        EventManager<string>.TriggerEvent(GameEvents.ItemPickedUp, itemName);

        // 隐藏或销毁自己
        gameObject.SetActive(false);
        // 或者 Destroy(gameObject);
    }
}