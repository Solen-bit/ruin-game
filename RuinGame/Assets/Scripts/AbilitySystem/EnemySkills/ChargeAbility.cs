using System.Collections;
using UnityEngine;
using MoreMountains.Feedbacks;
using UnityEngine.AI;

// Path: Assets\Scripts\AbilitySystem\EnemySkills\ChargeAbility.cs
// Enemy charge ability, extends abstract EnemyAbility
[CreateAssetMenu(fileName = "Charge Ability", menuName = "ScriptableObject/Abilities/Charge")]
public class ChargeAbility : EnemyAbility
{
    [SerializeField] float _jumpSpeed = 15f; // Jump speed
    [SerializeField] float _chargeDistance = 8f; // Charge distance
    [SerializeField] float _fatigueTime = 4f; // Fatigue time
    [SerializeField] float _anticipationTime = 1f; // Anticipation time

    // Activate the ability
    public override void Activate(MMF_Player feedback, Enemy enemy, PlayerController player) 
    {
        if (enemy.AbilityCoroutine == null) enemy.AbilityCoroutine = enemy.StartCoroutine(Charge(feedback, enemy, enemy.PlayerController));
        enemy.Animator.SetFloat("EnemyBlend", 0f);
    }

    // Enemy charge coroutine - causes the enemy to dash at the player
    IEnumerator Charge(MMF_Player feedback, Enemy enemy, PlayerController player)
    {
        enemy.Agent.enabled = false; // Disable the agent so that the enemy doesn't move while charging
        enemy.Movement.enabled = false; // Disable the movement script so that the enemy doesn't move while charging

        enemy.Movement.State = EnemyState.UsingAbility; // Set the enemy's state to using ability

        enemy.Movement.Sight.enabled = false; // Disable the sight script so that the enemy doesn't move while charging
        enemy.AttackRadius.enabled = false; // Disable the attack radius so that the enemy doesn't move while charging

        enemy.transform.LookAt(player.transform.position); // Make the enemy look at the player
        Vector3 direction = enemy.transform.forward; // Get the direction the enemy is facing

        float distance = _chargeDistance; // Get the distance the enemy will charge
        float time = distance / _jumpSpeed; // Get the time it will take the enemy to charge

        enemy.Animator.SetTrigger("Attack"); // Trigger the attack animation

        yield return new WaitForSeconds(_anticipationTime); // Wait for the anticipation time

        feedback.PlayFeedbacks(); // Play the feedbacks
        float t = 0f; // Set the time to 0
        while (t <= time) // While the time is less than the time it will take to charge
        {
            t += Time.deltaTime; // Add to the time variable
            enemy.transform.position += direction * _jumpSpeed * Time.deltaTime; // Move the enemy forward
            yield return null; // Wait for the next frame
        }

        enemy.Animator.SetTrigger("FatigueTrigger"); // Trigger the fatigue animation
        enemy.Animator.SetBool("Fatigue", true); // Set the fatigue bool to true

        yield return new WaitForSeconds(_fatigueTime); // Wait for the fatigue time

        enemy.Animator.SetBool("Fatigue", false); // Set the fatigue bool to false
        
        enemy.Movement.enabled = true; // Enable the movement script
        enemy.AttackRadius.enabled = true; // Enable the attack radius
        enemy.Agent.enabled = true; // Enable the agent

        if (NavMesh.SamplePosition(enemy.transform.position, out NavMeshHit hit, 1f, enemy.Agent.areaMask)) // If the enemy is on the navmesh
        {
            enemy.Agent.Warp(hit.position); // Warp the enemy to the navmesh
            enemy.Movement.State = EnemyState.Chase; // Set the enemy's state to chase
        }

        enemy.Movement.Sight.enabled = true; // Enable the sight script

        enemy.AbilityCoroutine = null; // Set the ability coroutine to null
    }

    /// <summary>
    /// This function resets an enemy's ability by stopping its coroutine and resetting various
    /// components.
    /// </summary>
    public override void ResetAbility(Enemy enemy) 
    {
        if (enemy.AbilityCoroutine != null)
        {
            enemy.StopCoroutine(enemy.AbilityCoroutine);
            enemy.AbilityCoroutine = null;
            enemy.Agent.enabled = true;
            enemy.Movement.enabled = true;
            enemy.Movement.Sight.enabled = true;
            enemy.AttackRadius.enabled = true;

            enemy.Movement.State = EnemyState.Default;

            enemy.Animator.SetBool("Fatigue", false);
        }
    }
}
