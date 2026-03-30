using UnityEngine;

public class PlayerControllerCifu : MonoBehaviour
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
        cameraForward.y = 0;

        // Se la camera guarda dritto in basso, il forward diventa zero. 
        // In quel caso usiamo il forward del player stesso.
        if (cameraForward.sqrMagnitude < 0.01f)
        {
            cameraForward = transform.forward;
        }

        movementDirection = cameraForward.normalized;
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
        Vector2 input = canMove ? playerInputManager.MoveInput : Vector2.zero;

        // 1. Calcoliamo la direzione desiderata basandoci sulla telecamera
        Vector3 moveInputDirection = gameCamera.transform.forward * input.y + gameCamera.transform.right * input.x;
        moveInputDirection.y = 0; // Appiattiamo il movimento sul piano XZ

        // 2. AGGIORNIAMO movementDirection solo se c'è effettivamente un input
        // Questo evita che diventi (0,0,0) e rompa la rotazione
        if (moveInputDirection.sqrMagnitude > 0.01f)
        {
            movementDirection = moveInputDirection.normalized;
        }

        // 3. Calcoliamo il target della velocità orizzontale
        // Se non c'è input (input.sqrMagnitude == 0), il target sarà Vector3.zero (il player si ferma col Lerp)
        Vector3 targetHorizontalVelocity = (input.sqrMagnitude > 0.01f) ? movementDirection * currentSpeed : Vector3.zero;

        // 4. Gestiamo la gravità prima del movimento
        HandleGravity();

        // 5. Uniamo la velocità orizzontale (fluida grazie al Lerp) con quella verticale (gravità/salto)
        Vector3 targetMovement = targetHorizontalVelocity;
        targetMovement.y = currentMovement.y;

        // Applichiamo l'interpolazione per rendere il movimento meno "scattoso"
        // Nota: usiamo una variabile temporanea per non sovrascrivere la Y della gravità durante il Lerp
        float savedVerticalVelocity = currentMovement.y;
        currentMovement = Vector3.Lerp(currentMovement, targetMovement, slideLerpSpeed * Time.deltaTime);
        currentMovement.y = savedVerticalVelocity;

        // 6. Muoviamo il controller
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
