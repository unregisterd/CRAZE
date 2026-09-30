using UnityEngine;

public class DeathSound : MonoBehaviour
{
    void OnEnable()
    {
        EventManager<string>.StartListening(GameEvents.PlayerDied, OnPlayerDied);
    }

    void OnDisable()
    {
        EventManager<string>.StopListening(GameEvents.PlayerDied, OnPlayerDied);
    }

    void OnPlayerDied(string data)
    {
        Debug.Log($"DeathSound 收到：{data} 死亡，播放死亡音效");
    }
}