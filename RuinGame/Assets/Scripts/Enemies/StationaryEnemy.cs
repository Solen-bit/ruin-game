using MoreMountains.Feedbacks;
using UnityEngine;

// This class is the parent class of all the enemies that are not moving such as Turrets, Traps, etc.
public class StationaryEnemy : MonoBehaviour
{   
    [SerializeField] protected Transform _player; // Reference to the player
    protected Animator _animator = null; // Reference to the animator

    [SerializeField] protected MMF_Player _FB_hit; // Feedback when the enemy is hit
    [SerializeField] protected MMF_Player _FB_swing; // Feedback when the enemy is swinging
    [SerializeField] protected MMF_Player _FB_death; // Feedback when the enemy dies

    [SerializeField] protected bool _canDamageEnemy = true; // Can this enemy take damage?
    public bool CanDamageEnemy { get => _canDamageEnemy; set => _canDamageEnemy = value; } // Property to access the _canDamageEnemy variable
    
    [SerializeField] protected int _damage = 0; // Damage that this enemy deals
    public int TrapDamage { get => _damage; set => _damage = value; } // Property to access the _damage variable

    // Awake is called before the first frame update
    private void Awake()
    {
        _animator = GetComponent<Animator>(); // Get the animator component
        OverrideAwake(); // Call the OverrideAwake method
    }

    public virtual void OverrideAwake(){} // Override this method in the child class

    // Built in Unity method that is called when the object is disabled
    private void OnDisable()
    {
        if (_FB_death != null) 
        {
            _FB_death.transform.position = transform.position; // Set the position of the feedback to the position of the enemy
            _FB_death.PlayFeedbacks(); // Play the feedback
        }
    }

    // Update is called once per frame
    void Update()
    {
        OverrideUpdate(); // Call the OverrideUpdate method
    }

    public virtual void OverrideUpdate(){} // Override this method in the child class

    public virtual void TrapAttack() {} // Override this method in the child class
}
