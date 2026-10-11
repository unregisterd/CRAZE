using System.Collections.Generic;
using UnityEngine;
using TMPro;

/// <summary>
/// HUD 任务追踪器。显示第一个进行中任务的当前目标。
/// 监听 QuestStateChanged 事件刷新。
/// </summary>
public class QuestTrackerUI : MonoBehaviour
{
    [SerializeField] private GameObject panelRoot;
    [SerializeField] private TextMeshProUGUI questNameText;
    [SerializeField] private TextMeshProUGUI objectiveText;

    void Start()
    {
        if (panelRoot != null)
            panelRoot.SetActive(false);
        Refresh();
    }

    void OnEnable()
    {
        EventManager<string>.StartListening(GameEvents.QuestStateChanged, OnQuestStateChanged);
        EventManager<string>.StartListening(GameEvents.QuestCompleted, OnQuestCompleted);
    }

    void OnDisable()
    {
        EventManager<string>.StopListening(GameEvents.QuestStateChanged, OnQuestStateChanged);
        EventManager<string>.StopListening(GameEvents.QuestCompleted, OnQuestCompleted);
    }

    void OnQuestStateChanged(string questId)
    {
        Refresh();
    }

    void OnQuestCompleted(string questId)
    {
        Refresh();
    }

    private void Refresh()
    {
        if (QuestManager.Instance == null)
        {
            Hide();
            return;
        }

        // 找第一个未完成的任务
        string activeQuestId = null;
        var questIds = QuestManager.Instance.GetActiveQuestIds();

        foreach (var id in questIds)
        {
            if (!QuestManager.Instance.IsQuestComplete(id))
            {
                activeQuestId = id;
                break;
            }
        }

        if (string.IsNullOrEmpty(activeQuestId))
        {
            Hide();
            return;
        }

        var def = QuestManager.Instance.GetDefinition(activeQuestId);
        if (def == null)
        {
            Hide();
            return;
        }

        // 显示任务名
        if (questNameText != null)
            questNameText.text = def.questName;

        // 显示当前目标（取第一个未完成的）
        var objectives = QuestManager.Instance.GetActiveObjectiveProgressTexts(activeQuestId);
        if (objectiveText != null)
        {
            if (objectives.Count == 0)
                objectiveText.text = "";
            else
                objectiveText.text = string.Join("\n", objectives);
        }

        if (panelRoot != null)
            panelRoot.SetActive(true);
    }

    private void Hide()
    {
        if (panelRoot != null)
            panelRoot.SetActive(false);
    }
}