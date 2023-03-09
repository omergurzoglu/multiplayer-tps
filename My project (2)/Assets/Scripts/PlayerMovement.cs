
using Cinemachine;
using UnityEngine;
using UnityEngine.Animations.Rigging;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private LayerMask environmentMask;
    [SerializeField] private float rigSwapSmooth;
    private Vector3 _moveDirection;
    private Vector3 _rawDirection;
    private CharacterController _characterController;
    private Animator _animator;
    private bool _isGrounded;
    private float _fallVelocity = 0f;
    private float _gravity = -9.81f;
    private const float TerminalVelocity = -50f;
    private bool _isAiming;
    private Vector2 _look;
    private float _cinemachineTargetYaw;
    private float _cinemachineTargetPitch;
    private float _aimRigWeight;

    [SerializeField] private Rig aimRig;
    

    private Camera _camera;
    [SerializeField]private float deltaTimeMultiplier =   1.0f ;
    [SerializeField] private float blendDampTime=0.1f;
    [SerializeField] private Transform cineMachineCameraTarget;
    [SerializeField]private float cameraAngleOverride = 0.0f;
    [SerializeField] private Transform targetSphere;

    [SerializeField] private CinemachineVirtualCamera virtualCamera;
    [SerializeField] private CinemachineVirtualCamera aimCamera;
    [SerializeField] [Range(0f, 720f)] private float cameraSmoothDamp;
    
    private static readonly int XAxisParameter = Animator.StringToHash("Xaxis");
    private static readonly int YAxisParameter = Animator.StringToHash("Yaxis");
    private static readonly int IsAimingAnimation = Animator.StringToHash("IsAiming");

    private Vector2 screenCenter;

    private void Awake()

    {
        _camera = Camera.main;
        _animator = GetComponent<Animator>();
        _characterController = GetComponent<CharacterController>();
        _cinemachineTargetYaw = cineMachineCameraTarget.transform.rotation.eulerAngles.y;
        aimRig.weight = 0f;
    }

    private void Update()
    {
        screenCenter = new Vector2(Screen.width / 2f, Screen.height / 2f);
        TargetGizmo();
        Move();
        CheckGround();
        aimRig.weight = Mathf.Lerp(aimRig.weight, _aimRigWeight, Time.deltaTime * 20);
    }

   
    private void LateUpdate()
    {
        CameraRotation();
    }
    
    private void Move()
    {
        Vector3 cameraForward = _camera.transform.forward;
        cameraForward.y = 0f;
        Quaternion cameraRotation = Quaternion.LookRotation(cameraForward);
        
        Vector3 direction = cameraRotation * Vector3.forward * _rawDirection.z + cameraRotation * Vector3.right * _rawDirection.x;
        direction.y = 0f;
        
        Quaternion targetRotation = Quaternion.LookRotation(cameraForward);
        transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, Time.deltaTime * cameraSmoothDamp);
        
        _animator.SetFloat(XAxisParameter, _moveDirection.x,blendDampTime,Time.deltaTime);
        _animator.SetFloat(YAxisParameter, _moveDirection.y,blendDampTime,Time.deltaTime);

        _characterController.Move(direction.normalized * (moveSpeed * Time.deltaTime));
    }


    private void CheckGround()
    {
        _isGrounded = _characterController.isGrounded;
        if (!_isGrounded)
        {
            _fallVelocity += _gravity * Time.deltaTime;
            _fallVelocity = Mathf.Clamp(_fallVelocity, TerminalVelocity, Mathf.Infinity);
            Vector3 fallDirection = Vector3.up * (_fallVelocity * Time.deltaTime);
            _characterController.Move(fallDirection);
        }
        else
        {
            _fallVelocity = 0f;
        }
    }

    public void OnMove(InputAction.CallbackContext context)
    {
        
        _moveDirection = context.ReadValue<Vector2>();
        _rawDirection = new Vector3(_moveDirection.x, 0, _moveDirection.y);
    }

    public void OnAim(InputAction.CallbackContext context)
    {
        
        if (!_isAiming)
        {
            _isAiming = true;
            moveSpeed = 2f;
            _animator.SetBool(IsAimingAnimation,true);
            virtualCamera.gameObject.SetActive(false);
            aimCamera.gameObject.SetActive(true);
            _aimRigWeight = 1f;


        }
        else
        {
            moveSpeed = 5f;
            _animator.SetBool(IsAimingAnimation,false);
            virtualCamera.gameObject.SetActive(true);
            aimCamera.gameObject.SetActive(false);
            _isAiming = false;
            _aimRigWeight = 0f;

        }
    }

    public void OnShoot(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            Shoot();
        }
    }

    public void OnLook(InputAction.CallbackContext context)
    {
        LookInput(context.ReadValue<Vector2>());
    }
    public void LookInput(Vector2 newLookDirection)
    {
        _look = newLookDirection;
    }
    private void Shoot()
    {
        if (_isAiming)
        {
            
            Ray ray = _camera.ScreenPointToRay(screenCenter);
        
            if (Physics.Raycast(ray, out var hit,100f,environmentMask))
            {
                Vector3 hitPoint = hit.point;
                Debug.Log(hitPoint);
            }
        }
        
    }

    

    private void TargetGizmo()
    {
        Ray ray = _camera.ScreenPointToRay(screenCenter);
        
        if (Physics.Raycast(ray, out var hit,100f,environmentMask))
        {
            targetSphere.position = hit.point;
           
        }
    }
    
    private void CameraRotation()
    {
        
        if (_look.sqrMagnitude >= 0.01f )
        {
            
            _cinemachineTargetYaw += _look.x * deltaTimeMultiplier;
            _cinemachineTargetPitch += _look.y * deltaTimeMultiplier;
        }
        
        _cinemachineTargetYaw = ClampAngle(_cinemachineTargetYaw, float.MinValue, float.MaxValue);
        _cinemachineTargetPitch = ClampAngle(_cinemachineTargetPitch, -30.0f, 70.0f);
        
        cineMachineCameraTarget.transform.rotation = Quaternion.Euler(_cinemachineTargetPitch + cameraAngleOverride,
            _cinemachineTargetYaw, 0.0f);
    }
    private static float ClampAngle(float lfAngle, float lfMin, float lfMax)
    {
        if (lfAngle < -360f) lfAngle += 360f;
        if (lfAngle > 360f) lfAngle -= 360f;
        return Mathf.Clamp(lfAngle, lfMin, lfMax);
    }
}