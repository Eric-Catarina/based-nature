
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(PlayerInput))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float rotationSpeed = 720f; 
    [SerializeField] private float groundCheckDistance = 0.2f;
    [SerializeField] private LayerMask groundLayer;

    [Header("Animation")]
    [SerializeField] private Animator playerAnimator;
    private readonly int _isMovingHash = Animator.StringToHash("IsMoving");
    private readonly int _velocityMagnitudeHash = Animator.StringToHash("VelocityMagnitude");


    private Rigidbody _rigidbody;
    private PlayerControls _playerControls;
    private Vector2 _moveInput;
    private Transform _cameraMainTransform;
    private bool _isGrounded;

    private void Awake()
    {
        _rigidbody = GetComponent<Rigidbody>();
        _playerControls = new PlayerControls();

        if (Camera.main != null)
        {
            _cameraMainTransform = Camera.main.transform;
        }
        else
        {
            Debug.LogError("Main Camera not found. Player movement might not work as expected relative to camera.");
        }

        _rigidbody.freezeRotation = true; 

        
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        
    }

    private void OnEnable()
    {
        _playerControls.Gameplay.Enable();
        _playerControls.Gameplay.Move.performed += OnMoveInput;
        _playerControls.Gameplay.Move.canceled += OnMoveInput;
    }

    private void OnDisable()
    {
        _playerControls.Gameplay.Move.performed -= OnMoveInput;
        _playerControls.Gameplay.Move.canceled -= OnMoveInput;
        _playerControls.Gameplay.Disable();
    }

    private void OnMoveInput(InputAction.CallbackContext context)
    {
        _moveInput = context.ReadValue<Vector2>();
    }

    private void Update()
    {
        UpdateAnimator();
    }

    private void FixedUpdate()
    {
        PerformGroundCheck();
        HandleMovement();
        HandleRotation();
    }

    private void PerformGroundCheck()
    {
        Vector3 rayOrigin = transform.position + Vector3.up * 0.1f;
        _isGrounded = Physics.Raycast(rayOrigin, Vector3.down, groundCheckDistance, groundLayer);
        Debug.DrawRay(rayOrigin, Vector3.down * groundCheckDistance, _isGrounded ? Color.green : Color.red);
    }

    private void HandleMovement()
    {
        if (_cameraMainTransform == null) return;

        Vector3 forward = _cameraMainTransform.forward;
        Vector3 right = _cameraMainTransform.right;

        forward.y = 0f;
        right.y = 0f;

        forward.Normalize();
        right.Normalize();

        Vector3 desiredMoveDirection = (forward * _moveInput.y + right * _moveInput.x).normalized;
        Vector3 targetVelocity = desiredMoveDirection * moveSpeed;

        if (_isGrounded)
        {
            _rigidbody.linearVelocity = new Vector3(targetVelocity.x, _rigidbody.linearVelocity.y, targetVelocity.z);
        }
        else
        {
             
            _rigidbody.linearVelocity = new Vector3(targetVelocity.x * 0.8f, _rigidbody.linearVelocity.y, targetVelocity.z * 0.8f);
        }
    }

    private void HandleRotation()
    {
        if (_moveInput == Vector2.zero) return;
        if (_cameraMainTransform == null) return;


        Vector3 forward = _cameraMainTransform.forward;
        Vector3 right = _cameraMainTransform.right;
        forward.y = 0;
        right.y = 0;
        forward.Normalize();
        right.Normalize();

        Vector3 desiredMoveDirection = (forward * _moveInput.y + right * _moveInput.x).normalized;


        if (desiredMoveDirection != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(desiredMoveDirection, Vector3.up);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, rotationSpeed * Time.fixedDeltaTime);
        }
    }

    private void UpdateAnimator()
    {
        if (playerAnimator == null) return;

        float velocityMagnitude = new Vector3(_rigidbody.linearVelocity.x, 0, _rigidbody.linearVelocity.z).magnitude / moveSpeed;
        velocityMagnitude = Mathf.Clamp01(velocityMagnitude);

        playerAnimator.SetBool(_isMovingHash, velocityMagnitude > 0.01f);
        playerAnimator.SetFloat(_velocityMagnitudeHash, velocityMagnitude);
    }
}