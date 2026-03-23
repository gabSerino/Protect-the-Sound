using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float walkingSpeed = 5f;
    [SerializeField] private float rotationSpeed = 10f;
    [SerializeField] private float slideLerpSpeed = 10f;
    [Header("Jump Settings")]
    [SerializeField] private float jumpForce = 5f;
    [SerializeField] private float gravityMultiplier = 1f;
    [Header("Attack Settings")]
    [SerializeField] private float attackRange = 2f;
    [SerializeField] private float attackDamage = 10f;
    [Header("References")]
    [SerializeField] private PlayerInputManager playerInputManager;
    [SerializeField] private CharacterController characterController;
    [SerializeField] private Camera gameCamera;
    [SerializeField] private GameObject playerCapsule;
    [SerializeField] private GameObject attackHitbox;

    private Vector3 currentMovement;
    private float currentSpeed;
    private Vector3 cameraForward;
    private Vector3 movementDirection;

    // CONTROLLER  FLAGS
    private bool canMove = true;
    private bool canAttack = true;
    private bool canJump = true;

    void Start()
    {
        cameraForward = gameCamera.transform.forward;
        movementDirection = cameraForward;
        movementDirection.y = 0;
        movementDirection.Normalize();
    }

    void Update()
    {
        HandleMovement();
        HandleRotation();
        HandleJump();
        currentSpeed = walkingSpeed; //HandleSpeed();
        HandleAttack();
    }
    private void HandleJump()
    {
        if (characterController.isGrounded && canJump)
        {
            currentMovement.y = -0.5f;
            if (playerInputManager.JumpInput)
            {
                currentMovement.y = jumpForce;
            }
        }
    }

    /*private void HandleSpeed()
    {
        if (playerInputManager.SprintInput)
        {
            currentSpeed = walkingSpeed * 2;
        }
        else
        {
            currentSpeed = walkingSpeed;
        }
    }*/
    private void HandleAttack()
    {
        
        if (playerInputManager.AttackInput && canAttack)
        {
            Debug.DrawRay(transform.position, transform.forward * attackRange, Color.red);
            RaycastHit hit;
            if (Physics.Raycast(transform.position, transform.forward, out hit, attackRange))
            {
                // Implement damage logic here, e.g., hit.collider.GetComponent<Health>().TakeDamage(attackDamage);
            }
        }
        else
        {
            Debug.DrawRay(transform.position, transform.forward * attackRange, Color.green);
            // Handle attack cooldown or reset logic if needed
        }
    }
    private void HandleGravity()
    {
        if (!characterController.isGrounded)
        {
            currentMovement.y += Physics.gravity.y * gravityMultiplier * Time.deltaTime;
        }
    }

    private void HandleMovement()
    {
        Vector2 input;
        if(canMove)
        {
            input = playerInputManager.MoveInput;
        }
        else
        {
            input = Vector2.zero;
        }
        movementDirection = cameraForward * input.y + gameCamera.transform.right * input.x;
        movementDirection.y = 0;
        movementDirection.Normalize();
        
        Vector3 targetMovement = movementDirection * currentSpeed;
        targetMovement.y = currentMovement.y;
        currentMovement = Vector3.Lerp(currentMovement, targetMovement, slideLerpSpeed * Time.deltaTime);
        HandleGravity();
        characterController.Move(currentMovement * Time.deltaTime);
    }

    private void HandleRotation()
    {
        Vector3 targetDirection = movementDirection;
        targetDirection.y = 0;
        targetDirection.Normalize();
        Quaternion targetRotation = Quaternion.LookRotation(targetDirection, Vector3.up);
        if(targetDirection.sqrMagnitude > 0.01f)
        {
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }
    }
}
