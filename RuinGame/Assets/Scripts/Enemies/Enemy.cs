using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Events;
using MoreMountains.Feedbacks;

// Enemy class
public class Enemy : PoolableObject, IDamageable
{
    // Components
    [SerializeField] private Damager _damager; // Damager component. Used to deal damage to IDamageable objects
    [SerializeField] private AttackRadius _attackRadius; // Attack radius component. Used to detect player and handle attacks
    private NavMeshAgent _agent;
    [SerializeField] private EnemyMovement _movement; // Enemy movement component. Used to handle enemy movement
    private BoxCollider _collider; // Enemy hitbox collider

    private PlayerController _player; // Player controller reference
    private Animator _animator; // Enemy animator reference
    
    // Skills
    private EnemyAbility _ability; // Enemy ability
    private float _cooldownTime; // Cooldown time
    private float _activeTime; // Duration of the ability
    private bool _canUseAbility = false; // Used to check if the ability can be used
    private EnemyAbility.AbilityState _state; // Used to check the current state of the ability
    private Coroutine _abilityCoroutine = null; // Ability coroutine reference
    

    // Feedbacks
    [SerializeField] private MMF_Player _FB_takeDamage; // Feedback for taking damage
    [SerializeField] private MMF_Player _FB_ability; // Feedback for ability
    [SerializeField] private MMF_Player _FB_death; // Feedback for death
    [SerializeField] private MMF_Player _FB_attackImpact; // Feedback for attack impact

    // Health
    private UnitHealth _health; // Enemy health
    [SerializeField] private Healthbar _healthbar; // Enemy healthbar
    [SerializeField] private Canvas _healthbarCanvas; // Enemy healthbar canvas (used to enable/disable healthbar)
    private bool _isDead = false; // Used to check if the enemy is dead

    // Events
    public UnityEvent OnDeathWave; // Event called when the enemy dies, called by the wave manager BurstSpawnArea.cs

    // Getters and setters
    public NavMeshAgent Agent { get => _agent; set => _agent = value; }
    public Animator Animator { get => _animator; set => _animator = value; }
    public EnemyMovement Movement { get => _movement; set => _movement = value; }
    public AttackRadius AttackRadius { get => _attackRadius; set => _attackRadius = value; }
    public UnitHealth E_Health { get => _health; set => _health = value; }
    public PlayerController PlayerController { get => _player; set => _player = value; }
    public EnemyAbility Ability { get => _ability; set => _ability = value; }
    public bool IsEnemyDead { get => _isDead; set => _isDead = value; }
    public Damager EnemyDamager { get => _damager; set => _damager = value; }
    public Canvas EnemyHealthbar { get => _healthbarCanvas; set => _healthbarCanvas = value; }
    public Coroutine AbilityCoroutine { get => _abilityCoroutine; set => _abilityCoroutine = value; }

    // Awake is called before the first frame update
    private void Awake()
    {
        _agent = GetComponent<NavMeshAgent>();
        _movement = GetComponent<EnemyMovement>();
        _animator = GetComponent<Animator>();
        _collider = GetComponent<BoxCollider>();

        _attackRadius.OnAttack += OnAttack;
        _healthbarCanvas.enabled = false;
    }

    // Start is called on the first frame update
    private void Start()
    {
        _ability.SetupRangedAbility(this); // Setup the ability
        _state = EnemyAbility.AbilityState.ready; // Set the ability state to ready
    }

    // Update is called once per frame
    void Update()
    {
        if (_isDead) return; // If the enemy is dead, return
        switch (_state)
        {
            case EnemyAbility.AbilityState.ready: // If the ability is ready, check if the ability can be used
                if (_canUseAbility) {
                    if (_ability.EAIsAbilityRanged) {
                        _ability.Activate(_FB_ability, this, _player, this.GetComponentInChildren<RangedAttackRadius>()); // Activate the ability (ranged)
                    } else {
                        _ability.Activate(_FB_ability, this, _player); // Activate the ability
                    }
                    _state = EnemyAbility.AbilityState.active; // Set the ability state to active
                    _activeTime = _ability.EAActiveTime; // Set the active time
                }
            break;
            case EnemyAbility.AbilityState.active: // If the ability is active, count down the active time
                if (_activeTime > 0) {
                    _activeTime -= Time.deltaTime;
                } else { // Ability ended, start cooldown
                    _state = EnemyAbility.AbilityState.cooldown;
                    _cooldownTime = _ability.EACooldownTime;
                }
            break;
            case EnemyAbility.AbilityState.cooldown:
                // Count down cooldown time
                if (_cooldownTime > 0) {
                    _cooldownTime -= Time.deltaTime;
                } else { // Cooldown ended, ability is ready
                    _state = EnemyAbility.AbilityState.ready;
                }
            break;
        }
    }

