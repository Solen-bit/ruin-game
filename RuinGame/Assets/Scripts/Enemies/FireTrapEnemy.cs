using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Path: Assets\Scripts\Enemies\FireTrapEnemy.cs
// This is the script for the fire trap enemy
public class FireTrapEnemy : StationaryEnemy
{
    [SerializeField] private List<ParticleSystem> _trapParticles = new List<ParticleSystem>(); // List of trap particles
    [SerializeField] private float _activationInterval = 1f; // Activation interval
    [SerializeField] private float _activationRange = 10f; // Activation range of the trap
    [SerializeField] AudioSource _audioSource; // Audio sfx

    private float _activationTimer = 0f; // Timer for activation interval used to traverse list of trap particles
    private int _indexCounter = 0; // Index counter for the trap particles

    private bool _isPlayerNearby = false; // Is the player nearby?
    private bool _audioPlaying = false; // Is the audio playing?

    public override void OverrideUpdate()
    {
        base.OverrideUpdate();
        
        _isPlayerNearby = Physics.CheckSphere(transform.position, _activationRange, LayerMask.GetMask("Player")); // Check if player is nearby

        if (_isPlayerNearby) { // If player is nearby
            _activationTimer += Time.deltaTime; // Increment timer

            if (_activationTimer >= _activationInterval) { // If timer is greater than activation interval
                _activationTimer = 0f; // Reset timer
                _indexCounter++; // Increment index counter

                if (_indexCounter >= _trapParticles.Count) { // If index counter is greater than the number of trap particles
                    _indexCounter = 0; // Reset index counter
                }
            }

            // Only play audion if not already active, to prevent audio overlap
            if (!_audioPlaying)
            {
                _audioPlaying = true;
                _audioSource.Play();
            }
            
            StartCoroutine(ActivateTrap(_indexCounter)); // Activate trap
        } else { // If player is not nearby reset everything
            _activationTimer = 0f;
            _indexCounter = 0;
            _audioPlaying = false;
            _audioSource.Stop();
        }
    }

    /// <summary>
    /// This function activates a trap by playing its particle effect and enabling/disabling its
    /// collider at specific times.
    /// </summary>
    /// <param name="index">The index parameter is an integer that represents the index of the trap
    /// particle system in the array of trap particles. It is used to access the specific trap particle
    /// system that needs to be activated.</param>
    private IEnumerator ActivateTrap(int index) {
        if (!_trapParticles[index].isPlaying) { // If trap particle is not already playing
            float _particleDuration = _trapParticles[index].main.duration; // Get particle duration
            BoxCollider collider = _trapParticles[index].GetComponentInChildren<BoxCollider>(); // Get collider

            _trapParticles[index].Play(); // Play particle effect

            yield return new WaitForSeconds(_particleDuration - (_particleDuration - 0.8f));  // Wait a bit before enabling collider
            collider.enabled = true; // Enable collider

            yield return new WaitUntil(() => _trapParticles[index].time >= _particleDuration - 1.8); // Wait until particle effect is almost done
            collider.enabled = false; // Disable collider
        } 
    }
}
