using UnityEngine;
using UnityEngine.UI;
using TMPro;
/// <summary>
/// 显示交互提示的 UI。
/// 监听 InteractableEnter / InteractableExit 事件，控制提示的显隐和文字。
/// </summary>
public class InteractPromptUI : MonoBehaviour
{
    [Header("UI 引用")]
    [SerializeField] private GameObject promptRoot;   // 整个提示面板
    [SerializeField] private TextMeshProUGUI promptText;         // 提示文字

    void OnEnable()
    {
        EventManager<string>.StartListening(GameEvents.InteractableEnter, OnEnter);
        EventManager<string>.StartListening(GameEvents.InteractableExit, OnExit);
    }

    void OnDisable()
    {
        EventManager<string>.StopListening(GameEvents.InteractableEnter, OnEnter);
        EventManager<string>.StopListening(GameEvents.InteractableExit, OnExit);
    }

    void Start()
    {
        // 初始时隐藏提示
        if (promptRoot != null)
            promptRoot.SetActive(false);
    }

    void OnEnter(string data)
    {
        string prompt = data as string;
        if (promptRoot != null)
            promptRoot.SetActive(true);
        if (promptText != null)
            promptText.text = prompt;
    }

    void OnExit(string data)
    {
        if (promptRoot != null)
            promptRoot.SetActive(false);
    }
}