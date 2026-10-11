using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class ObjectStateChange : MonoBehaviour
{
    [Header("物品名称")]
    [SerializeField] private string objectName = "Cat";

    [Header("原来的状态名")]
    [SerializeField] private string stateNameOrigin = "toDance";

    [Header("需要切换的状态名")]
    [SerializeField] private string stateNameChange = "toWalk";

    [Header("移动路径")]
    [SerializeField] private Transform patrolRoute;
    [SerializeField] private List<Transform> locations;

    private Animator anim;
    private NavMeshAgent _agent;
    private int _locationIndex = 1;
    private bool canChange = false;

    private void Start()
    {
        anim = GetComponent<Animator>();
        _agent = GetComponent<NavMeshAgent>();

        if (anim != null)
            anim.SetTrigger(stateNameOrigin);

        if (patrolRoute != null)
            InitializePatrolRoute();

        if (_agent != null)
        {
            _agent.isStopped = true;
            _agent.velocity = Vector3.zero;
        }
    }

    void OnEnable()
    {
        EventManager<string>.StartListening(GameEvents.ItemAdded, onObjectInteract);
    }

    void OnDisable()
    {
        EventManager<string>.StopListening(GameEvents.ItemAdded, onObjectInteract);
    }

    private void Update()
    {
        if (_agent == null || !canChange) return;

        if (_agent.hasPath && !_agent.pathPending &&
            _agent.remainingDistance <= 0.5f)
        {
            // 到达目标点：停下，切回跳舞
            canChange = false;
            if (anim != null) anim.SetTrigger(stateNameOrigin);

            _agent.isStopped = true;
            _agent.velocity = Vector3.zero;

            Debug.Log($"[{objectName}] 到达路径点，停止");
        }
    }

    private void InitializePatrolRoute()
    {
        locations.Clear();
        foreach (Transform child in patrolRoute)
            locations.Add(child);
    }

    private void onObjectInteract(string itemName)
    {
        if (itemName != objectName) return;
        //if (canChange) return;                       // 已经在移动中，忽略
        if (locations == null || locations.Count == 0) return;

        // 切换到走路动画
        canChange = true;
        if (anim != null) anim.SetTrigger(stateNameChange);

        // 开始移动
        if (_agent != null)
        {
            _agent.isStopped = false;
            MoveToNextPatrolLocation();
        }
    }

    private void MoveToNextPatrolLocation()
    {
        if (locations == null || locations.Count == 0) return;

        _locationIndex %= locations.Count;
        _agent.destination = locations[_locationIndex].position;
        Debug.Log($"[{objectName}] 前往 {locations[_locationIndex].name}");

        _locationIndex = (_locationIndex + 1) % locations.Count;
    }
}