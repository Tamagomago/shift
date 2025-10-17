using UnityEngine;
using UnityEngine.AI;
using System.Linq;

public class EnemyPatrol3D : MonoBehaviour
{
    private bool isPaused = false;
    private bool isChasing = false;

    [Header("Patrol Config")]
    [SerializeField] private float speed = 3f;
    [SerializeField] private Transform[] points;

    private Vector3[] _initialPointPos;
    private int patrolPointIdx = 0;
    private NavMeshAgent agent;

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _initialPointPos = points.Select(p => p.position).ToArray();
        agent.speed = speed;

        if (points.Length < 2)
        {
            Debug.Log("No assigned patrol points");
            return;
        }

        agent.SetDestination(_initialPointPos[patrolPointIdx]);
    }

    // Update is called once per frame
    void Update()
    {
        if (isPaused || isChasing)
        {
            agent.isStopped = true;
            return;
        }

        Debug.Log($"{gameObject.name} | Agent Pos: {agent.transform.position:F2} | Target: {agent.destination:F2} | Remaining: {agent.remainingDistance:F2}");

        agent.isStopped = false;

        if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance + 0.1f)
        {
            GoToNextPoint();
        }
    }
    
    private void GoToNextPoint()
    {
        if (points.Length < 2)
        {
            Debug.Log("Patrol Points are incorrectly set.");
            return;
        }

        patrolPointIdx = (patrolPointIdx + 1) % _initialPointPos.Length;
        agent.SetDestination(_initialPointPos[patrolPointIdx]);
    }
}
