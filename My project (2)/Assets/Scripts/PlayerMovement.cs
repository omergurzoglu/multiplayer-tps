
using Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 5f;
    private Vector3 _moveDirection;
    private Vector3 _rawDirection;
    private CharacterController _characterController;
    private Animator _animator;
    private bool _isGrounded;
    private float _fallVelocity = 0f;
    private float _gravity = -9.81f;
    private const float TerminalVelocity = -50f;
    
    [SerializeField]private CinemachineVirtualCamera virtualCamera;
    [SerializeField][Range(0f,720f)] private float cameraSmoothDamp;
    private static readonly int IsWalking = Animator.StringToHash("IsWalking");
    
    private void Awake()

    {
        _animator = GetComponent<Animator>();
        _characterController = GetComponent<CharacterController>();
    }

    private void Update()
    {
        Move();
        CheckGround();
    }

    private void Move()
    {
        Vector3 cameraForward = virtualCamera.transform.forward;
        cameraForward.y = 0f;
        Quaternion cameraRotation = Quaternion.LookRotation(cameraForward);
        
        if (_rawDirection.magnitude > 0.1f) {
            Quaternion targetRotation = Quaternion.LookRotation(cameraRotation * _rawDirection);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, Time.deltaTime * cameraSmoothDamp);
        }
        Vector3 direction = cameraRotation * Vector3.forward * _rawDirection.z + cameraRotation * Vector3.right * _rawDirection.x;
        direction.y = 0f;
        _characterController.Move(direction.normalized * (moveSpeed * Time.deltaTime));
        
        
     
        
    }

    private void CheckGround()
    {
        _isGrounded = _characterController.isGrounded;
        if (!_isGrounded)
        {
            // Update the fall velocity using gravity
            _fallVelocity += _gravity * Time.deltaTime;
            _fallVelocity = Mathf.Clamp(_fallVelocity, TerminalVelocity, Mathf.Infinity);

            // Move the character controller in the direction of the fall
            Vector3 fallDirection = Vector3.up * (_fallVelocity * Time.deltaTime);
            _characterController.Move(fallDirection);
        }
        else
        {
            // Reset the fall velocity when grounded
            _fallVelocity = 0f;
        }
    }

    public void OnMove(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            _animator.SetBool(IsWalking,true);
        }
        _moveDirection = context.ReadValue<Vector2>(); 
        _rawDirection = new Vector3(_moveDirection.x, 0, _moveDirection.y);
        if (context.canceled)
        {
            _animator.SetBool(IsWalking,false);
        }
    }

   
}