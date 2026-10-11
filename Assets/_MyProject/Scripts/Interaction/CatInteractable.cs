using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// 猫的交互脚本。玩家按 F 后通知任务系统并开始巡逻。
/// </summary>
public class CatInteractable : InteractableBase
{
    [Header("任务目标 ID")]
    [SerializeField] private string catId = "Cat1";

    [Header("巡逻路径")]
    [SerializeField] private Transform patrolRoute;
    [SerializeField] private float arriveThreshold = 0.5f;

    [Header("动画触发")]
    [SerializeField] private string walkTrigger = "toWalk";
    [SerializeField] private string danceTrigger = "toDance";

    private Animator anim;
    private NavMeshAgent agent;
    private List<Transform> waypoints = new List<Transform>();
    private int destinationIndex = 1;
    private bool isMoving = false;

    void Start()
    {
        anim = GetComponent<Animator>();
        agent = GetComponent<NavMeshAgent>();

        if (anim != null) anim.SetTrigger(danceTrigger);

        if (agent != null)
        {
            agent.isStopped = true;
            agent.velocity = Vector3.zero;
        }

        if (patrolRoute != null)
            InitializePatrolRoute();
    }

    private void InitializePatrolRoute()
    {
        waypoints.Clear();
        foreach (Transform child in patrolRoute)
            waypoints.Add(child);
    }

    protected override void OnInteract(GameObject interactor)
    {
        if (QuestManager.Instance != null && !string.IsNullOrEmpty(catId))
            QuestManager.Instance.NotifyObjectiveProgress("Interact", catId);

        if (waypoints == null || waypoints.Count == 0) return;

        if (!isMoving)
        {
            isMoving = true;
            if (anim != null) anim.SetTrigger(walkTrigger);
            if (agent != null) agent.isStopped = false;
        }

        MoveToNextWaypoint();
    }

    void Update()
    {
        if (!isMoving) return;
        if (agent == null || waypoints == null || waypoints.Count == 0) return;
        if (agent.pathPending) return;
        if (!agent.hasPath) return;

        if (agent.remainingDistance <= arriveThreshold)
        {
            isMoving = false;
            if (anim != null) anim.SetTrigger(danceTrigger);
            agent.isStopped = true;
            agent.velocity = Vector3.zero;
        }
    }

    private void MoveToNextWaypoint()
    {
        if (waypoints == null || waypoints.Count == 0) return;

        destinationIndex %= waypoints.Count;
        agent.destination = waypoints[destinationIndex].position;

        destinationIndex = (destinationIndex + 1) % waypoints.Count;
    }
}