using UnityEngine;
using UnityEngine.InputSystem; 

public class PlayerMovement : MonoBehaviour
{
    
    public float moveSpeed = 5f;

    
    private CharacterController characterController;
    private Vector2 moveInput;
    private Vector3 finalVelocity;

    private void Start()
    {
        
        characterController = GetComponent<CharacterController>();
    }

    private void Update()
    {
        CalculateLocalMovement();
    }

    
    public void OnMove(InputValue value)
    {
        moveInput = value.Get<Vector2>();
    }

    private void CalculateLocalMovement()
    {
        
        Vector3 direction = (transform.forward * moveInput.y) + (transform.right * moveInput.x);

        
        finalVelocity.x = direction.x * moveSpeed;
        finalVelocity.z = direction.z * moveSpeed;

        
        finalVelocity.y = Physics.gravity.y;

        
        characterController.Move(finalVelocity * Time.deltaTime);
    }
}