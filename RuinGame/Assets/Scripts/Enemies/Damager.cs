using UnityEngine;

// Damager class, which handles damage actions for enemies to other objects
public class Damager : MonoBehaviour
{
    private int _damage; // Damage amount
    private SphereCollider _colliderDamager; // Damager collider
    public SphereCollider DamagerCollider { get => _colliderDamager; set => _colliderDamager = value; } // Damager collider getter and setter

    // Awake is called before the first frame update
    private void Awake()
    {
        _colliderDamager = GetComponent<SphereCollider>(); // Get the damager collider
    }

    // Start is called on the first frame update
    private void Start()
    {
        _damage = GetComponentInParent<Enemy>().AttackRadius.Radius_Damage; // Get the damage amount from the enemy
    }

    /// <summary>
    /// This function checks if a collider has a component that implements the IDamageable interface and
    /// if so, calls the TakeDamage function on it with a specified damage value.
    /// </summary>
    /// <param name="Collider">Collider is a component in Unity that defines the shape of an object's
    /// collision boundary. It is used to detect collisions with other objects in the game world. When
    /// two colliders intersect, a collision event is triggered. In this code snippet, the
    /// OnTriggerEnter method is called when the collider attached to the game object collides with another</param>
    protected virtual void OnTriggerEnter(Collider other)
    {
        IDamageable damageable = other.GetComponent<IDamageable>();
        if (damageable != null)
        {
            damageable.TakeDamage(_damage);
        }
    }
}
