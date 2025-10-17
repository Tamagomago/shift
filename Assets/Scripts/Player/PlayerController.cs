using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    private InputSystem_Actions _playerInputActions;
    private CharacterController _characterController;
    [SerializeField] private float maxSpeed = 10f;
    [SerializeField] private float rotationSpeed = 360f;
    [SerializeField] private float acceleration = 5f;
    [SerializeField] private float deceleration = 10f;

    private float _currentSpeed;
    private Vector3 _input;
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
    }
    private void OnDisable()
    {
        _playerInputActions.Player.Disable();
    }

    private void Update()
    {
        GetInput();
        Look();
        CalculateSpeed();
        Move();
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
        if (_input == Vector3.zero) return;

        Matrix4x4 isometricMatrix = Matrix4x4.Rotate(Quaternion.Euler(0, 45, 0));
        Vector3 multipliedMatrix = isometricMatrix.MultiplyPoint3x4(_input);

        Quaternion rotation = Quaternion.LookRotation(multipliedMatrix, Vector3.up);
        transform.rotation = Quaternion.RotateTowards(transform.rotation, rotation, rotationSpeed * Time.deltaTime);
    }
    
    private void Move()
    {
        Vector3 moveDirection = transform.forward * _currentSpeed * _input.magnitude * Time.deltaTime;

        _characterController.Move(moveDirection);
    }
    private void GetInput()
    {
        Vector2 input = _playerInputActions.Player.Move.ReadValue<Vector2>();
        _input = new Vector3(input.x, 0, input.y);

        Debug.Log($"Input: {_input}");
    }
}
