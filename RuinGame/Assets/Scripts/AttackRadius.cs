using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Attack radius component class. This is attached to the enemy gameobject and is used to detect if the player is in range of the enemy's attack.
// If the player is in range, the enemy will attack the player.
[RequireComponent(typeof(SphereCollider))]
public class AttackRadius : MonoBehaviour
{
    // Components
    protected Enemy _enemy; // Enemy reference, can be accessed by child classes

    // Stats
    [SerializeField] private int _damage = 0; // Serialize for stationary enemies, which dont use scriptable object setup
    private float _attackRate = 1f; // How often the enemy can attack, used for ranged enemies
    private float _attackRange; // Radius of the sphere used in the CheckSphere method
    private bool _isInAttackRange; // Is the player in range of the enemy's attack

    // Event called here and caught by Enemy.cs
    public delegate void AttackEvent(IDamageable target, bool _isAttacking);
    public AttackEvent OnAttack;

    // Sight
    private EnemyLineOfSightChecker _lineOfSightChecker;

    private bool _isAttacking = false;

    // Getters and Setters
    public int Radius_Damage { get => _damage; set => _damage = value; }
    public float Radius_AttackRate { get => _attackRate; set => _attackRate = value; }
    public float Radius_AttackRange { get => _attackRange; set => _attackRange = value; }

    // Awake is called before the first frame update
    protected virtual void Awake()
    {
        _enemy = GetComponentInParent<Enemy>(); // Get the enemy component from the parent gameobject
    }

    // Start is called on the first frame
    protected virtual void Start()
    {
        if (_enemy != null) _lineOfSightChecker = _enemy.Movement.Sight; // Get the enemy's line of sight checker
    }

    // Update is called once per frame
    protected virtual void Update() 
    {
        // Check if the player is in range of the enemy's attack
        _isInAttackRange = Physics.CheckSphere(transform.position, _attackRange, LayerMask.GetMask("Player")) && _lineOfSightChecker.PlayerSeen;

        // If the player is in range, invoke the OnAttack event, which is caught by the Enemy.cs script.
        // If the player is not in range, invoke the OnAttack event with a negative value, which is caught by the Enemy.cs script.
        if (_isInAttackRange) { _isAttacking = true; OnAttack?.Invoke(_enemy.PlayerController, _isAttacking); }
        else { _isAttacking = false; OnAttack?.Invoke(_enemy.PlayerController, _isAttacking); }
    }
}
