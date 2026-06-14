using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    
    public float walkSpeed = 5f;
    public float sprintSpeed = 8f;
    public float crouchSpeed = 2.5f;

    
    public float maxStamina = 100f;
    public float currentStamina;
    public float staminaDrainRate = 20f;
    public float staminaRegenRate = 15f;
    private bool isExhausted = false;

    [Header("Jump & Gravity")]
    public float jumpHeight = 2f;
    public float gravityMultiplier = 2f;

    
    public Transform cameraRoot;          
    public float standingHeight = 2f;
    public float crouchHeight = 1f;
    public Vector3 standingCenter = new Vector3(0, 1f, 0);
    public Vector3 crouchCenter = new Vector3(0, 0.5f, 0);
    public float cameraStandingY = 1.6f;
    public float cameraCrouchY = 0.8f;

    private CharacterController characterController;
    private Vector2 moveInput;
    private Vector3 finalVelocity;

    private bool isJumpPressed;
    private bool isCrouching;
    private bool isSprintPressed;

    private void Start()
    {
        characterController = GetComponent<CharacterController>();
        currentStamina = maxStamina;

        
        if (cameraRoot != null)
        {
            cameraRoot.localPosition = new Vector3(0f, cameraStandingY, 0f);
        }
    }

    private void Update()
    {
        HandleStamina();
        CalculateLocalMovement();
        HandleCameraHeight();
    }

    public void OnMove(InputValue value) => moveInput = value.Get<Vector2>();
    public void OnJump(InputValue value) => isJumpPressed = value.isPressed;

    
    public void OnSprint(InputValue value)
    {
        if (value.isPressed)
        {
            isSprintPressed = !isSprintPressed;
        }
    }

    public void OnCrouch(InputValue value)
    {
        if (value.isPressed)
        {
            isCrouching = !isCrouching;

            if (isCrouching)
            {
                characterController.height = crouchHeight;
                characterController.center = crouchCenter;
            }
            else
            {
                characterController.height = standingHeight;
                characterController.center = standingCenter;
            }
        }
    }

    private void HandleStamina()
    {
        
        bool isMoving = moveInput.magnitude > 0;
        bool isMovingForward = moveInput.y > 0;

       
        if (!isMoving)
        {
            isSprintPressed = false;
        }

        
        if (isSprintPressed && isMovingForward && !isExhausted && !isCrouching)
        {
            currentStamina -= staminaDrainRate * Time.deltaTime;

            if (currentStamina <= 0)
            {
                currentStamina = 0;
                isExhausted = true;
                isSprintPressed = false;
            }
        }
        else
        {
            currentStamina += staminaRegenRate * Time.deltaTime;
            if (currentStamina > maxStamina) currentStamina = maxStamina;

            if (isExhausted && currentStamina >= (maxStamina * 0.2f))
            {
                isExhausted = false;
            }
        }
    }

    private void CalculateLocalMovement()
    {
        
        bool isRunning = isSprintPressed && !isExhausted && !isCrouching && moveInput.y > 0;

        float currentSpeed = walkSpeed;
        if (isCrouching) currentSpeed = crouchSpeed;
        else if (isRunning) currentSpeed = sprintSpeed;

        Vector3 direction = (Camera.main.transform.forward * moveInput.y) + (Camera.main.transform.right * moveInput.x);
        finalVelocity.x = direction.x * currentSpeed;
        finalVelocity.z = direction.z * currentSpeed;

        if (characterController.isGrounded)
        {
            if (finalVelocity.y < 0) finalVelocity.y = -2f;

            if (isJumpPressed && !isCrouching)
            {
                finalVelocity.y = Mathf.Sqrt(jumpHeight * -2f * (Physics.gravity.y * gravityMultiplier));
            }
            isJumpPressed = false;
        }
        else
        {
            finalVelocity.y += Physics.gravity.y * gravityMultiplier * Time.deltaTime;
        }

        characterController.Move(finalVelocity * Time.deltaTime);
    }

    private void HandleCameraHeight()
    {
        if (cameraRoot != null)
        {
            float targetY = isCrouching ? cameraCrouchY : cameraStandingY;
            Vector3 rootPos = cameraRoot.localPosition;

            rootPos.y = Mathf.Lerp(rootPos.y, targetY, Time.deltaTime * 10f);
            cameraRoot.localPosition = rootPos;
        }
    }
}