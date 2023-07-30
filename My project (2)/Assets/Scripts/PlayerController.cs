using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using DG.Tweening;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Animations.Rigging;
using UnityEngine.InputSystem;
public class PlayerController : NetworkBehaviour 
{
    #region Fields
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private LayerMask environmentMask;
    [SerializeField] private Rig aimRig;
    [SerializeField] private float deltaTimeMultiplier =   1.0f ;
    [SerializeField] private float blendDampTime=0.1f;
    [SerializeField] private Transform cineMachineCameraTarget;
    [SerializeField] private float cameraAngleOverride = 0.0f;
    [SerializeField] private CinemachineVirtualCamera defaultVirtualCamera;
    [SerializeField] private CinemachineVirtualCamera aimCamera;
    [SerializeField] [Range(0f, 2000f)] private float cameraSmoothDamp;
    [SerializeField] private bool _isAiming;
    [SerializeField] private ParticleSystem muzzleParticle;
    [SerializeField] private Transform Gun;
    [SerializeField] private Vector2 _screenCenter;
    private Vector3 _moveDirection;
    private Vector3 _rawDirection;
    [SerializeField]private CharacterController _characterController;
    [SerializeField]private Animator _animator;
    private bool _isGrounded;
    private float _fallVelocity = 0f;
    private float _gravity = -15;
    private const float TerminalVelocity = -50f;
    private Vector2 _look;
    private float _cinemachineTargetYaw;
    private float _cinemachineTargetPitch;
    private float _aimRigWeight;
    [SerializeField]private Camera _camera;
    private static readonly int XAxisParameter = Animator.StringToHash("Xaxis");
    private static readonly int YAxisParameter = Animator.StringToHash("Yaxis");
    private static readonly int IsAimingAnimation = Animator.StringToHash("IsAiming");
    private static readonly int IsShooting = Animator.StringToHash("IsShooting");
    private static readonly int IsSprinting = Animator.StringToHash("IsSprinting");
    public bool isShooting;
    public bool isSprinting;
    private Coroutine _adjustAimRigWeightShooting;
    private Coroutine _startShootingCoroutine;
    private float _lastShootTime;
    [SerializeField]private RigBuilder _rigBuilder;
    [SerializeField] private PlayerInput _input;
    private NetworkVariable<float> syncAimRigWeight = new (default,NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    [SerializeField] public Transform rigTargetObject;
    
    #endregion
    
    #region MonoBehavior
    private void Awake()
    {
        _cinemachineTargetYaw = cineMachineCameraTarget.transform.rotation.eulerAngles.y;
        aimRig.weight = 0f;
        if (_camera == null)
        {
            _camera = Camera.main;
        }
        CinemachineVirtualCamera[] virtualCameras = FindObjectsOfType<CinemachineVirtualCamera>();
        foreach (CinemachineVirtualCamera virtualCamera in virtualCameras)
        {
            if (virtualCamera.name == "DefaultCam")
            {
                defaultVirtualCamera = virtualCamera;
            }
            else if (virtualCamera.name == "AimCAM")
            {
                aimCamera = virtualCamera;
            }
        }
        syncAimRigWeight.OnValueChanged += OnRigWeightChanged;
        aimCamera.gameObject.SetActive(false);
        _rigBuilder.Build();
    }
    private void OnRigWeightChanged(float oldValue, float newValue)
    {
        _aimRigWeight = newValue;
        aimRig.weight = newValue;
    }
    
    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        if (IsOwner && IsClient)
        {
            Debug.Log("on network spawn inside");
            _input = GetComponent<PlayerInput>();
            _input.enabled = true;
            defaultVirtualCamera.Follow = cineMachineCameraTarget;
            defaultVirtualCamera.LookAt = cineMachineCameraTarget;
            aimCamera.Follow = cineMachineCameraTarget;
            aimCamera.LookAt = cineMachineCameraTarget;
        }
        
    }
    private void Update()
    {
        if (IsOwner)
        {
            _screenCenter = new Vector2(Screen.width / 2f, Screen.height / 2f);
            Move();
            CheckGround();
            LerpRig();
            CameraRotation();
            syncAimRigWeight.Value = _aimRigWeight;
        }
    }

    #endregion
    
    #region MainMethods
    
