using System;
using UnityEngine;

/// <summary>
/// 对话管理器。负责加载对话数据、推进节点、处理选项、锁定玩家。
/// </summary>
public class DialogueManager : MonoBehaviour
{
    public static DialogueManager Instance { get; private set; }

    [Header("UI 引用")]
    [SerializeField] private DialogueUI dialogueUI;   // 做好的对话框脚本

    private DialogueData currentData;
    private DialogueNode currentNode;
    private bool isTalking = false;
    private Action onDialogueComplete;

    public bool IsTalking => isTalking;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    /// <summary>从 Resources 加载 JSON 并开始对话。path 不带扩展名，如 "Dialogues/SwimBoyNPC"。</summary>
    public void StartDialogue(string jsonPath, Action onComplete = null)
    {
        // 从 Resources 加载 JSON 文件
        TextAsset jsonFile = Resources.Load<TextAsset>(jsonPath);
        if (jsonFile == null)
        {
            Debug.LogError($"[DialogueManager] 找不到对话文件：{jsonPath}");
            return;
        }
        // 将 JSON 解析为 DialogueData
        DialogueData data = JsonUtility.FromJson<DialogueData>(jsonFile.text);
        if (data == null)
        {
            Debug.LogError($"[DialogueManager] JSON 解析失败：{jsonPath}");
            return;
        }

        StartDialogue(data, onComplete);

        // 广播对话事件开始
        EventManager<string>.TriggerEvent(GameEvents.DialogueStarted, currentData.npcName);
    }

    /// <summary>用已有的 DialogueData 开始对话。</summary>
    public void StartDialogue(DialogueData data, Action onComplete = null)
    {
        if (isTalking) return;
        if (data == null) return;

        currentData = data;
        onDialogueComplete = onComplete;
        isTalking = true;

        // 锁定玩家
        LockPlayer(true);

        // 显示起始节点
        GoToNode(data.GetStartNode());
    }

    /// <summary>玩家点"继续"。</summary>
    public void Continue()
    {
        if (!isTalking) return;
        if (currentNode == null) return;

        // 有选项时必须选选项，不能直接继续
        if (currentNode.options != null && currentNode.options.Length > 0) return;

        if (string.IsNullOrEmpty(currentNode.next))
        {
            EndDialogue();
        }
        else
        {
            GoToNode(currentData.GetNode(currentNode.next));
        }
    }

    /// <summary>玩家选了一个选项。</summary>
    public void SelectOption(int index)
    {
        if (!isTalking) return;
        if (currentNode == null) return;
        if (currentNode.options == null) return;
        if (index < 0 || index >= currentNode.options.Length) return;

        DialogueOption option = currentNode.options[index];

        // 1. 先执行动作
        if (!string.IsNullOrEmpty(option.action))
            DialogueActionHandler.Execute(option.action, option.actionParam);

        // 2. 再跳转
        if (string.IsNullOrEmpty(option.next))
        {
            EndDialogue();
        }
        else
        {
            GoToNode(currentData.GetNode(option.next));
        }
    }

    /// <summary>跳到指定节点。</summary>
    private void GoToNode(DialogueNode node)
    {
        if (node == null)
        {
            EndDialogue();
            return;
        }

        // 条件节点：不显示文本，直接根据物品判断跳分支
        if (!string.IsNullOrEmpty(node.conditionItem))
        {
            bool hasItem = ItemInventory.Instance != null && ItemInventory.Instance.Has(node.conditionItem);
            string nextId = hasItem ? node.nextIfHas : node.nextIfNo;

            if (string.IsNullOrEmpty(nextId))
                EndDialogue();
            else
                GoToNode(currentData.GetNode(nextId));
            return;
        }

        currentNode = node;

        if (dialogueUI != null)
            dialogueUI.ShowNode(node, currentData.npcName);
    }
    /// <summary>结束对话。</summary>
    private void EndDialogue()
    {
        // 先取出 npcName（后面 currentData 会置 null）
        string npcName = currentData != null ? currentData.npcName : "";

        isTalking = false;
        currentNode = null;
        currentData = null;

        // 隐藏 UI
        if (dialogueUI != null)
            dialogueUI.Hide();

        // 解锁玩家
        LockPlayer(false);

        

        // 广播带 npcName 的事件
        EventManager<string>.TriggerEvent(GameEvents.DialogueEnded, npcName);

        // 回调
        onDialogueComplete?.Invoke();
        onDialogueComplete = null;

    }

    /// <summary>锁定/解锁玩家输入。</summary>
    private void LockPlayer(bool locked)
    {
        var player = GameObject.FindGameObjectWithTag("Player");
        if (player == null) return;

        // 1. 禁用输入
        var input = player.GetComponent("vThirdPersonInput") as MonoBehaviour;
        if (input != null)
            input.enabled = !locked;

        // 2. 刚体处理
        var rb = player.GetComponent<Rigidbody>();
        if (rb != null)
        {
            if (locked)
            {
                rb.velocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
                rb.isKinematic = true;
            }
            else
            {
                rb.isKinematic = false;
                rb.velocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
            }
        }

        // 3. 清零动画参数，让它回到 Idle
        var animator = player.GetComponent<Animator>();
        if (animator != null)
        {
            if (locked)
            {
                animator.SetFloat("InputHorizontal", 0f);
                animator.SetFloat("InputVertical", 0f);
                animator.SetFloat("InputMagnitude", 0f);
                //animator.SetFloat("Speed", 0f);
            }   
        }

        // 4. 鼠标
        if (locked)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        else
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }
}