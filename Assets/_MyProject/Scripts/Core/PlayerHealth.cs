using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    public GameObject gameOverUIObject;

    void Start()
    {
        EventManager<string>.DebugPrintListeners(GameEvents.PlayerDied);
    }
   
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.G))
            {
                EventManager<string>.TriggerEvent(GameEvents.PlayerDied, "Sorry for That!");
            }
        if (Input.GetKeyDown(KeyCode.Alpha1))
    EventManager<string>.TriggerEvent(GameEvents.ItemAdded, "泳圈");

if (Input.GetKeyDown(KeyCode.Alpha2))
    EventManager<string>.TriggerEvent(GameEvents.ItemRemoved, "泳圈");
    }
}