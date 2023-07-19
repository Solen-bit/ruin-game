using UnityEngine;
using MoreMountains.Feedbacks;

// Path: Assets\Scripts\Bullet.cs
// Class for the in-game projectiles. Handles the damage, hit feedback, etc. It also has a poolable object component.
[RequireComponent(typeof(Rigidbody))]
public class Bullet : PoolableObject
{
    [SerializeField] float _autoDestroyTime = 5f; // Time before the bullet is destroyed
    [SerializeField] float _speed = 2f; // Speed of the bullet
    [SerializeField] int _damage = 1; // Damage of the bullet
    [SerializeField] Rigidbody _rigidbody; // Rigidbody of the bullet
    [SerializeField] private BulletHitFX _bulletHitFX; // Bullet hit feedback
    protected Transform _target; // Target of the bullet

    // Getters and Setters
    public int BulletDamage { get => _damage; set => _damage = value; }
    public float BulletSpeed { get => _speed; set => _speed = value; }
    public float BulletAutoDestroyTime { get => _autoDestroyTime; set => _autoDestroyTime = value; }


    protected const string DISABLE_METHOD_NAME = "Disable"; // Method name for disabling the bullet

    // Awake is called before the first frame update
    private void Awake()
    {
        _rigidbody = GetComponent<Rigidbody>(); // Get the rigidbody of the bullet
        _bulletHitFX = FindObjectOfType<BulletHitFX>(); // Get the bullet hit feedback
    }

    // OnEnable is called when the bullet is enabled
    protected virtual void OnEnable()
    {
        CancelInvoke(DISABLE_METHOD_NAME); // Cancel the disable method
        Invoke(DISABLE_METHOD_NAME, _autoDestroyTime); // Invoke the disable method
    }

    /// <summary>
    /// The function spawns a bullet and adds force to it in a given direction with a given speed,
    /// while also setting the damage and target properties.
    /// </summary>
    /// <param name="Vector3">A 3-dimensional vector representing the direction in which the object
    /// should be spawned.</param>
    /// <param name="damage">The amount of damage that the spawned object will deal to its
    /// target.</param>
    /// <param name="Transform">Transform is a data type in Unity that represents the position,
    /// rotation, and scale of a game object. In this context, the Transform parameter is used to
    /// specify the target object that the spawned object will be aimed at or directed towards.</param>
    public virtual void Spawn(Vector3 forward, int damage, Transform target)
    {
        this._damage = damage;
        this._target = target;
        _rigidbody.AddForce(forward * _speed, ForceMode.VelocityChange);
    }

    /// <summary>
    /// The Deflect function sets the velocity of a rigidbody to zero, rotates the transform to face a
    /// given direction, and adds a force in that direction to the rigidbody.
    /// </summary>
    /// <param name="Vector3">A Vector3 is a data type in Unity that represents a 3-dimensional vector,
    /// which consists of three float values (x, y, z) that can be used to represent positions,
    /// directions, velocities, and other physical quantities in 3D space. In this code snippet, the
    /// "forward</param>
    public void Deflect(Vector3 forward) // Used for deflecting bullets in player's AttackCollision.cs class
    {
        _rigidbody.velocity = Vector3.zero;
        transform.rotation = Quaternion.LookRotation(forward);
        _rigidbody.AddForce(forward * _speed, ForceMode.VelocityChange);
    }

    /// <summary>
    /// This function triggers an action when a collider enters a trigger, checks if it's a player
    /// attack radius, damages the object if it's damageable, and disables the collider.
    /// </summary>
    /// <param name="Collider">A Collider is a component that defines the shape of an object for the
    /// purposes of physical collisions. It can be attached to any GameObject in a scene and is used to
    /// detect collisions with other objects in the scene. When two Colliders collide, they generate
    /// collision events that can be detected and responded to by, in this case, OnTriggerEnter</param>
    protected virtual void OnTriggerEnter(Collider other) 
    {
        // Debug.Log(other.gameObject.layer); For debugging purposes
        // Debug.Log(other.gameObject.name); For debugging purposes
        _bulletHitFX?.PlayBulletHitFeedback(this.transform); // Play the bullet hit feedback at the bullet's position
        if (other.gameObject.layer == LayerMask.NameToLayer("PlayerAttackRadius")) { return; } // If the bullet hits the player's attack radius, do nothing and skip the rest of the code

        IDamageable damageable;

        /* Calls the TakeDamage function on damageable component, passing in the damage value of the bullet. 
        After that, it calls the Disable function to disable the bullet. This is used to handle the damage dealt by the bullet to the
        object it has collided with and to disable the bullet after it has hit something. */
        if (other.TryGetComponent<IDamageable>(out damageable))
        {
            damageable.TakeDamage(_damage);
        }
        
        Disable();
    }

    /// <summary>
    /// This function disables the game object and cancels any scheduled invokes, sets the velocity to
    /// zero, and changes the layer if it's a player bullet.
    /// </summary>
    protected void Disable()
    {
        CancelInvoke(DISABLE_METHOD_NAME);
        _rigidbody.velocity = Vector3.zero;
        if (gameObject.layer == LayerMask.NameToLayer("PlayerBullet"))
            gameObject.layer = LayerMask.NameToLayer("EnemyBullets");
        gameObject.SetActive(false);
    }
}
