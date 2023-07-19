using System.Collections;
using UnityEngine;

// This script contains the logic for the enemy's sight
[RequireComponent(typeof(SphereCollider))]
public class EnemyLineOfSightChecker : MonoBehaviour
{
    // Components
    private float _fieldOfView = 90f; // The field of view of the enemy
    private LayerMask _obstacleMask; // The layer of the obstacles

    private Transform _target; // Location of the target that the enemy is looking for

    // Events
    public delegate void GainSightEvent(PlayerController player); // Event that is called when the enemy gains sight of the player, caught by the EnemyMovement script
    public GainSightEvent OnGainSight;
    public delegate void LoseSightEvent(PlayerController player); // Event that is called when the enemy loses sight of the player, caught by the EnemyMovement script
    public LoseSightEvent OnLoseSight;

    private Coroutine _checkForPlayerCoroutine; // Coroutine that checks for the player
    private bool _playerSeen = false; // Whether or not the player has been seen

    public bool PlayerSeen { get => _playerSeen; set => _playerSeen = value; } // Getter and setter for the player seen variable
    public float E_FieldOfView { get => _fieldOfView; set => _fieldOfView = value; } // Getter and setter for the field of view
    public LayerMask ObstacleMask { get => _obstacleMask; set => _obstacleMask = value; } // Getter and setter for the obstacle mask

    /// <summary>
    /// This function checks if the player has entered a trigger collider and starts a coroutine to
    /// check for line of sight if the player is not visible.
    /// </summary>
    /// <param name="Collider">Collider is a component in Unity that defines a physical boundary for an
    /// object. It is used to detect collisions with other objects in the scene. In this code snippet,
    /// OnTriggerEnter is a method that is called when a collider attached to an object enters another
    /// collider. The parameter "other" refers to the collider</param>
    private void OnTriggerEnter(Collider other)
    {
        PlayerController player;
        if (other.TryGetComponent<PlayerController>(out player)) // Check if the collider is the player
        {
            _target = player.transform; // Set the target to the player
            if (!CheckLineOfSight(player))
            {
                _checkForPlayerCoroutine = StartCoroutine(CheckForLineOfSightCoroutine(player));
            }
        }
    }

    /// <summary>
    /// This function checks if a player has exited a collider and stops a coroutine if it is running.
    /// </summary>
    /// <param name="Collider">Collider is a component in Unity that defines a physical shape for an
    /// object in the game world. It is used to detect collisions with other objects and trigger events
    /// when certain conditions are met, such as when two objects touch or overlap. In this code
    /// snippet, the OnTriggerExit method is called when another collider enters this collider</param>
    private void OnTriggerExit(Collider other)
    {
        PlayerController player;
        if (other.TryGetComponent<PlayerController>(out player))
        {
            OnLoseSight?.Invoke(player); // Invoke the lose sight event
            if (_checkForPlayerCoroutine != null)
            {
                StopCoroutine(_checkForPlayerCoroutine);
                _playerSeen = false;
            }
        }
    }

    /// <summary>
    /// This function checks if the player is within the field of view and line of sight of the AI
    /// character.
    /// </summary>
    /// <param name="PlayerController">A class or script that controls the player character in the game.
    /// Used to check if the player is within the field of view and
    /// line of sight of the AI character.</param>
    /// <returns>
    /// The method is returning a boolean value.
    /// </returns>
    private bool CheckLineOfSight(PlayerController player)
    {
        if (_target == null) { return false; } // No player visible

        Vector3 dirToTarget = (_target.position - transform.position).normalized; // Get the direction to the target
        float angleBetween = Vector3.Angle(transform.forward, dirToTarget); // Get the angle between the forward vector and the direction to the target

        if (angleBetween < _fieldOfView / 2) 
        {
            float distanceToTarget = Vector3.Distance(transform.position, _target.position); // Get the distance to the target

            if (!Physics.Raycast(transform.position, dirToTarget, distanceToTarget, _obstacleMask)) // Check if there is an obstacle between the AI and the player
            {
                OnGainSight?.Invoke(player); // Invoke the gain sight event
                _playerSeen = true; 
                return true;
            }
        }
        _playerSeen = false;
        return false;
    }

    /// <summary>
    /// This function checks for line of sight between the player and an object using a coroutine in C#.
    /// </summary>
    private IEnumerator CheckForLineOfSightCoroutine(PlayerController player)
    {
        WaitForSeconds wait = new WaitForSeconds(0.1f);
        while(!CheckLineOfSight(player)) { yield return wait; }
    }
}
