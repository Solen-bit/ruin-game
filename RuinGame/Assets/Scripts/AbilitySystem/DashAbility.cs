using System.Collections;
using UnityEngine;
using MoreMountains.Feedbacks;

// Player dash ability, which extends the Ability class
[CreateAssetMenu]
public class DashAbility : Ability
{
    [SerializeField] private float dashVelocity; // Velocity of the dash
    [SerializeField] private float maxDashDistance = 5f; // Max distance the player can dash
    
    private GameObject parent; // Parent GameObject
    private Animator animator; // Animator for the player
    private PlayerController playerController; // Player controller for the player

    /// <summary>
    /// The function sets the parent object and retrieves its animator and player controller components.
    /// </summary>
    /// <param name="GameObject">GameObject is a class in Unity that represents a game object in the
    /// scene. It is used to create, manipulate, and destroy game objects. In this code snippet, the
    /// SetParent method takes a GameObject parameter named "parent", which is used to set the parent of
    /// the current game object.</param>
    public override void SetParent(GameObject parent) 
    {
        this.parent = parent;
        animator = parent.GetComponent<Animator>();
        playerController = parent.GetComponent<PlayerController>();
    }

    /// <summary>
    /// The function activates a dash action for a player character in a game, playing feedback and
    /// setting animation while dashing in a given direction.
    /// </summary>
    public override void Activate(MMF_Player feedback)
    {
        Vector3 dashDirection = parent.transform.forward; // Get the direction the player is facing
        
        // Disable the player controller so the player cannot move while dashing
        parent.GetComponent<PlayerController>().enabled = false;

        feedback.PlayFeedbacks(); // Play the feedback for the dash
        animator.SetBool("isDashing", true); // Set the isDashing animation parameter to true

        playerController.StartCoroutine(Dash(dashDirection));
    }

    // Coroutine for the dash action
    private IEnumerator Dash(Vector3 dashDirection)
    {
        float distance = maxDashDistance; // Get the distance the enemy will charge
        float time = distance / dashVelocity; // Get the time it will take the enemy to charge

        float t = 0f; // Set the time to 0
        while (t <= time) // While the time is less than the time it will take to charge
        {
            t += Time.deltaTime; // Add to the time variable
            parent.transform.position += dashDirection * dashVelocity * Time.deltaTime; // Move the enemy forward
            yield return null; // Wait for the next frame
        }
    }

    // On ability end, set the isDashing animation parameter to false and enable the player controller
    public override void BeginCooldown()
    {
        animator.SetBool("isDashing", false);
        parent.GetComponent<PlayerController>().enabled = true;
    }
}
