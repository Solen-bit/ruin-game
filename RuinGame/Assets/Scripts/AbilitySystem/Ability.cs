using UnityEngine;
using MoreMountains.Feedbacks;

// Base class for all player abilities
public class Ability : ScriptableObject
{
    [SerializeField] private float cooldownTime; // cooldown time
    [SerializeField] private float activeTime; // duration of the ability
    [SerializeField] private bool isPressed; // is the ability pressed

    public float PlayerAbilityCooldownTime { get => cooldownTime; set => cooldownTime = value; }
    public float PlayerAbilityActiveTime { get => activeTime; set => activeTime = value; }

    public virtual void SetParent(GameObject parent) {} // Set the parent of the ability
    public virtual void Activate(MMF_Player feedback) {} // Called whenever the ability is activated
    public virtual void BeginCooldown() {} // Called whenever the ability begins cooldown
}
