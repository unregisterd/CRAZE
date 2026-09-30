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
        
    }
}