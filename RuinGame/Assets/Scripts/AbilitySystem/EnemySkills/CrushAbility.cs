using System.Collections;
using UnityEngine;
using MoreMountains.Feedbacks;
using UnityEngine.AI;

// Path: Assets\Scripts\AbilitySystem\EnemySkills\CrushAbility.cs
// Enemy crush ability, extends abstract EnemyAbility
[CreateAssetMenu(fileName = "Crush Ability", menuName = "ScriptableObject/Abilities/Crush")]
public class CrushAbility : EnemyAbility
{
    [SerializeField] float _fatigueTime = 4f; // Fatigue time
    [SerializeField] float _crushTime = 1f; // Crush time

    // Activate the ability
    public override void Activate(MMF_Player feedback, Enemy enemy, PlayerController player) 
    {
        if (enemy.AbilityCoroutine == null) enemy.AbilityCoroutine = enemy.StartCoroutine(Crush(feedback, enemy, enemy.PlayerController));
        enemy.Animator.SetFloat("EnemyBlend", 0f);
    }

    // Enemy crush coroutine
    private IEnumerator Crush(MMF_Player feedback, Enemy enemy, PlayerController player)
    {
        enemy.Agent.enabled = false; // Disable the agent so that the enemy doesn't move while using the ability
        enemy.Movement.enabled = false; // Disable the movement script so that the enemy doesn't move while using the ability

        enemy.Movement.State = EnemyState.UsingAbility; // Set the enemy's state to using ability

        enemy.Movement.Sight.enabled = false; // Disable the sight script so that the enemy doesn't move while using the ability
        enemy.AttackRadius.enabled = false; // Disable the attack radius so that the enemy doesn't move while using the ability

        enemy.transform.LookAt(player.transform.position); // Make the enemy look at the player

        enemy.Animator.SetTrigger("Attack"); // Trigger the attack animation
        feedback.PlayFeedbacks(); // Play the visual and audio effects

        yield return new WaitForSeconds(_crushTime); // Wait for the crush time

        enemy.Animator.SetTrigger("FatigueTrigger"); // Trigger the fatigue animation
        enemy.Animator.SetBool("Fatigue", true); // Set the fatigue boolean to true

        yield return new WaitForSeconds(_fatigueTime); // Wait for the fatigue time

        enemy.Animator.SetBool("Fatigue", false); // Set the fatigue boolean to false
        
        enemy.Movement.enabled = true; // Enable the movement script
        enemy.AttackRadius.enabled = true; // Enable the attack radius
        enemy.Agent.enabled = true; // Enable the agent
        enemy.Movement.Sight.enabled = true; // Enable the sight script

        if (NavMesh.SamplePosition(enemy.transform.position, out NavMeshHit hit, 1f, enemy.Agent.areaMask)) // Warp the enemy to the nearest navmesh position
        {
            enemy.Agent.Warp(hit.position);
            enemy.Movement.State = EnemyState.Chase;
        }

        enemy.AbilityCoroutine = null; // Set the ability coroutine to null
    }

    /// <summary>
    /// This function resets an enemy's ability by stopping its coroutine and enabling its movement,
    /// sight, and attack radius.
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
