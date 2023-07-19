using MoreMountains.Feedbacks;
using UnityEngine;
using UnityEngine.InputSystem;

// PlayerController.cs for the player. Handles player movement, combat, and dashing. Core of the player character
public class PlayerController : MonoBehaviour, IDamageable
{
    [SerializeField] private MMF_Player _SFX_hit; // feedback for when the player is hit by an enemy
    [SerializeField] private MMF_Player _SFX_footstep; // feedback for when the player takes a step
    [SerializeField] private MMF_Player _FB_heal; // feedback for when the player heals
    [SerializeField] private Healthbar _healthbar; // healthbar for the player 
    [SerializeField] private int _damage = 1; // player attack damage
    private PlayerCombat _combat; // player combat reference
    private PlayerInputs _playerInputs; // player inputs reference
    private CharacterController _characterController; // character controller reference
    private Animator _animator; // animator reference
    
    private bool _isGameOver; // boolean to check if the game is over

    // masks
    [SerializeField] private LayerMask _enemyMask; // enemy mask

    // blend tree variables
    private int _blendHash; // hash for the blend tree
    private float _idleBlend = 0.0f; // idle blend value
    private float _runBlend = 1f; // run blend value

    // variables to store optimized setter/getter parameter IDs
    private int _isCombattingHash;
    private int _isAttackingHash;
    
    // movement variables
    private Vector2 _currentMovementInput;
    private Vector3 _currentMovement; 
    private Vector3 _currentSprintMovement; 
    private bool _isMovementPressed;
    private bool _isSprintPressed;

    // attack variables
    private bool _isAttackPressed;
    private InputAction _attackAction;

    // soundtrack variables
    [SerializeField] private float _enemiesInRangeThreshold = 9f; // threshold for the soundtrack to change
    private bool _enemiesInRange;

    // Constants
    private float _rotationFactorPerFrame = 35.0f;
    private float _sprintMultiplier = 2.0f;
    private float _moveSpeed = 5.0f;
    private int _zero = 0;

    // gravity variables
    private float _gravity = -9.81f;
    private float _groundedGravity = -.05f;

    // dashing variables
    private bool _isDashPressed = false;

    // Getters and Setters
    public bool IsDashPressed { get => _isDashPressed; set => _isDashPressed = value; }
    public int PlayerDamage { get => _damage; set => _damage = value; }
    public MMF_Player HealPlayer { get => _FB_heal; set => _FB_heal = value; }

    private void Awake()
    {
        // Initializing the components
        _combat = GetComponent<PlayerCombat>();
        _playerInputs = new PlayerInputs();  
        _characterController = GetComponent<CharacterController>();
        _animator = GetComponent<Animator>();

        // Defining hashes for the animator to optimize.
        _isAttackingHash = Animator.StringToHash("isAttacking");
        _isCombattingHash = Animator.StringToHash("isCombatting");
        _blendHash = Animator.StringToHash("Blend");

        // Defining actions
        _attackAction = _playerInputs.CharacterControls.Attack;

        /* Subscribing to the events of the input system. */
        _playerInputs.CharacterControls.Move.started += onMovementInput;
        _playerInputs.CharacterControls.Move.canceled += onMovementInput;
        _playerInputs.CharacterControls.Move.performed += onMovementInput;
        _playerInputs.CharacterControls.Dash.started += onDash;
        _playerInputs.CharacterControls.Dash.canceled += onDash;
        _attackAction.performed += _combat.onAttackAction;
        _attackAction.canceled += _combat.onAttackAction;
    }

    // OnEnable and OnDisable are called when the script is enabled or disabled.
    void OnEnable() { _playerInputs.CharacterControls.Enable(); }
    void OnDisable() { _playerInputs.CharacterControls.Disable(); }

    // Start is called before the first frame update
    void Start()
    {
        _characterController.Move(_currentMovement * Time.deltaTime); // Move the character controller
    }

    // Update is called once per frame
    void Update()
    {
        HandleRotation(); // Handle the rotation of the player

        if (!_isMovementPressed) { Idle(); }
        else if (_isMovementPressed) { Run(); }

        if (!_characterController.isGrounded) { Fall(); }
        _characterController.Move(_currentMovement * Time.deltaTime);

        _enemiesInRange = Physics.CheckSphere(transform.position, _enemiesInRangeThreshold, _enemyMask); // Check if there are enemies in range

        // If there are enemies in range, set the combat mode to true.
        // Else, set the combat mode to false.
        if (_enemiesInRange) { GameManager.gameManager.combatMode = true; } 
        else { GameManager.gameManager.combatMode = false; }
        
    }

