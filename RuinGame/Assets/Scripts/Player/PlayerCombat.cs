using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using MoreMountains.Feedbacks;

// Path: Assets\Scripts\Player\PlayerCombat.cs
// Player Combat Script responsible for handling player combat i.e attacking, and etc.
public class PlayerCombat : MonoBehaviour
{
    [SerializeField] private Animator _animator; // reference to the animator component
    [SerializeField] private float attackInputWindow = 0.5f; // the time window in which the player can input the next attack
    [SerializeField] private int maxComboStage = 2; // the maximum combo stage the player can reach
    [SerializeField] private MMF_Player[] _SFX_swings; // the sound effect for the player's attack
    [SerializeField] private GameObject[] _VFX_slashes; // the visual effect for the player's attack
    [SerializeField] private SphereCollider _weaponCollider; // the collider for the player's weapon
    
    private InputAction _attackAction = null; // the input action for the player's attack
    
    private bool attackTriggered = false; // whether the player has triggered an attack
    private bool fatigued = false; // whether the player is fatigued
    private bool _attacking = false; // whether the player is attacking
    private int comboStage = 0; // the current combo stage the player is in
    private float attackTimer = 0f; // the timer for the player's attack input window

    /// <summary>
    /// This function sets the attack action.
    /// </summary>
    /// <param name="context">The context parameter is an object that contains information about the
    /// input action that triggered the callback function. It includes information such as the current
    /// state of the input (e.g. whether the button is pressed or released), the time the input
    /// occurred, and the device that generated the input.</param>
    public void onAttackAction(InputAction.CallbackContext context) 
    { 
        _attackAction = context.action;
    }

    // Update is called once per frame
    private void Update()
    {
        /* This code block checks if the `_attackAction` variable is not null and if it has been
        triggered (i.e. the player has inputted an attack). If both conditions are true, it sets
        the "Attack" trigger in the animator component, which will play the attack animation. */
        if (_attackAction != null && _attackAction.triggered) //
        {
            _animator.SetTrigger("Attack");
            _attacking = true;
        }
        if (_attacking) attackTimer += Time.deltaTime; // increments the attack timer

        if (attackTimer > attackInputWindow) // if the attack timer exceeds the attack input window
        {
            _attacking = false; // the player is no longer attacking
            ResetCombo(); // reset the combo
            _animator.SetInteger("Combo", comboStage); // set the combo stage in the animator component
            _animator.ResetTrigger("Attack"); // reset the "Attack" trigger in the animator component
        }
    }

    /// <summary>
    /// This function increases the combo stage and sets the animator integer "Combo" if the attack
    /// timer is within the attack input window.
    /// </summary>
    public void TriggerTransition()
    {
        if (attackTimer <= attackInputWindow)
        {
            _attacking = true;
            comboStage++;
            attackTimer = 0f;
            _animator.SetInteger("Combo", comboStage);
        }
    }

    /// <summary>
    /// The function resets the combo stage, resets the attack timer, sets the
    /// combo integer in the animator to 0, and resets the attack trigger.
    /// </summary>
    public void ResetCombo()
    {
        comboStage = 0;
        attackTimer = 0f;
        _animator.SetInteger("Combo", comboStage);
        _animator.ResetTrigger("Attack");
    }

    /// <summary>
    /// These are three functions that play sound and activate visual effects for three different player
    /// swings, and then disable the visual effect after a certain amount of time.
    /// </summary>
    public void PlayerSwing1() { _SFX_swings[0].PlayFeedbacks(); _VFX_slashes[0].SetActive(true); StartCoroutine(DisableSlash(0));}
    public void PlayerSwing2() { _SFX_swings[1].PlayFeedbacks(); _VFX_slashes[1].SetActive(true); StartCoroutine(DisableSlash(1));}
    public void PlayerSwing3() { _SFX_swings[2].PlayFeedbacks(); _VFX_slashes[2].SetActive(true); StartCoroutine(DisableSlash(2));}

    /// <summary>
    /// This function disables a specific game object after a delay.
    /// </summary>
    /// <param name="index">The index parameter is an integer that represents the index of the slash
    /// effect that needs to be disabled. The method waits for 0.3 seconds before disabling the slash
    /// effect at the specified index.</param>
    private IEnumerator DisableSlash(int index)
    {
        yield return new WaitForSeconds(0.3f);
        _VFX_slashes[index].SetActive(false);
    }

    /// <summary>
    /// This function enables a weapon collider and starts a coroutine to disable it, which servers as the damage dealer.
    /// </summary>
    public void EnableWeaponCollider() { 
        _weaponCollider.enabled = true;
        StartCoroutine(DisableWeaponCollider());
    }

    private IEnumerator DisableWeaponCollider()
    {
        yield return new WaitForSeconds(0.1f);
        _weaponCollider.enabled = false;
    }
}
