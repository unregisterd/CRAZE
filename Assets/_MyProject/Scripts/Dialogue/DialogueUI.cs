using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// 对话 UI。显示 NPC 名字、台词、继续按钮、选项按钮。
/// 挂在对话框面板的控制器对象上（一直激活），面板本身初始隐藏。
/// </summary>
public class DialogueUI : MonoBehaviour
{
    [Header("面板")]
    [SerializeField] private GameObject panelRoot;

    [Header("文本")]
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI contentText;

    [Header("继续按钮")]
    [SerializeField] private Button continueButton;

    [Header("选项")]
    [SerializeField] private Transform optionContainer;      // 选项按钮的父对象，挂 Vertical Layout Group
    [SerializeField] private Button optionButtonPrefab;      // 选项按钮预制体

    // 运行时生成的选项按钮
    private readonly List<GameObject> spawnedOptions = new List<GameObject>();

    void Start()
    {
        if (panelRoot != null)
            panelRoot.SetActive(false);

        if (continueButton != null)
            continueButton.onClick.AddListener(OnContinueClicked);
    }

    /// <summary>显示一个对话节点。</summary>
    public void ShowNode(DialogueNode node, string npcName)
    {
        if (node == null) return;

        if (panelRoot != null)
            panelRoot.SetActive(true);

        if (nameText != null)
            nameText.text = npcName;

        if (contentText != null)
            contentText.text = node.text;

        ClearOptions();

        // 有选项 → 生成选项按钮，隐藏继续
        // 无选项 → 显示继续按钮
        bool hasOptions = node.options != null && node.options.Length > 0;

        if (hasOptions)
        {
            if (continueButton != null)
                continueButton.gameObject.SetActive(false);

            for (int i = 0; i < node.options.Length; i++)
            {
                CreateOptionButton(node.options[i], i);
            }
        }
        else
        {
            if (continueButton != null)
                continueButton.gameObject.SetActive(true);
        }
    }

    /// <summary>隐藏对话框。</summary>
    public void Hide()
    {
        ClearOptions();
        if (panelRoot != null)
            panelRoot.SetActive(false);
    }

    // ---- 内部 ----

    private void CreateOptionButton(DialogueOption option, int index)
    {
        if (optionButtonPrefab == null || optionContainer == null) return;

        Button btn = Instantiate(optionButtonPrefab, optionContainer);
        btn.gameObject.SetActive(true);

        // 设置文字
        var text = btn.GetComponentInChildren<TextMeshProUGUI>();
        if (text != null)
            text.text = option.text;

        // 绑定点击，捕获 index（闭包必须用局部变量）
        int capturedIndex = index;
        btn.onClick.AddListener(() => OnOptionClicked(capturedIndex));

        spawnedOptions.Add(btn.gameObject);
    }

    private void ClearOptions()
    {
        foreach (var obj in spawnedOptions)
        {
            if (obj != null)
                Destroy(obj);
        }
        spawnedOptions.Clear();
    }

    private void OnContinueClicked()
    {
        DialogueManager.Instance.Continue();
    }

    private void OnOptionClicked(int index)
    {
        DialogueManager.Instance.SelectOption(index);
    }
}