using UnityEngine;
using UnityEngine.AI;
using System.Linq;

public class EnemyPatrol3D : MonoBehaviour
{
    private bool _isPaused = false;
    private bool _isChasing = false;

    [Header("Patrol Config")]
    [SerializeField] private float speed = 1f;
    [SerializeField] private Transform[] points;
    [Header("Chase Config")]
    [SerializeField] private float chaseRange = 5f;
    [SerializeField] private float loseRange = 25f;

    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip attackSound;
    
    private Transform _player;
    private DimensionManager _dimensionManager;
    private PlayerController _playerController;

    private Vector3[] _initialPointPos;
    private int patrolPointIdx = 0;
    private NavMeshAgent _agent;

    void Awake()
    {
        _agent = GetComponent<NavMeshAgent>();
        _dimensionManager = FindFirstObjectByType<DimensionManager>();
        if (_dimensionManager == null)
        {
            Debug.Log("No `DimensionManager` script found in scene!");
        }
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // Detect player at runtime
        GameObject taggedPlayer = GameObject.FindGameObjectWithTag("Player");
        if (taggedPlayer == null) Debug.Log("No player found!");
        _player = taggedPlayer.transform;

        // Get reference to player controller for respawn on interaction
        _playerController = taggedPlayer.GetComponent<PlayerController>();
        if (_playerController == null) Debug.Log("No PlayerController found from player!");
        
        _initialPointPos = points.Select(p => p.position).ToArray();
        _agent.speed = speed;

        if (points.Length < 2)
        {
            Debug.Log("No assigned patrol points");
            return;
        }

        _agent.SetDestination(_initialPointPos[patrolPointIdx]);
    }

    // Update is called once per frame
    void Update()
    {
        if (_isPaused)
        {
            _agent.isStopped = true;
            return;
        }

        DetectPlayer();
        // Debug.Log($"{gameObject.name} | Agent Pos: {agent.transform.position:F2} | Target: {agent.destination:F2} | Remaining: {agent.remainingDistance:F2}");
        if(_isChasing)
        {
            _agent.isStopped = false;
            _agent.SetDestination(_player.position);
            return;
        }
        // agent.isStopped = false;

        if (!_agent.pathPending && _agent.remainingDistance <= _agent.stoppingDistance + 0.1f)
        {
            GoToNextPoint();
        }
    }

    
    private void DetectPlayer()
    {
        if (_player == null) return;

        float distance = Vector3.Distance(transform.position, _player.position);

        if (!_isChasing && distance <= chaseRange) StartChase();
        else if (_isChasing && distance > loseRange) StopChase();
    }

    private void StartChase()
    {
        _isChasing = true;
        _agent.speed = speed + 1f;
        Debug.Log($"{gameObject.name} started chasing {_player.name}");
    }

    private void StopChase()
    {
        _isChasing = false;
        _agent.speed = speed;
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
        _agent.SetDestination(_initialPointPos[patrolPointIdx]);
    }

    // Check using collision detection if there are players attacked by the enemy
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && _playerController != null)
        {
            if (audioSource != null && attackSound != null)
            {
                AudioSource.PlayClipAtPoint(attackSound, transform.position);
            }
            if (_dimensionManager != null && _dimensionManager.ToggleDimensionRef != null)
            {
                _dimensionManager.ToggleDimensionRef.SwitchRealm();
            }
        }
    }
}
