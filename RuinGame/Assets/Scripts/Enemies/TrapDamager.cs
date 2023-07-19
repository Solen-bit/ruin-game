using System.Collections;
using UnityEngine;

// Class for the trap damager, which is a script that damages the player when they enter the trigger collider. Used for the traps in the game.
public class TrapDamager : MonoBehaviour
{
    private int _damage; // Damage that this enemy deals

    [SerializeField] private bool _isContinuousDamage = false; // Is the damage continuous?

    private Coroutine _damageCoroutine = null; // Coroutine for dealing continuous damage
    private StationaryEnemy _stationaryEnemy; // Reference to the StationaryEnemy script

    // Start is called on the first frame
    private void Start()
    {
        _stationaryEnemy = GetComponentInParent<StationaryEnemy>(); // Get the StationaryEnemy script
        _damage = GetComponentInParent<StationaryEnemy>().TrapDamage; // Get the damage from the StationaryEnemy script
    }

    /// <summary>
    /// This function deals damage to a damageable object and sets a coroutine to null after a delay
    /// in order to deal damage every second.
    /// </summary>
    /// <param name="damage">The amount of damage that will be dealt to the IDamageable object.</param>
    /// <param name="IDamageable">An interface that defines an object that can take damage. 
    /// It a method called TakeDamage that takes an integer parameter for the amount of damage to be
    /// dealt.</param>
    private IEnumerator DealDamage(int damage, IDamageable damageable)
    {
        damageable.TakeDamage(damage);
        yield return new WaitForSeconds(1f);
        _damageCoroutine = null;
    }

    /// <summary>
    /// This function deals continuous damage to a collider if it is a damageable object and the enemy
    /// is able to damage it.
    /// </summary>
    /// <param name="Collider">Collider is a component in Unity that defines the shape of an object's
    /// collision boundary. It is used to detect collisions with other objects in the game world. In
    /// this code snippet, the OnTriggerStay method is called when another collider stays within the
    /// trigger collider of the object that this script is attached to.</param>
    protected virtual void OnTriggerStay(Collider other)
    {
        Debug.Log("OnTriggerStay:" + other.name);
        if (!_isContinuousDamage) return; // If the damage is not continuous, return (do nothing)
        IDamageable damageable = other.GetComponent<IDamageable>(); // Get the IDamageable interface from the collider
        if (damageable != null && _damageCoroutine == null)
        {
            /* This code block is checking if the `StationaryEnemy` that this `TrapDamager` script is
            attached to can damage the `damageable` object that entered its trigger collider. If it
            can, it starts a coroutine to deal damage to the `damageable` object every second. If it
            cannot damage the `damageable` object but the `damageable` object is a
            `PlayerController`, it also starts the coroutine to deal damage to the
            `PlayerController`. */
            if (_stationaryEnemy.CanDamageEnemy)
                _damageCoroutine = StartCoroutine(DealDamage(_damage, damageable));
            else if (!_stationaryEnemy.CanDamageEnemy && damageable is PlayerController)
                _damageCoroutine = StartCoroutine(DealDamage(_damage, damageable)); 
        }
    }

    /// <summary>
    /// This function checks if a collider has a component that implements the IDamageable interface and
    /// deals damage to it if it meets certain conditions.
    /// </summary>
    /// <param name="Collider">Collider is a component in Unity that defines the shape of an object's
    /// collision boundary. It is used to detect collisions with other objects in the game world. In
    /// this code snippet, the OnTriggerEnter method is called when another collider enters the
    /// trigger collider of the object that this script is attached to.</param>
    protected virtual void OnTriggerEnter(Collider other)
    {
        Debug.Log("OnTriggerEnter:" + other.name);
        if (_isContinuousDamage) return; // If the damage is continuous, return (do nothing)
        IDamageable damageable = other.GetComponent<IDamageable>(); // Get the IDamageable interface from the collider

        if (damageable != null)
        {
            if (_stationaryEnemy.CanDamageEnemy)
                damageable.TakeDamage(_damage); // If the enemy can damage the damageable object, deal damage to it
            else if (!_stationaryEnemy.CanDamageEnemy && damageable is PlayerController)
                damageable.TakeDamage(_damage); // If the enemy cannot damage the damageable object but it is a PlayerController, deal damage to it
        }
    }
}
