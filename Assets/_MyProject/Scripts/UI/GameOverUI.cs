using UnityEngine;
using TMPro;

public class GameOverUI : MonoBehaviour
{
    [Header("UI 引用")]
    [SerializeField] private GameObject promptRoot;   // 整个提示面板
    [SerializeField] private TextMeshProUGUI promptText;         // 提示文字
    void OnEnable()
    {
        EventManager<string>.StartListening(GameEvents.PlayerDied, OnPlayerDied);
    }

    void OnDisable()
    {
        EventManager<string>.StopListening(GameEvents.PlayerDied, OnPlayerDied);
    }

    void Start()
    {
        // 初始时隐藏提示
        if (promptRoot != null)
            promptRoot.SetActive(false);
    }


    void OnPlayerDied(string data)
    {
        if (promptRoot != null)
            promptRoot.SetActive(true);

        if (promptText != null)
            promptText.text = $"Game Over\n{data}";
    }
}