using UnityEngine;
using MoreMountains.Feedbacks;

// Player ability holder class, which holds the ability and handles its states
public class AbilityHolder : MonoBehaviour
{
    [SerializeField] private MMF_Player _FB_ability; // Feedback for the ability
    private PlayerController _controller; // Player controller for the player
    [SerializeField] private Ability ability; // Ability for the player
    private float _cooldownTime; // Cooldown time for the ability
    private float _activeTime; // Active time for the ability

    // Ability states
    enum AbilityState {
        ready,
        active,
        cooldown
    }
    AbilityState _state = AbilityState.ready; // Current state of the ability

    // Awake is called before the first frame update
    void Awake() 
    {
        _controller = GetComponent<PlayerController>(); // Get the player controller
        ability.SetParent(gameObject); // Set the parent of the ability  
    }

    // Update is called once per frame
    void Update()
    {
        switch (_state)
        {
            case AbilityState.ready: // Ability is ready to be used
                if (_controller.IsDashPressed) { // If the ability is pressed, activate it
                    ability.Activate(_FB_ability);
                    _state = AbilityState.active;
                    _activeTime = ability.PlayerAbilityActiveTime;
                }
            break;
            case AbilityState.active:
                // While active, count down the active time
                if (_activeTime > 0) {
                    _activeTime -= Time.deltaTime;
                } else { // Ability ended, start cooldown
                    ability.BeginCooldown();
                    _state = AbilityState.cooldown;
                    _cooldownTime = ability.PlayerAbilityCooldownTime;
                }
            break;
            case AbilityState.cooldown:
                // Count down cooldown time
                if (_cooldownTime > 0) {
                    _cooldownTime -= Time.deltaTime;
                } else { // Cooldown ended, ability is ready
                    _state = AbilityState.ready;
                }
            break;
        }
    }
}
