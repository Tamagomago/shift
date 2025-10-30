using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    private InputSystem_Actions _playerInputActions;
    private CharacterController _characterController;
    [Header("Movement Config")]
    [SerializeField] private float maxSpeed = 10f;
    [SerializeField] private float rotationSpeed = 360f;
    [SerializeField] private float acceleration = 15f;
    [SerializeField] private float deceleration = 10f;
    [SerializeField] private float jumpForce = 5f;
    [SerializeField] private float gravity = -9.81f;
    [SerializeField] private float fallMultiplier = 1.5f;
    private float _verticalVelocity;
    private float _currentSpeed;
    private Vector3 _input;

    // --- PLATFORM UTILS ---
    private Switch _currentSwitch; // Reference to the current switch the player is interacting with
    private Transform _currentPlatform; // Reference to the platform the player is standing on (if any)
    private Vector3 _platformLastPosition;
    private void Awake()
    {
        _playerInputActions = new InputSystem_Actions();
        _characterController = GetComponent<CharacterController>();
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
        Look();
        CalculateSpeed();
        Move();
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
        }
        
    }
    private void CalculateSpeed()
    {
        if (_input == Vector3.zero && _currentSpeed > 0)
        {
            _currentSpeed -= deceleration * Time.deltaTime;
        }
        else if (_input != Vector3.zero && _currentSpeed < maxSpeed)
        {
            _currentSpeed += acceleration * Time.deltaTime;
        }

        _currentSpeed = Mathf.Clamp(_currentSpeed, 0, maxSpeed);
    }
    
    private void Look()
    {
        // No movement input is read
        if (_input == Vector3.zero) return;

        Matrix4x4 isometricMatrix = Matrix4x4.Rotate(Quaternion.Euler(0, -135, 0));
        Vector3 multipliedMatrix = isometricMatrix.MultiplyPoint3x4(_input);

        Quaternion rotation = Quaternion.LookRotation(multipliedMatrix, Vector3.up);
        transform.rotation = Quaternion.RotateTowards(transform.rotation, rotation, rotationSpeed * Time.deltaTime);
    }

    private void Move()
    {
        if (_characterController.isGrounded && _verticalVelocity < 0)
        {
            _verticalVelocity = -1.5f;
        }

        _verticalVelocity += gravity * fallMultiplier * Time.deltaTime;

        Vector3 moveDirection = transform.forward * _currentSpeed * _input.magnitude;
        moveDirection.y = _verticalVelocity;

        _characterController.Move(moveDirection * Time.deltaTime);
    }
    
    private void GetInput()
    {
        Vector2 input = _playerInputActions.Player.Move.ReadValue<Vector2>();
        _input = new Vector3(input.x, 0, input.y).normalized;

        Debug.Log($"Input: {_input}");
    }
}
