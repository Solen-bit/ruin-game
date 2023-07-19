using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MoreMountains.Feedbacks;

// Base class for enemy abilities
public class EnemyAbility : ScriptableObject
{
    [SerializeField] private float cooldownTime; // cooldown time
    [SerializeField] private float activeTime; // duration of the ability
    [SerializeField] private int _damage; // damage of the ability
    [SerializeField] private bool isAbilityRanged = false; // is the ability ranged

    /* `public enum AbilityState` is defining an enumeration type called `AbilityState`. It has three
    possible values: `ready`, `active`, and `cooldown`. This enum is used to represent the current
    state of the enemy's ability. */
    public enum AbilityState {
        ready,
        active,
        cooldown
    }

    // Getters and setters for class variables
    public float EACooldownTime { get => cooldownTime; set => cooldownTime = value; }
    public float EAActiveTime { get => activeTime; set => activeTime = value; }
    public bool EAIsAbilityRanged { get => isAbilityRanged; set => isAbilityRanged = value; }
    public int EADamage { get => _damage; set => _damage = value; }

    private RangedAttackRadius _rangedAttackRadius; // Ranged attack radius reference

    public virtual void Activate(MMF_Player feedback, Enemy enemy, PlayerController player) {} // Called whenever the ability is activated
    public virtual void Activate(MMF_Player feedback, Enemy enemy, PlayerController player, RangedAttackRadius attackRadius) {} // Called whenever the ability is activated
    public virtual bool CanUseAbility() { return true; } // Called whenever the ability is activated

    public virtual void SetupRangedAbility(Enemy enemy) 
    {
        if (isAbilityRanged)
        {
            _rangedAttackRadius = enemy.GetComponentInChildren<RangedAttackRadius>();
        }
    } // Called whenever the ability is ranged
    public virtual void ResetAbility(Enemy enemy) {} // Called whenever the ability is reset           

    
}