    /// <summary>
    /// The function sets a boolean variable to indicate whether the ability can be used during an
    /// attack.
    /// </summary>
    /// <param name="IDamageable">IDamageable is an interface that represents an object
    /// that can take damage.</param>
    /// <param name="isAttacking">A boolean value indicating whether the character is currently
    /// attacking or not.</param>
    private void OnAttack(IDamageable target, bool isAttacking) // OnAttack event function (called by AttackRadius.cs)
    {
        _canUseAbility = isAttacking; // Set the ability boolean to the isAttacking boolean
    }

    /// <summary>
    /// This function plays a death feedback, disables various components, waits for a set amount of
    /// time, and then invokes an event and deactivates the game object.
    /// </summary>
    private IEnumerator DeathTimer()
    {
        _FB_death?.PlayFeedbacks(); // Play death feedback if not null
        _animator.SetTrigger("Die"); // Play death animation
        _isDead = true;
        _agent.enabled = false;

        if (_damager != null) _damager.DamagerCollider.enabled = false;
        _healthbarCanvas.enabled = false;
        _collider.enabled = false;
        _movement.enabled = false;

        yield return new WaitForSeconds(1f);

        OnDeathWave?.Invoke(); // Invoke the OnDeathWave event (caught by BurstSpawnArea.cs)
        gameObject.SetActive(false); // Deactivate the game object
    }

    // This fucntion is built into Unity and is called when the game object is disabled
    public override void OnDisable()
    {
        base.OnDisable(); // Call the base class OnDisable function
        _agent.enabled = false;

        // Reenable colliders disabled during death
        if (_damager != null) _damager.DamagerCollider.enabled = true;
        _collider.enabled = true;
        _movement.enabled = true;

        // Remove listeners for wave events
        OnDeathWave.RemoveAllListeners();

        if (_ability != null) _ability.ResetAbility(this);
        _isDead = false;
    }

    /// <summary>
    /// This function reduces the health of the enemy, updates its health bar, plays feedback, and
    /// triggers death or health timers if necessary.
    /// </summary>
    /// <param name="damage">The amount of damage that the object is taking.</param>
    public void TakeDamage(int damage)
    {
        _health.TakeDamage(damage);
        _healthbar.SetHealth(_health.Health);
        _FB_takeDamage?.PlayFeedbacks();

        if (_health.Health <= 0)
        {
            _isDead = true;
            StartCoroutine(DeathTimer());
        }
        else { Debug.Log(_health.Health); }
        if (gameObject.activeSelf) StartCoroutine(HealthTimer());
    }

    public Transform GetTransform() { return transform; } // Get the transform of the enemy
    
    /// <summary>
    /// This function enables the health bar canvas for 2 seconds and then disables it.
    /// </summary>
    private IEnumerator HealthTimer()
    {
        _healthbarCanvas.enabled = true;
        yield return new WaitForSeconds(2f);
        _healthbarCanvas.enabled = false;
    }

    // This function is called in an animation event
    public void PlayAttackImpactFX()
    {
        _FB_attackImpact?.PlayFeedbacks();
    }

    /// <summary>
    /// This function enables the enemy's attack radius collider and starts a coroutine to disable it
    /// later. Used in an animation event to trigger attack collider.
    /// </summary>
    public void EnableEnemyAttackRadius()
    {
        _damager.DamagerCollider.enabled = true;
        StartCoroutine(DisableEnemyAttackRadius());
    }

    private IEnumerator DisableEnemyAttackRadius()
    {
        yield return new WaitForSeconds(0.2f);
        _damager.DamagerCollider.enabled = false;
    }
}
