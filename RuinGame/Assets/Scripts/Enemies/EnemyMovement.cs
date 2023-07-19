using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

// Enemy movement class. Responsible for handling movement and animation logic.
[RequireComponent(typeof(NavMeshAgent))]
public class EnemyMovement : MonoBehaviour
{
    // Components
    private Transform _player; // Player transform reference
    private NavMeshTriangulation _triangulation; // Navmesh triangulation reference
    private float _updateRate = 0.1f; // Update rate
    private NavMeshAgent _agent; // Navmesh agent reference
    private Animator _animator = null; // Animator reference
    private Enemy _enemy; // Enemy reference

    //Sight
    [SerializeField] private EnemyLineOfSightChecker _lineOfSightChecker; // Line of sight checker reference

    // Idle
    private bool _isIdle = false; // Used to check if the enemy is idle

    // Patroling
    private Coroutine _followCoroutine; // Follow coroutine reference
    private int _waypointIndex = 0; // Current waypoint index
    private Vector3[] _waypoints = new Vector3[4]; // Waypoints array

    // States
    public EnemyState defaultState; // Default state
    private EnemyState _state; // Current state
    public EnemyState State { get { return _state; } set { OnStateChange?.Invoke(_state, value);  _state = value; }} // Current state getter and setter. Invokes OnStateChange event when state changes

    // State events
    public delegate void StateChangeEvent(EnemyState oldState, EnemyState newState); // State change event delegate
    public StateChangeEvent OnStateChange;

    // Getters and setters used in scriptble objects to setup enemy
    public Transform E_Player { get => _player; set => _player = value; }
    public float E_UpdateRate { get => _updateRate; set => _updateRate = value; }
    public NavMeshTriangulation E_Triangulation { get => _triangulation; set => _triangulation = value; }
    public EnemyState E_DefaultState { get => defaultState; set => defaultState = value; }
    public Vector3[] E_Waypoints { get => _waypoints; set => _waypoints = value; }
    public float E_FieldOfView { get => _lineOfSightChecker.E_FieldOfView; set => _lineOfSightChecker.E_FieldOfView = value; }
    public EnemyLineOfSightChecker Sight { get => _lineOfSightChecker; set => _lineOfSightChecker = value; }

    private void Awake()
    {
        _agent = GetComponent<NavMeshAgent>();
        _enemy = GetComponent<Enemy>();
        _animator = GetComponent<Animator>();

        // Subscribing to line of sight checker events
        _lineOfSightChecker.OnGainSight += HandleGainSight;
        _lineOfSightChecker.OnLoseSight += HandleLoseSight; 

        // Subscribing to state change event
        OnStateChange += HandleStateChange;
    }

    /// <summary>
    /// This function handles the enemy gaining sight of the player and changes the enemy state to chase
    /// if not already using an ability.
    /// </summary>
    /// <param name="PlayerController">A class or data type that represents the player character in the
    /// game. It contains information such as the player's position, health, and movement
    /// capabilities.</param>
    /// <returns>
    /// If the enemy is dead, the method returns and does not execute the rest of the code.
    /// </returns>
    private void HandleGainSight(PlayerController player)
    {
        if (_enemy.IsEnemyDead) return;
        Debug.Log("Gain sight");
        if (State != EnemyState.UsingAbility)
            State = EnemyState.Chase;
    }

    /// <summary>
    /// This function handles the enemy losing sight of the player and changes the enemy state to default
    /// if not already using an ability.
    /// </summary>
    /// <param name="PlayerController">A class or data type that represents the player character in the
    /// game. It contains information such as the player's position, health, and movement
    /// capabilities.</param>
    /// <returns>
    /// If the enemy is dead, the method returns and does not execute the rest of the code.
    /// </returns>
    private void HandleLoseSight(PlayerController player)
    {
        if (_enemy.IsEnemyDead) return;
        Debug.Log("Lose sight");
        if (State != EnemyState.UsingAbility)
            State = defaultState;
    }

    // Built in Unity function called when script is disabled
    private void OnDisable()
    {
        _state = defaultState; // Resetting state to default
    }

    // Built in Unity function called when script is enabled
    private void OnEnable()
    {
        if (_state == EnemyState.UsingAbility) { _state = defaultState; } // Resetting state to default if using ability
    }

