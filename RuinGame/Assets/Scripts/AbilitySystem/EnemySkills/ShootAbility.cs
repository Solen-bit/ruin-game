using System.Collections;
using UnityEngine;
using MoreMountains.Feedbacks;
using UnityEngine.AI;

// Path: Assets\Scripts\AbilitySystem\EnemySkills\ShootAbility.cs
// Enemy shoot ability, extends abstract EnemyAbility
[CreateAssetMenu(fileName = "Shoot Ability", menuName = "ScriptableObject/Abilities/Shoot")]
public class ShootAbility : EnemyAbility
{
    [SerializeField] float _anticipationTime = 1f; // Anticipation time before the enemy shoots

    public override void Activate(MMF_Player feedback, Enemy enemy, PlayerController player, RangedAttackRadius attackRadius) 
    {
        if (enemy.AbilityCoroutine == null) enemy.AbilityCoroutine = enemy.StartCoroutine(Shoot(feedback, enemy, enemy.PlayerController, attackRadius));
        enemy.Animator.SetFloat("EnemyBlend", 0f);
    }

    // Enemy shoot coroutine
    IEnumerator Shoot(MMF_Player feedback, Enemy enemy, PlayerController player, RangedAttackRadius attackRadius)
    {
        WaitForSeconds wait = new WaitForSeconds(_anticipationTime); // anticipation time

        enemy.Agent.enabled = false; // Disable the agent so that the enemy doesn't move while using the ability
        enemy.Movement.enabled = false; // Disable the movement script so that the enemy doesn't move while using the ability 

        enemy.Movement.State = EnemyState.UsingAbility; // Set the enemy's state to using ability
        enemy.transform.LookAt(player.transform.position); // Make the enemy look at the player
        enemy.Animator.SetTrigger("Attack"); // Trigger the attack animation

        yield return wait; // anticipation time

        if (attackRadius != null) // if the attack radius is not null
        {
            attackRadius.AttackRadiusShoot(); // call the attack radius's shoot function, which will shoot a projectile
            feedback.PlayFeedbacks(); // play the visual and audio effects
        }
        else
            Debug.LogError("RangedAttackRadius is null");

        yield return new WaitForSeconds(1); // reload time

        enemy.Agent.enabled = true; // Enable the agent so that the enemy can move again
        enemy.Movement.enabled = true; // Enable the movement script so that the enemy can move again

        if (NavMesh.SamplePosition(enemy.transform.position, out NavMeshHit hit, 1f, enemy.Agent.areaMask)) // if the enemy is on the navmesh
        {
            enemy.Agent.Warp(hit.position); // warp the enemy to the navmesh 
            enemy.Movement.State = EnemyState.Chase; // set the enemy's state to chase
        }

        enemy.AbilityCoroutine = null; // set the ability coroutine to null
    }

    /// <summary>
    /// This function resets an enemy's ability by stopping its coroutine and enabling its movement and
    /// attack radius.
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
        }
    }
}
