using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// ScriptableObject for enemy base stats, which can be modified at object creation time.
/// </summary>
[CreateAssetMenu(fileName = "Enemy Configuration", menuName = "ScriptableObject/Enemy Configuration")]
public class EnemyScriptableObject : ScriptableObject
{
    // Components
    [SerializeField] private Enemy _prefab; // The enemy prefab
    [SerializeField] private AttackScriptableObject _attackConfig; // The attack configuration
    [SerializeField] private EnemyAbility _ability; // The enemy's ability

    // Stats
    [SerializeField] private int _maxHealth = 5; // The enemy's maximum health

    // Vision
    [SerializeField] private LayerMask _obstacleMask;

    // Movement Stats
    [SerializeField] private EnemyState _defaultState = EnemyState.Patrol; // The default state of the enemy
    [SerializeField] [Range(2, 4)] private int _waypoints = 4; // The number of waypoints the enemy will patrol between
    [SerializeField] private float _lineOfSightDistance = 10f; // The distance the enemy can see the player
    [SerializeField] [Range(0, 360)] private float _fieldOfView = 90f; // The angle the enemy can see the player

    // NavMeshAgent Configs
    public float _AIUpdateInterval = 0.1f;
    [SerializeField] private float _acceleration = 8f; // The acceleration of the enemy
    [SerializeField] private float _angularSpeed = 120f; // The angular speed of the enemy (how fast it turns)
    [SerializeField] private int _areaMask = -1; // -1 is all areas
    [SerializeField] private int _avoidancePriority = 50; // The priority of the enemy's avoidance
    [SerializeField] private float _baseOffset = 0f; // The base offset of the enemy (how high or low it is off the ground)
    [SerializeField] private float _height = 2f; // The height of the enemy agent
    [SerializeField] private ObstacleAvoidanceType _obstacleAvoidanceType = ObstacleAvoidanceType.LowQualityObstacleAvoidance; // The obstacle avoidance type of the enemy
    [SerializeField] private float _radius = 0.5f; // The radius of the enemy agent
    [SerializeField] private float _speed = 3.5f; // The speed of the enemy
    [SerializeField] private float _stoppingDistance = 1f; // The stopping distance of the enemy (how close it gets to other objects before stopping)

    // Getters and Setters
    public Enemy E_Prefab { get { return _prefab; } set { _prefab = value; } } // Referenced in EnemySpawner.cs

    /// <summary>
    /// This function sets up an enemy's attributes, movement, sight, abilities, and health based on the
    /// values provided.
    /// </summary>
    /// <param name="Enemy">An object representing an enemy in the game.</param>
    public void SetupEnemy(Enemy enemy)
    {
        // NavMeshAgent Configs
        enemy.Agent.acceleration = _acceleration;
        enemy.Agent.angularSpeed = _angularSpeed;
        enemy.Agent.areaMask = _areaMask;
        enemy.Agent.avoidancePriority = _avoidancePriority;
        enemy.Agent.baseOffset = _baseOffset;
        enemy.Agent.height = _height;
        enemy.Agent.obstacleAvoidanceType = _obstacleAvoidanceType;
        enemy.Agent.radius = _radius;
        enemy.Agent.speed = _speed;
        enemy.Agent.stoppingDistance = _stoppingDistance;

        // States and patrol waypoints
        enemy.Movement.E_UpdateRate = _AIUpdateInterval;
        enemy.Movement.E_DefaultState = _defaultState;
        enemy.Movement.E_Waypoints = new Vector3[_waypoints];
        
        // Sight Configs
        enemy.Movement.Sight.ObstacleMask = _obstacleMask;
        enemy.Movement.E_FieldOfView = _fieldOfView;

        // Abilities
        enemy.Ability = _ability;

        // Health
        enemy.E_Health = new UnitHealth(_maxHealth, _maxHealth);

        // Attack Configs
        _attackConfig.SetupEnemy(enemy, _ability); // Configure the enemy's attack
    }
}
