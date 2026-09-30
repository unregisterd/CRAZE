using UnityEngine;

public class DoorSound : MonoBehaviour
{
    void OnEnable()
    {
        EventManager<string>.StartListening(GameEvents.DoorOpened, OnDoorOpened);
        EventManager<string>.StartListening(GameEvents.DoorClosed, OnDoorClosed);
    }

    void OnDisable()
    {
        EventManager<string>.StopListening(GameEvents.DoorOpened, OnDoorOpened);
        EventManager<string>.StopListening(GameEvents.DoorClosed, OnDoorClosed);
    }

    void OnDoorOpened(string data)
    {
        Debug.Log($"DoorSound 收到：门 {data} 打开了，播放开门音效");
    }

    void OnDoorClosed(string data)
    {
        Debug.Log($"DoorSound 收到：门 {data} 关上了，播放关门音效");
    }
}