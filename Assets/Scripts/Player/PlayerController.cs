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
    [SerializeField] private float acceleration = 5f;
    [SerializeField] private float deceleration = 5f;
    [SerializeField] private float jumpForce = 10f;
    [SerializeField] private float gravity = -9.81f;
    [SerializeField] private float fallMultiplier = 2f;
    
    [Header("Respawn Config")]
    [SerializeField] private float respawnDelay = 0.3f;

    private Vector3 _initialPos;
    private float _verticalVelocity;
    private float _currentSpeed;
    private Vector3 _input;

    // --- PLATFORM UTILS ---
    private Switch _currentSwitch; // Reference to the current switch the player is interacting with
    private Transform _currentPlatform; // Reference to the platform the player is standing on (if any)
    private Vector3 _platformLastPosition;

    private void Awake()
    {
        _anim = GetComponent<Animator>();
        _playerInputActions = new InputSystem_Actions();
        _characterController = GetComponent<CharacterController>();
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
        CalculateSpeed();
        LookAndMove();
        UpdateAnimations();
        Debug.Log("isGrounded Param: " + _characterController.isGrounded);
    }

    private void UpdateAnimations()
    {
        bool isGrounded = _characterController.isGrounded;
        bool isJumping = _anim.GetBool("isJumping");

        _anim.SetBool("isGrounded", isGrounded);

        float speedNormalized = _currentSpeed / maxSpeed;
        _anim.SetFloat("speed", speedNormalized);

        if (!_characterController.isGrounded && _verticalVelocity < 0)
        {
            _anim.SetBool("isJumping", false);
        }
        if (isGrounded && isJumping)
        {
            _anim.SetBool("isJumping", false);
        }
    }


    // Called when the the "Interact" button is pressed
    private void OnInteractPerformed(InputAction.CallbackContext context)
    {
        if (_currentSwitch != null)
        {
            _currentSwitch.ActivateSwitch();
        }
    }

    public void SetCurrentSwitch(Switch newSwitch)
    {
        _currentSwitch = newSwitch;
        Debug.Log("In range of switch!");
    }

    public void ClearCurrentSwitch(Switch oldSwitch)
    {
        if (_currentSwitch == oldSwitch)
        {
            _currentSwitch = null;
            Debug.Log("Out of range of switch!");
        }
    }

    // Called by PlatformTrigger when the player steps onto a platform
    private void LateUpdate()
    {
        if (_currentPlatform != null)
        {
            // Calculate how much the platform has moved since the last frame
            Vector3 deltaMovement = _currentPlatform.position - _platformLastPosition;

            // Move the character controller by that same amount
            _characterController.Move(deltaMovement);

            // Update the platform's last known position for the next frame
            _platformLastPosition = _currentPlatform.position;
        }
    }
    
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

    private void OnJumpPerformed(InputAction.CallbackContext context)
    {
        if (_characterController.isGrounded)
        {
            _verticalVelocity = jumpForce;
            _anim.SetBool("isJumping", true);
            _anim.SetBool("isGrounded", false);
        }
    }

    private void CalculateSpeed()
    {
        float targetSpeed = (_input == Vector3.zero) ? 0f : maxSpeed;
        float accel = (_input == Vector3.zero) ? deceleration : acceleration;

        _currentSpeed = Mathf.MoveTowards(_currentSpeed, targetSpeed, accel * Time.deltaTime);
    }

    private void LookAndMove()
    {
        Vector2 input2D = _playerInputActions.Player.Move.ReadValue<Vector2>();
        _input = new Vector3(input2D.x, 0, input2D.y);

        Vector3 forwardIso = new Vector3(-1, 0, -1).normalized;
        Vector3 rightIso = new Vector3(-1, 0, 1).normalized;

        Vector3 moveDir = (forwardIso * _input.z + rightIso * _input.x).normalized;

        if (_characterController.isGrounded)
        {
            if (_verticalVelocity < 0) _verticalVelocity = -2f;
        }
        else
        {
            _verticalVelocity += gravity * fallMultiplier * Time.deltaTime;
        }

        if (moveDir.sqrMagnitude > 0.01f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(moveDir);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }

        Vector3 velocity = moveDir * _currentSpeed;
        velocity.y = _verticalVelocity;
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
        transform.position = _initialPos;
        _verticalVelocity = 0f;
        _currentSpeed = 0f;
        _characterController.enabled = true;
    }
}