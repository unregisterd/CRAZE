using System.Collections;
using UnityEngine;

public class Door : MonoBehaviour, IInteractable
{
    [Header("门的旋转设置")]
    [SerializeField] private float openAngle = 90f;
    [SerializeField] private float openSpeed = 3f;

    [Header("钥匙条件")]
    [SerializeField] private bool needKey = false;
    [SerializeField] private string requiredItem = "钥匙";

    private bool isOpen = false;
    private bool hasKey = false;
    private bool isListening = false;
    private Coroutine rotateCoroutine;

    private Quaternion closedRot;
    private Quaternion openRot;

    void Start()
    {
        closedRot = transform.localRotation;
        openRot = closedRot * Quaternion.Euler(0, openAngle, 0);
    }

    void OnEnable()
    {
        // 如果需要钥匙，监听 ItemPickedUp 事件
        if (needKey)
        {
            EventManager<string>.StartListening(GameEvents.ItemPickedUp, OnItemPickedUp);
            isListening = true;
        }
    }

    void OnDisable()
    {
        if (isListening)
        {
            EventManager<string>.StopListening(GameEvents.ItemPickedUp, OnItemPickedUp);
            isListening = false;
        }
    }

    void OnItemPickedUp(object data)
    {
        string item = data as string;
        if (item == requiredItem)
        {
            hasKey = true;
            Debug.Log($"[Door] 已获得 {requiredItem}，可以开门了");
            EventManager<string>.StopListening(GameEvents.ItemPickedUp, OnItemPickedUp);
            isListening = false;
        }
    }

    // 私有方法，唯一的判断入口，以后改规则只改这一个方法
    private bool HasRequiredKey()
    {
        if (!needKey) return true;
        return hasKey;
    }

    public string GetPrompt()
    {
        if (!HasRequiredKey())
            return $"需要 {requiredItem}";
        return "F";
    }

    public bool CanInteract(GameObject interactor)
    {
        if (!HasRequiredKey()) return false;
        return true;
    }

    public void Interact(GameObject interactor)
    {
        isOpen = !isOpen;

        //如果还有旧的旋转协程，先停掉（防止冲突）
        if (rotateCoroutine != null)
            StopCoroutine(rotateCoroutine);

        Quaternion target = isOpen ? openRot : closedRot;
        rotateCoroutine = StartCoroutine(RotateTo(target));

        EventManager<string>.TriggerEvent(
            isOpen ? GameEvents.DoorOpened : GameEvents.DoorClosed,
            gameObject.name
        );
    }
    //用协程。按下 F 时启动一个协程，转完后自动结束。不在动的时候完全没有开销。
    private IEnumerator RotateTo(Quaternion target)
    {
        while (Quaternion.Angle(transform.localRotation, target) > 0.1f)
        {
            transform.localRotation = Quaternion.Slerp(
                transform.localRotation, target, Time.deltaTime * openSpeed);
            yield return null;
        }
        transform.localRotation = target;
        rotateCoroutine = null;
    }
}