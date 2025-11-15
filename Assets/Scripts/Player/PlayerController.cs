using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    private Animator _anim;
    private InputSystem_Actions _playerInputActions;
    private CharacterController _characterController;
    
    [Header("Movement Config")]
    [SerializeField] private float maxSpeed = 10f;
    [SerializeField] private float rotationSpeed = 360f;
    [SerializeField] private float jumpForce = 5f;
    [SerializeField] private float gravity = -9.81f;
    [SerializeField] private float fallMultiplier = 1f;
    [SerializeField] private float fallThreshold = 0.1f; // tolerance on how we can consider a fall for our animation
    
    [Header("Respawn Config")]
    [SerializeField] private float respawnDelay = 0.5f;

    private ToggleDimension _toggleDimension;
    private Vector3 _initialPos;
    private float _verticalVelocity;
    private float _currentSpeed;
    private Vector3 _input;
    private bool _hasJumped = false;
    // --- PLATFORM UTILS ---
    private MonoBehaviour _currentSwitchScript; // Reference to the current switch-like script the player is interacting with
    private Transform _currentPlatform; // Reference to the platform the player is standing on (if any)
    private Vector3 _platformLastPosition;
    private DimensionManager _dimensionManager; // Reference to the current scene which we will get later for respawn
    // --- KEY & DOOR UTILS ---
    private Door _currentDoor; // Reference to the current door the player can interact with
    private int _lightKeys = 0;
    private int _darkKeys = 0;

    private void Awake()
    {
        _anim = GetComponent<Animator>();
        _playerInputActions = new InputSystem_Actions();
        _characterController = GetComponent<CharacterController>();
        _dimensionManager = FindFirstObjectByType<DimensionManager>();
        _initialPos = transform.position;
    }

    private void OnEnable()
    {
        if (_playerInputActions == null)
        {
            _playerInputActions = new InputSystem_Actions();
        }
        _playerInputActions.Player.Enable();

        // Subscribe to Jump action binding
        _playerInputActions.Player.Jump.performed += OnJumpPerformed;

        // Subscribe to Interact action binding
        _playerInputActions.Player.Interact.performed += OnInteractPerformed;
    }

    private void OnDisable()
    {
        // Unsubscribe to Jump action binding
        _playerInputActions.Player.Jump.performed -= OnJumpPerformed;

        // Unsubscribe to Interact action binding
        _playerInputActions.Player.Interact.performed -= OnInteractPerformed;

        _playerInputActions.Player.Disable();
    }

    private void Update()
    {
        GetInput();
        _currentSpeed = (_input == Vector3.zero) ? 0f : maxSpeed;
        LookAndMove();
        UpdateAnimations();
        Debug.Log("isGrounded Param: " + _characterController.isGrounded);
    }

    // TO FIX 
    private void UpdateAnimations()
    {
        bool isGrounded = _characterController.isGrounded;
        bool isFalling = _hasJumped && _verticalVelocity < -fallThreshold;
        bool isJumping = _hasJumped && _verticalVelocity > 0f;

        // Update animator parameters
        _anim.SetBool("isJumping", isJumping);
        _anim.SetBool("isFalling", isFalling);
        _anim.SetBool("isGrounded", isGrounded);

        // Reset _hasJumped on landing
        if (isGrounded && _hasJumped)
        {
            _hasJumped = false;
        }
        // Update horizontal speed
        float speedNormalized = _currentSpeed / maxSpeed;
        _anim.SetFloat("speed", speedNormalized);
    }


    // Called when the the "Interact" button is pressed
    private void OnInteractPerformed(InputAction.CallbackContext context)
    {
        if (_currentSwitchScript != null)
        {
            // Use SendMessage so we can activate any script that implements an ActivateSwitch method
            _currentSwitchScript.SendMessage("ActivateSwitch", SendMessageOptions.DontRequireReceiver);
        }

        if (_currentDoor != null)
        {
            _currentDoor.TryOpen(this); // Pass 'this' (the player)
        }
    }

    public void SetCurrentSwitch(MonoBehaviour newSwitch)
    {
        _currentSwitchScript = newSwitch;
        Debug.Log("In range of switch/script: " + newSwitch.GetType().Name);
    }

    public void ClearCurrentSwitch(MonoBehaviour oldSwitch)
    {
        if (_currentSwitchScript == oldSwitch)
        {
            _currentSwitchScript = null;
            Debug.Log("Out of range of switch/script: " + oldSwitch.GetType().Name);
        }
    }

    #region Key Management

    // Called by Key.cs when a key is collected
    public void AddKey(KeyType type)
    {
        if (type == KeyType.Light)
        {
            _lightKeys++;
        }
        else if (type == KeyType.Dark)
        {
            _darkKeys++;
        }
        Debug.Log($"Collected {type} key. Total Light: {_lightKeys}, Total Dark: {_darkKeys}");
        // Here you would also update your UI
    }

    // Called by Door.cs to check if we can open it
    public bool HasKeys(int lightNeeded, int darkNeeded)
    {
        return _lightKeys >= lightNeeded && _darkKeys >= darkNeeded;
    }

    // Called by Door.cs when it successfully opens
    public void UseKeys(int lightToUse, int darkToUse)
    {
        // This assumes HasKeys was already checked
        _lightKeys -= lightToUse;
        _darkKeys -= darkToUse;
        Debug.Log($"Used keys. Remaining Light: {_lightKeys}, Remaining Dark: {_darkKeys}");
        // Here you would also update your UI
    }

    #endregion

    #region Door Interaction

    // Called by Door.cs on trigger enter
    public void SetCurrentDoor(Door newDoor)
    {
        _currentDoor = newDoor;
        Debug.Log("In range of door!");
        // Here you could show an "Press E to interact" UI prompt
    }

    // Called by Door.cs on trigger exit
    public void ClearCurrentDoor(Door oldDoor)
    {
        if (_currentDoor == oldDoor)
        {
            _currentDoor = null;
            Debug.Log("Out of range of door!");
            // Here you would hide the UI prompt
        }
    }

    #endregion
    
    // This function is called by PlatformTrigger.cs when we step ON the platform
    public void SetCurrentPlatform(Transform newPlatform)
    {
        _currentPlatform = newPlatform;
        // Store its exact position the moment we get on
        _platformLastPosition = newPlatform.position; 
    }

    // This function is called by PlatformTrigger.cs when we step OFF the platform
    public void ClearCurrentPlatform(Transform oldPlatform)
    {
        // Only clear if it's the platform we are currently on
        if (_currentPlatform == oldPlatform)
        {
            _currentPlatform = null;
        }
    }

    // TO FIX JUMP LOGIC
    private void OnJumpPerformed(InputAction.CallbackContext context)
    {
        if (_characterController.isGrounded)
        {
            _verticalVelocity = jumpForce;
            _currentPlatform = null; // Detach from any moving platform
            _hasJumped = true; // A jump was detected 
        }
    }
    // TO FIX JUMP LOGIC
    private void LookAndMove()
    {
        // Define world directions relative to isometric camera (-135 degrees)
        Vector3 forwardIso = new Vector3(-1, 0, -1).normalized;
        Vector3 rightIso = new Vector3(-1, 0, 1).normalized;
        // Combine isometric directions to get final move direction
        Vector3 moveDir = (forwardIso * _input.z + rightIso * _input.x).normalized;

        // If the player is grounded, apply small velocity, else apply gravity 
        if (_characterController.isGrounded && !_hasJumped)
        {
            _verticalVelocity = -2f;
        }
        else
        {
            float gravityApplied = _verticalVelocity < 0f ? gravity * fallMultiplier : gravity;
            _verticalVelocity += gravityApplied * Time.deltaTime;
        }
        
        // Rotate player towards movement direction
        if (moveDir.sqrMagnitude > 0.01f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(moveDir);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }

        // Combine horizontal movement and vertical velocity
        Vector3 velocity = moveDir * _currentSpeed;
        velocity.y = _verticalVelocity;

        if (_currentPlatform != null)
        {
            Vector3 platformDeltaPos = (_currentPlatform.position - _platformLastPosition) / Time.deltaTime;
            velocity += platformDeltaPos;
            _platformLastPosition = _currentPlatform.position;
        }
        
        // Move character with final combined velocity
        _characterController.Move(velocity * Time.deltaTime);
    }
    
    private void GetInput()
    {
        Vector2 input = _playerInputActions.Player.Move.ReadValue<Vector2>();
        _input = new Vector3(input.x, 0, input.y).normalized;

        Debug.Log($"Input: {_input}");
    }

    // "Respawn" the player back to the original location
    public IEnumerator Respawn() {
        Debug.Log("Started Coroutine at timestamp : " + Time.time);
        yield return new WaitForSeconds(respawnDelay);

        // Teleporting a CharacterController can cause collisions; disable it briefly
        _characterController.enabled = false;
        _verticalVelocity = 0f;
        // Switch to the other realm ONLY IF the player dies in the other realm
        if(_dimensionManager.CurrentSceneDimension != _dimensionManager.ToggleDimensionRef.IsLightRealmActive)
        {
            _dimensionManager.ToggleDimensionRef.SwitchRealm(_dimensionManager.CurrentSceneDimension);
        }
        // Spawn the player to its initial position
        transform.position = _initialPos;
        _characterController.enabled = true;

    }
}