    /// <summary>
    /// The Spawn function generates random waypoints for an enemy to navigate to using NavMesh.
    /// </summary>
    public void Spawn()
    {
        for (int i = 0; i < _waypoints.Length; i++)
        {
            NavMeshHit hit;
            Vector3 randomDir = Random.insideUnitSphere * 6f; // Random direction
            randomDir += transform.position; // Adding random direction to enemy position
            if (NavMesh.SamplePosition(randomDir, out hit, 6f, _agent.areaMask)) // If a valid position is found
            {
                _waypoints[i] = hit.position; // Set waypoint to valid position
            }
            else { Debug.LogError("No valid position found"); }
        }
        OnStateChange?.Invoke(EnemyState.Default, defaultState); // Invoking OnStateChange event
    }
    
    // Patroling state
    /// <summary>
    /// This function controls the patrolling behavior of an enemy character in a game.
    /// </summary>
    private IEnumerator Patroling()
    {
        WaitForSeconds wait = new WaitForSeconds(_updateRate);
        yield return new WaitUntil(() => _agent.isOnNavMesh && _agent.enabled); // Wait until agent is on navmesh and enabled

        _agent.speed = 2f;
        _animator.SetFloat("EnemyBlend", 1f);

        _agent.SetDestination(_waypoints[_waypointIndex]); // Set destination to first waypoint

        while(true)
        {
            /* This code block is checking if the enemy agent is currently on the NavMesh, enabled, and
            if the remaining distance to the current waypoint is less than or equal to the stopping
            distance. If all of these conditions are met, the waypoint index is incremented and if
            it exceeds the length of the waypoints array, it is reset to 0. The agent is then
            stopped and the enemy is set to idle for 4 seconds before resuming movement towards the
            next waypoint. */
            if (_agent.isOnNavMesh && _agent.enabled && _agent.remainingDistance <= _agent.stoppingDistance)
            {
                _waypointIndex++;

                if (_waypointIndex >= _waypoints.Length)
                {
                    _waypointIndex = 0;
                }

                // Idle
                _agent.isStopped = true;
                _animator.SetFloat("EnemyBlend", 0f);

                yield return new WaitForSeconds(4f);

                _agent.isStopped = false;
                _isIdle = false;
                _animator.SetFloat("EnemyBlend", 1f);
                _agent.SetDestination(_waypoints[_waypointIndex]);
            }
            yield return wait;
        }
    }

    /// <summary>
    /// This function makes an enemy chase the player by setting its destination to the player's
    /// position.
    /// </summary>
    private IEnumerator ChasePlayer()
    {
        WaitForSeconds wait = new WaitForSeconds(_updateRate);

        while (true)
        {
            if (_agent.enabled)
            {
                _agent.isStopped = false;
                _agent.speed = 4f;
                _animator.SetFloat("EnemyBlend", 1f); // Setting chase animation
                _agent.SetDestination(_player.position);
            }
            yield return wait;
        }
        
    }

    /// <summary>
    /// This function handles state changes for an enemy, stopping any current coroutines and starting
    /// new ones based on the new state.
    /// </summary>
    /// <param name="EnemyState">An enum that represents the current state of the enemy. It can be
    /// either Patrol or Chase.</param>
    /// <returns>
    /// If the `_enemy` is dead, the method returns without doing anything.
    /// </returns>
    private void HandleStateChange(EnemyState oldState, EnemyState newState)
    {
        if (_enemy.IsEnemyDead) return;
        if (oldState != newState)
        {
            if (_followCoroutine != null)
            {
                StopCoroutine(_followCoroutine);
            }

            switch (newState)
            {
                case EnemyState.Patrol:
                    _followCoroutine = StartCoroutine(Patroling());
                    break;
                case EnemyState.Chase:
                    _followCoroutine = StartCoroutine(ChasePlayer());
                    break;
            }
        }
    }

    /// <summary>
    /// This function draws wire spheres and lines between waypoints in a Gizmos view.
    /// </summary>
    private void OnDrawGizmosSelected()
    {
        for (int i = 0; i < _waypoints.Length; i++)
        {
            Gizmos.DrawWireSphere(_waypoints[i], 0.25f);
            if (i + 1 < _waypoints.Length) { Gizmos.DrawLine(_waypoints[i], _waypoints[i + 1]); }
            else { Gizmos.DrawLine(_waypoints[i], _waypoints[0]); }
        }
    }
}
