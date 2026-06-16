using UnityEngine;

public class CameraHeadBobbing : MonoBehaviour
{
    
    public float idleBobSpeed = 2f;
    public float idleBobAmount = 0.05f; 

    
    public float walkBobSpeed = 12f;
    public float walkBobAmount = 0.07f; 

    
    public float sprintBobSpeed = 18f;
    public float sprintBobAmount = 0.1f;

    
    public float crouchBobSpeed = 8f;
    public float crouchBobAmount = 0.02f;

    
    public CharacterController playerController;
    public PlayerMovement playerMovement;

    private float timer = 0f;

    private void Update()
    {
        if (playerController == null || playerMovement == null) return;

        Vector3 horizontalVelocity = new Vector3(playerController.velocity.x, 0, playerController.velocity.z);
        float currentSpeed = horizontalVelocity.magnitude;

        float currentBobSpeed = 0f;
        float currentBobAmount = 0f;

        
        if (currentSpeed < 0.1f)
        {
            currentBobSpeed = idleBobSpeed;
            currentBobAmount = idleBobAmount;
        }
        
        else if (currentSpeed > playerMovement.walkSpeed + 0.5f)
        {
            currentBobSpeed = sprintBobSpeed;
            currentBobAmount = sprintBobAmount;
        }
        
        else if (currentSpeed < playerMovement.walkSpeed - 0.5f)
        {
            currentBobSpeed = crouchBobSpeed;
            currentBobAmount = crouchBobAmount;
        }
        
        else
        {
            currentBobSpeed = walkBobSpeed;
            currentBobAmount = walkBobAmount;
        }

        
        timer += Time.deltaTime * currentBobSpeed;

        
        Vector3 targetPos = transform.localPosition;
        targetPos.y = Mathf.Sin(timer) * currentBobAmount;
        targetPos.x = 0f;
        targetPos.z = 0f;

        
        transform.localPosition = Vector3.Lerp(transform.localPosition, targetPos, Time.deltaTime * 10f);
    }
}