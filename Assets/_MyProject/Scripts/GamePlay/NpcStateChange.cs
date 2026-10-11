using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class NpcStateChange : MonoBehaviour
{
    [Header("npc的名字")]
    [SerializeField] private string npcName = "艾伦";
    private enum ObjectiveType
    {
        Bool,
        Trigger
    }
    [Header("切换状态的参数类型")]
    [SerializeField] private ObjectiveType parameter;

    [Header("需要切换的状态名")]
    [SerializeField] private string changeState = "toRun";

    [Header("指定任务（开始后切换状态，结束后切换回原来的状态）")]
    [SerializeField] private string m_questId = "SwimBoyQuest2";

    [Header("移动路径")]
    [SerializeField] private Transform patrolRoute;
    [SerializeField] private List<Transform> locations;

    private Animator anim;
    private bool canChange = false;
    private int _locationIndex;
    private NavMeshAgent  _agent;

    private void Start()
    {
        anim = GetComponent<Animator>();
        ChangeState();
        if(patrolRoute)
            InitializePatrolRoute();
        _locationIndex = 1;
        if(GetComponent<NavMeshAgent>())
            _agent = GetComponent<NavMeshAgent>();

    }
    private void Update()
    {
        if (_agent && canChange)
        {
            if(_agent.hasPath && _agent.remainingDistance < 0.5f && !_agent.pathPending)
            {
                MoveToNextPatrolLocation();
            }
        }
    }
    void OnEnable()
    {
        EventManager<string>.StartListening(GameEvents.QuestStarted, onQuestStarted);
        EventManager<string>.StartListening(GameEvents.QuestCompleted, onQuestCompleted);
        EventManager<string>.StartListening(GameEvents.DialogueStarted, onDialogueStarted);
    }

    void OnDisable()
    {
        EventManager<string>.StopListening(GameEvents.QuestStarted, onQuestStarted);
        EventManager<string>.StopListening(GameEvents.QuestCompleted, onQuestCompleted);
        EventManager<string>.StopListening(GameEvents.DialogueStarted, onDialogueStarted);
    }

    private void ChangeState()
    {
        if(parameter.ToString() == "Bool")
            anim.SetBool(changeState, canChange);
        else if(parameter.ToString() == "Trigger")
            anim.SetTrigger(changeState);
    }

    private void InitializePatrolRoute()
    {
        foreach(Transform child in patrolRoute)
        {
            locations.Add(child);
        }
    }

    private void MoveToNextPatrolLocation()
    {
        if(locations.Count == 0) return;
        _agent.destination = locations[_locationIndex].position;

        _locationIndex = (_locationIndex + 1) % locations.Count;
    }

    private void onQuestStarted(string questId)
    {
        if(questId != m_questId) return;
        canChange = true;
        ChangeState();

        if (_agent)
        {
            _agent.isStopped = false; //立即恢复移动
            MoveToNextPatrolLocation();
        }      
    }
    private void onQuestCompleted(string questId)
    {
        if(questId != m_questId) return;
        canChange = false;
        ChangeState();
    }

    private void onDialogueStarted(string data)
    {
        // 必须要在玩家与指定 npc 发生对话后转换动作
        if(data != npcName) return;
        canChange = false;
        ChangeState();
        if (_agent)
        {
            _agent.isStopped = true;      // 停止移动
            _agent.velocity = Vector3.zero; // 清速度，防止惯性滑动
        }
       
    }
}
