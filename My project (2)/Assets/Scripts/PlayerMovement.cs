
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField]private PlayerInputActions1 _playerInput;
    [SerializeField] private float moveSpeed = 5f;
    private Vector3 _moveDirection;
    private Vector3 _rawDirection;
    private CharacterController _characterController;
    

    private void Awake()
    {
        _characterController = GetComponent<CharacterController>();
        _playerInput = new PlayerInputActions1();
    }

    private void Update()
    {
        _characterController.Move(_rawDirection * (moveSpeed *Time.deltaTime));
    }

    public void OnMove(InputAction.CallbackContext context)
    {
        _moveDirection = context.ReadValue<Vector2>(); 
        _rawDirection = new Vector3(_moveDirection.x, 0, _moveDirection.y);
    }

   
}