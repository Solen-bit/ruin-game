using System.Collections;
using UnityEngine;
using MoreMountains.Feedbacks;

// Path: Assets\Scripts\Enemies\ExplosionTrapEnemy.cs
// Script for the explosion trap enemy, which is a stationary enemy that explodes when the player
// gets too close to it.
public class ExplosionTrapEnemy : StationaryEnemy // Inherits from StationaryEnemy
{
    [SerializeField] private float _detectionRadius = 5f; // Detection radius of the trap
    [SerializeField] private float _cooldown = 5f; // Cooldown of the trap
    [SerializeField] private SphereCollider _sphereColliderDamager; // Sphere collider for the trap used as the damager

    private MMF_Player _propFeedback; // Feedback for the explosion
    private ParticleSystem _explosion; // Explosion particle system
    private bool _isPlayerDetected = false; // Is the player detected?
    private bool _onCooldown = false; // Is the trap on cooldown?
    private bool _isExploding = false; // Is the trap exploding?
    private GameObject _explodingProp; // The prop that explodes

    // Override Awake function
    public override void OverrideAwake()
    {
        base.OverrideAwake(); // Call base function

        _explodingProp = transform.GetChild(0).gameObject; // Get the prop that explodes, which is the first child of the trap GameObject
        _sphereColliderDamager = GetComponentInChildren<SphereCollider>(); // Get the sphere collider for the trap
        _explosion = GetComponent<ParticleSystem>(); // Get the explosion particle system
        _propFeedback = GetComponent<MMF_Player>(); // Get the feedback for the explosion
    }

    // Override Update function
    public override void OverrideUpdate()
    {
        base.OverrideUpdate(); // Call base function

        _isPlayerDetected = Physics.CheckSphere(transform.position, _detectionRadius, LayerMask.GetMask("Player")); // Check if player is detected

        // If the player is detected and the trap is not on cooldown and not exploding, then explode
        if (_isPlayerDetected) {
            if (!_onCooldown && !_isExploding) {
                StartCoroutine(Explode());
            }
        }
    }

    // Coroutine for the explosion
    public IEnumerator Explode() {
        _isExploding = true; // Set the trap to exploding
        float duration = _explosion.main.duration; // Get the duration of the explosion particle system

        _propFeedback.PlayFeedbacks(); // Play the feedback for the explosion
        _explosion.Play(); // Play the explosion particle system

        yield return new WaitForSeconds(duration - (duration - 0.8f)); // Wait a bit before enabling the damager

        _FB_swing.PlayFeedbacks(); // Play the feedback for the swing
        _sphereColliderDamager.enabled = true; // Enable the damager

        yield return new WaitForSeconds(0.1f); // Wait a bit before disabling the damager

        _sphereColliderDamager.enabled = false; // Disable the damager

        yield return new WaitUntil(() => _explosion.time >= duration); // Wait until the explosion particle system is done

        _onCooldown = true; // Set the trap to on cooldown
        _isExploding = false; // Set the trap to not exploding

        yield return new WaitForSeconds(_cooldown); // Wait for the cooldown

        _onCooldown = false; // Set the trap to not on cooldown
    }

    /// <summary>
    /// This function draws a red wire sphere with a given radius around the object's position in the
    /// scene view, which is used to visualize the detection radius of the trap.
    /// </summary>
    private void OnDrawGizmos() {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, _detectionRadius);
    }
}