    /// <summary>
    /// This function handles the rotation of a character to face the direction of movement.
    /// </summary>
    void HandleRotation()
    {
        Vector3 positionToLookAt; // Vector3 to store the position to look at.

        positionToLookAt.x = _currentMovement.x; // Set the x position to the x position of the movement.
        positionToLookAt.y = 0.0f; // Set the y position to 0.
        positionToLookAt.z = _currentMovement.z; // Set the z position to the z position of the movement.

        Quaternion currentRotation = transform.rotation; // Store the current rotation of the character.

        if (_isMovementPressed) {
            /* Rotating the character to face the direction of the movement. */
            Quaternion targetRotation = Quaternion.LookRotation(positionToLookAt);
            transform.rotation = Quaternion.Slerp(currentRotation, targetRotation, _rotationFactorPerFrame * Time.deltaTime);
        }
    }

    /// <summary>
    /// The function reads the value of the attack input button and sets the corresponding boolean
    /// variable.
    /// </summary>
    /// <param name="context">context is a parameter of type InputAction.CallbackContext that represents
    /// the context of the input action being performed. It contains information such as the input
    /// device, the control path, the input value, and the phase of the input action. This parameter is
    /// used in the onAttack method to read the value of</param>
    void onAttack(InputAction.CallbackContext context)
    {
        _isAttackPressed = context.ReadValueAsButton();
    }

    /// <summary>
    /// The function checks if the dash button is pressed and sets a boolean value accordingly.
    /// </summary>
    /// <param name="context">context is an object of type InputAction.CallbackContext which contains
    /// information about the input action that triggered the callback. It includes information such as
    /// the input device used, the value of the input, and the phase of the input (e.g. whether it was
    /// pressed or released).</param>
    void onDash(InputAction.CallbackContext context)
    {
        _isDashPressed = context.ReadValueAsButton();
    }

    /// <summary>
    /// This function updates the player's movement input based on user input.
    /// </summary>
    /// <param name="context">An object that contains information about the input action that triggered
    /// this method, such as the current value of the input and whether it was just pressed or
    /// released.</param>
    void onMovementInput(InputAction.CallbackContext context)
    {
            _currentMovementInput = context.ReadValue<Vector2>();
            _currentMovement.x = _currentMovementInput.x;
            _currentMovement.z = _currentMovementInput.y;
            _isMovementPressed = _currentMovementInput.x != _zero || _currentMovementInput.y != _zero;
    }

    /// <summary>
    /// The function sets the animator blend value to idle and sets the current movement to zero.
    /// Used when the player is not moving.
    /// </summary>
    private void Idle()
    {
        _animator.SetFloat(_blendHash, _idleBlend);
        _currentMovement.x = 0;
        _currentMovement.z = 0;
    }

    /// <summary>
    /// This function sets the current movement speed based on the input and updates the animator blend
    /// value if the attack button is not pressed.
    /// </summary>
    private void Run()
    {
        if (!_isAttackPressed) { _animator.SetFloat(_blendHash, _runBlend); }
        _currentMovement.x = _currentMovementInput.x * _moveSpeed;
        _currentMovement.z = _currentMovementInput.y * _moveSpeed;
    }

    private void Fall() { HandleGravity(); } // Handle the gravity of the player
    private void HandleGravity() { _currentMovement.y = _currentMovement.y + _gravity * Time.deltaTime; } // Handle the gravity of the player by adding it to the current y movement

    // Called by an animation event in the walking animation of the player character
    public void PlayerFootstep() { _SFX_footstep.PlayFeedbacks(); }

    /// <summary>
    /// This function takes damage, updates the player's health, plays a sound effect, and sets the game
    /// to be over if the player's health reaches zero.
    /// </summary>
    /// <param name="damage">an integer value representing the amount of damage to be inflicted on the
    /// player.</param>
    public void TakeDamage(int damage)
    {
        GameManager.gameManager._playerHealth.TakeDamage(damage);
        _healthbar.SetHealth(GameManager.gameManager._playerHealth.Health);
        _SFX_hit.PlayFeedbacks();
        if (GameManager.gameManager._playerHealth.Health <= 0)
        {
            gameObject.SetActive(false);
            GameManager.gameManager._isGameOver = true;
        }
    }

    public Transform GetTransform() { return transform; }

    public void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, _enemiesInRangeThreshold);
    }
}
