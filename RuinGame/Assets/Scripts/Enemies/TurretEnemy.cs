using System.Collections;
using UnityEngine;

// This is the script for the TurretEnemy, which is a stationary enemy that shoots at the player and can be damaged by the player.
public class TurretEnemy : StationaryEnemy, IDamageable
{
    [SerializeField] private RangedAttackRadius _rangedAttackRadius; // This is the script that handles the shooting of the turret.

    // Attacks
    [SerializeField] private float _attackRange = 8f; // This is the range at which the turret will shoot at the player.
    [SerializeField] private float _sightRange = 10f; // This is the range at which the turret sees the player
    private bool _playerInSight = false; 
    private bool _playerInAttackRange = false;

    // Health
    private UnitHealth _health; // This is the UnitHealth component of the turret.
    [SerializeField] private int _healthPoints = 5; // This is the amount of health the turret has.
    [SerializeField] private Healthbar _healthbar; // This is the healthbar of the turret.
    [SerializeField] private Canvas _healthbarCanvas; // This is the canvas of the healthbar.
    private bool _isDead = false; // This is a bool that checks if the turret is dead.

    // Override Awake
    public override void OverrideAwake()
    {
        base.OverrideAwake(); // Call the base OverrideAwake() method.
        
        _health = new UnitHealth(_healthPoints, _healthPoints); // Set the health of the turret.
        _healthbarCanvas.enabled = false; // Disable the healthbar canvas.
        
        _rangedAttackRadius.CreateBulletPool(); // Create the bullet pool for the turret.
    }

    // Override Update
    public override void OverrideUpdate()
    {
        _playerInSight = Physics.CheckSphere(transform.position, _sightRange, LayerMask.GetMask("Player")); // Check if the player is in sight of the turret.
        _playerInAttackRange = Physics.CheckSphere(transform.position, _attackRange, LayerMask.GetMask("Player")); // Check if the player is in attack range of the turret.

        // If the player is in sight of the turret, but not in attack range, then the turret will be active.
        if (!_playerInSight && !_playerInAttackRange) { 
            _animator.SetBool("Active", false);
            _animator.SetBool("Attacking", false);
        }

        // If the player is in sight of the turret and not in attack range, then the turret will be active but not attacking.
        if (_playerInSight && !_playerInAttackRange) {
            _animator.SetBool("Active", true);
            _animator.SetBool("Attacking", false);
        }

        // If the player is in sight of the turret and in attack range, then the turret will be active and attacking.
        if (_playerInSight && _playerInAttackRange) {
            _animator.SetBool("Attacking", true);
        }
    }

    /// <summary>
    /// This function reduces the health of an object, updates its health bar, plays a feedback, and
    /// deactivates the object if its health reaches zero.
    /// </summary>
    /// <param name="damage">The amount of damage that the object is taking.</param>
    public void TakeDamage(int damage)
    {
        _health.TakeDamage(damage); // Reduce the health of the turret.
        _healthbar.SetHealth(_health.Health); // Update the healthbar of the turret.
        _FB_hit.PlayFeedbacks(); // Play the feedback for the turret taking damage.

        if (_health.Health <= 0)
        {
            _isDead = true;
            gameObject.SetActive(false);
        }
        if (gameObject.activeSelf) StartCoroutine(HealthTimer());
    }

    /// <summary>
    /// This function enables the health bar canvas for 2 seconds and then disables it.
    /// </summary>
    private IEnumerator HealthTimer()
    {
        _healthbarCanvas.enabled = true;
        yield return new WaitForSeconds(2f);
        _healthbarCanvas.enabled = false;
    }

    /// <summary>
    /// The function returns the transform component of a game object.
    /// </summary>
    /// <returns>
    /// A reference to the Transform component of the game object this script is attached to.
    /// </returns>
    public Transform GetTransform()
    {
        return transform;
    }

    /// <summary>
    /// This function makes the character perform a trap attack by rotating towards the player's
    /// position and triggering a ranged attack.
    /// </summary>
    public override void TrapAttack()
    {
        Vector3 direction = _player.transform.position - transform.position; // Get the direction of the player.
        direction.y = 0f; // Set the y value of the direction to 0
        Quaternion lookRotation = Quaternion.LookRotation(direction); // Get the rotation of the direction.
        transform.rotation = Quaternion.Euler(0f, lookRotation.eulerAngles.y, 0f); // Rotate the turret towards the player.
        _rangedAttackRadius.AttackRadiusShoot(); // Shoot at the player.
        _FB_swing?.PlayFeedbacks(); // Play the feedback for the turret attacking.
    }
}