    private void Move()
    {
        Vector3 cameraForward = _camera.transform.forward;
        cameraForward.y = 0f;
        Quaternion cameraRotation = Quaternion.LookRotation(cameraForward);
        Quaternion targetRotation = Quaternion.LookRotation(cameraForward);
        transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, Time.deltaTime * cameraSmoothDamp);
        if (isSprinting)
        {
            Vector3 sprintDirection = cameraForward;
            sprintDirection.y = 0f;
            _characterController.Move(sprintDirection.normalized * (8.5f * Time.deltaTime));
        }
        else
        {
            Vector3 direction = cameraRotation * Vector3.forward * _rawDirection.z + cameraRotation * Vector3.right * _rawDirection.x;
            direction.y = 0f;
            
            _animator.SetFloat(XAxisParameter, _moveDirection.x, blendDampTime, Time.deltaTime);
            _animator.SetFloat(YAxisParameter, _moveDirection.y, blendDampTime, Time.deltaTime);
            _characterController.Move(direction.normalized * (moveSpeed * Time.deltaTime));
        }
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
    private void Shoot()
    {
        GunKnockBack();
        if (_aimRigWeight <= 0.1f)
        {
            _aimRigWeight = 1f;
        }
        syncAimRigWeight.Value = _aimRigWeight;
        Ray ray = _camera.ScreenPointToRay(_screenCenter);
        if (Physics.Raycast(ray, out var hit,100f,environmentMask))
        {
            Vector3 hitPoint = hit.point;
            Debug.Log(hitPoint);
        }
    }
    private void GunKnockBack()
    {
        Gun.DOShakePosition(0.06f, 0.02f, 1, 1f, false,false);
    }
    private void CameraRotation()
    {
        if (_look.sqrMagnitude >= 0.01f )
        {
            _cinemachineTargetYaw += _look.x * deltaTimeMultiplier;
            _cinemachineTargetPitch += _look.y * deltaTimeMultiplier;
        }
        _cinemachineTargetYaw = ClampAngle(_cinemachineTargetYaw, float.MinValue, float.MaxValue);
        _cinemachineTargetPitch = ClampAngle(_cinemachineTargetPitch, -70.0f, 85.0f);
        cineMachineCameraTarget.transform.rotation = Quaternion.Euler(_cinemachineTargetPitch + cameraAngleOverride,
            _cinemachineTargetYaw, 0.0f);
    }
    private void LerpRig()
    {
        aimRig.weight = Mathf.Lerp(aimRig.weight, _aimRigWeight, Time.deltaTime * 20); 
    }
    private static float ClampAngle(float lfAngle, float lfMin, float lfMax)
    {
        if (lfAngle < -360f) lfAngle += 360f;
        if (lfAngle > 360f) lfAngle -= 360f;
        return Mathf.Clamp(lfAngle, lfMin, lfMax);
    }
    #endregion
    
    #region Input
    public void OnMove(InputAction.CallbackContext context)
    {
        _moveDirection = context.ReadValue<Vector2>();
        _rawDirection = new Vector3(_moveDirection.x, 0, _moveDirection.y);
    }
    public void OnAim(InputAction.CallbackContext context)
    {
        if (!_isAiming&&!isSprinting)
        {
            _isAiming = true;
            moveSpeed = 2f;
            _animator.SetBool(IsAimingAnimation,true);
            defaultVirtualCamera.gameObject.SetActive(false);
            aimCamera.gameObject.SetActive(true);
            _aimRigWeight = 1f;
        }
        else
        {
            moveSpeed = 5f;
            _animator.SetBool(IsAimingAnimation,false);
            defaultVirtualCamera.gameObject.SetActive(true);
            aimCamera.gameObject.SetActive(false);
            _isAiming = false;
            _aimRigWeight = 0f;
        }

        syncAimRigWeight.Value = _aimRigWeight;
    }
    public void OnShoot(InputAction.CallbackContext context)
    {
        if (context.started&&!isSprinting&& Time.time - _lastShootTime >= 0.1f)
        {
            _startShootingCoroutine = StartCoroutine(ShootingCoroutine());
            isShooting = true;
            _animator.SetBool(IsShooting,true);
            _lastShootTime = Time.time;
        }
        if (context.canceled)
        {
            isShooting = false;
            _animator.SetBool(IsShooting,false);
           
            StopCoroutine(_startShootingCoroutine);
            if (!_isAiming)
            {
                _adjustAimRigWeightShooting = StartCoroutine(DecreaseRigWeight(1f));
            }
        }
    }
    public void OnLook(InputAction.CallbackContext context)
    {
        LookInput(context.ReadValue<Vector2>());
    }
    private void LookInput(Vector2 newLookDirection)
    {
        _look = newLookDirection;
    }
    public void OnSprint(InputAction.CallbackContext context)
    {
        
        if (context.started&&!_isAiming&&!isShooting)
        {
            isSprinting = true;
            _aimRigWeight = 0f;
            _animator.SetBool(IsSprinting,true);
            _rawDirection.x = 0;
            _rawDirection.z = Mathf.Clamp(_rawDirection.z, 0f, 1f);
        }
        
        if (context.canceled)
        {
            isSprinting = false; 
            _animator.SetBool(IsSprinting,false);
            _rawDirection = new Vector3(_moveDirection.x, 0, _moveDirection.y);
        }
        syncAimRigWeight.Value = _aimRigWeight;
        
    }
    #endregion

    #region  Ienum
    IEnumerator DecreaseRigWeight(float time)
    {
        float elapsedTime = 0f;
        while (elapsedTime < time)
        {
            if (Input.GetMouseButtonDown(0))
            {
                elapsedTime = 0f;
                yield return null;
            }
            else
            {
                elapsedTime += Time.deltaTime;
                yield return null;
            }
        }

        if (_isAiming||isShooting)
        {
            yield break;
        }
        _aimRigWeight = 0f;
        syncAimRigWeight.Value = _aimRigWeight;
    }
    IEnumerator ShootingCoroutine()
    {
        while (true)
        {
            Shoot();
            muzzleParticle.Play();
            yield return new WaitForSeconds(0.1f);
        }
    }
    
    #endregion
}