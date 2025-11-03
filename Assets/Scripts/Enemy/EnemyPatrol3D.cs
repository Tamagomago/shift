using UnityEngine;
using UnityEngine.AI;
using System.Linq;

public class EnemyPatrol3D : MonoBehaviour
{
    private bool _isPaused = false;
    private bool _isChasing = false;

    [Header("Patrol Config")]
    [SerializeField] private float speed = 3f;
    [SerializeField] private Transform[] points;
    [Header("Chase Config")]
    [SerializeField] private Transform player;
    [SerializeField] private float chaseRange = 5f;
    [SerializeField] private float loseRange = 25f;

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
        if (_isPaused)
        {
            agent.isStopped = true;
            return;
        }

        DetectPlayer();
        // Debug.Log($"{gameObject.name} | Agent Pos: {agent.transform.position:F2} | Target: {agent.destination:F2} | Remaining: {agent.remainingDistance:F2}");
        if(_isChasing)
        {
            agent.isStopped = false;
            agent.SetDestination(player.position);
            return;
        }
        // agent.isStopped = false;

        if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance + 0.1f)
        {
            GoToNextPoint();
        }
    }

    private void DetectPlayer()
    {
        if (player == null) return;

        float distance = Vector3.Distance(transform.position, player.position);

        if (!_isChasing && distance <= chaseRange) StartChase();
        else if (_isChasing && distance > loseRange) StopChase();
    }

    private void StartChase()
    {
        _isChasing = true;
        agent.speed = speed + 1f;
        Debug.Log($"{gameObject.name} started chasing {player.name}");
    }

    private void StopChase()
    {
        _isChasing = false;
        agent.speed = speed;
        GoToNextPoint();
        Debug.Log($"{gameObject.name} stopped chasing and resumed patrol");
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
