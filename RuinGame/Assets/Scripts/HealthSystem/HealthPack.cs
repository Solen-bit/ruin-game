using System.Collections;
using UnityEngine;
using MoreMountains.Feedbacks;

// Health pack class, which handles the health pack respawning and healing the player
public class HealthPack : MonoBehaviour
{
    [SerializeField] private int _healAmount = 2; // Amount of health to heal the player
    [SerializeField] private float _respawnTime = 30f; // Time it takes for the health pack to respawn
    [SerializeField] private Healthbar _healthbar; // Healthbar UI

    private bool _active = true; // Boolean variable that determines whether the health pack is active or not
    private MMF_Player _healthPackFeedbacks; // Feedback for the health pack
    private Animator _animator; // Animator for the health pack 
    private SphereCollider _collider; // Collider for the health pack
    private MeshRenderer _meshRenderer; // Mesh renderer for the health pack

    // Awake is called before the first frame update
    void Awake()
    {
        _animator = GetComponent<Animator>(); // Get the animator
        _collider = GetComponent<SphereCollider>(); // Get the collider
        _meshRenderer = GetComponent<MeshRenderer>(); // Get the mesh renderer
    }

    // Start is called on the first frame update
    void Start()
    {
        _healthPackFeedbacks = GetComponent<MMF_Player>(); // Get the feedback
    }

    /// <summary>
    /// This function triggers when the player collides with a health pack, and if the player's health
    /// is not already at maximum, it increases their health by a set amount and plays feedback
    /// animations.
    /// </summary>
    /// <param name="Collider">A collider is a component that defines the shape of an object for the
    /// purposes of physical collisions in a game engine. It is used to detect when two objects collide
    /// with each other. In this code, the OnTriggerEnter method is called when the collider of the
    /// health pack object collides with the collider of the other</param>
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("Player")) // If the other object is the player
        {
            UnitHealth playerHealth = GameManager.gameManager._playerHealth; // Get the player's health
            PlayerController playerController = other.GetComponent<PlayerController>(); // Get the player controller

            if (playerHealth.Health == playerHealth.MaxHealth) { return; } // If the player's health is already at maximum, return (skip the rest of the code)

            playerHealth.Health += _healAmount; // Increase the player's health by the heal amount

            _healthPackFeedbacks.PlayFeedbacks(); // Play the feedbacks
            playerController.HealPlayer.PlayFeedbacks(); // Play the player's heal feedbacks
            
            // If the player's health is greater than the maximum health after getting healed, set the player's health to the maximum health
            if (playerHealth.Health > playerHealth.MaxHealth) { playerHealth.Health = playerHealth.MaxHealth; } 
            _healthbar.SetHealth(playerHealth.Health); // Update the healthbar UI
            StartCoroutine(Respawn()); // Start the respawn coroutine
        }
    }

    /// <summary>
    /// This function disables and enables the mesh renderer and collider components after a certain
    /// amount of time has passed, used for respawning in a game.
    /// </summary>
    private IEnumerator Respawn()
    {
        _meshRenderer.enabled = false;
        _collider.enabled = false;
        yield return new WaitForSeconds(_respawnTime);
        _meshRenderer.enabled = true;
        _collider.enabled = true;
    }
}
