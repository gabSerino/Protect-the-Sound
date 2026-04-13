using UnityEngine;
using System.Collections;

public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float walkingSpeed = 5f;
    [SerializeField] private float rotationSpeed = 10f;
    [SerializeField] private float slideLerpSpeed = 10f;
    [SerializeField] private float acceleration = 25f;
    [SerializeField] private float deceleration = 30f;
    [SerializeField] private float airControl = 0.4f;

    [Header("Jump Settings")]
    [SerializeField] private float jumpForce = 5f;
    [SerializeField] private float gravityMultiplier = 1f;

    [Header("Attack Settings")]
    [SerializeField] private float attackRange = 2f;
    [SerializeField] private float attackDamage = 10f;
    [SerializeField] private float attackCooldown = 0.2f;
    [SerializeField] private float attackHitboxDuration = 0.3f;

    [Header("References")]
    [SerializeField] private PlayerInputManager playerInputManager;
    [SerializeField] private CharacterController characterController;
    [SerializeField] private Camera gameCamera;
    [SerializeField] private GameObject playerCapsule;
    [SerializeField] private GameObject attackHitbox;
    [SerializeField] private BeatManager beatManager;
    [SerializeField] private AudioClip attackSound;
    [SerializeField] private AudioSource audioSource;

    private Vector3 currentMovement;
    private float currentSpeed;
    private Vector3 cameraForward;
    private Vector3 movementDirection;
    private float lastAttackTime;
    private float bpm;

    // CONTROLLER FLAGS
    private bool canMove = true;
    private bool canAttack = true;
    private bool canJump = true;
    private bool attackPerformed = false;
    private bool isCheckingAttack = false;

    void Start()
    {
        cameraForward = gameCamera.transform.forward;
        cameraForward.y = 0;
        cameraForward.Normalize();
        bpm = beatManager._bpm;
        attackCooldown = 60f / (bpm * 2f); // finestra di 1/2 di battito
    }

    void Update()
    {
        HandleMovement();
        HandleRotation();
        HandleJump();
        HandleAttack();

        currentSpeed = walkingSpeed;
    }

    // =========================
    // CONTROLS ENABLE / DISABLE
    // =========================

    public void EnableAllControls()
    {
        canMove = true;
        canAttack = true;
        canJump = true;
    }

    public void DisableAllControls()
    {
        canMove = false;
        canAttack = false;
        canJump = false;
    }

    public void EnableMovement(bool value)
    {
        canMove = value;
    }

    public void EnableAttack(bool value)
    {
        canAttack = value;
    }

    public void EnableJump(bool value)
    {
        canJump = value;
    }

    // =========================
    // MOVEMENT
    // =========================

    private void HandleMovement()
    {
        Vector2 input = canMove ? playerInputManager.MoveInput : Vector2.zero;

        // Direzione relativa alla camera
        Vector3 desiredDirection = gameCamera.transform.forward * input.y + gameCamera.transform.right * input.x;
        desiredDirection.y = 0;
        desiredDirection.Normalize();

        float targetSpeed = walkingSpeed * input.magnitude;

        Vector3 horizontalVelocity = new Vector3(currentMovement.x, 0, currentMovement.z);

        float accel = characterController.isGrounded ? acceleration : acceleration * airControl;
        float decel = characterController.isGrounded ? deceleration : deceleration * airControl;

        if (input.magnitude > 0.1f)
        {
            // ACCELERAZIONE
            horizontalVelocity = Vector3.MoveTowards(
                horizontalVelocity,
                desiredDirection * targetSpeed,
                accel * Time.deltaTime
            );
        }
        else
        {
            // DECELERAZIONE
            horizontalVelocity = Vector3.MoveTowards(
                horizontalVelocity,
                Vector3.zero,
                decel * Time.deltaTime
            );
        }

        // Mantieni Y separata
        currentMovement.x = horizontalVelocity.x;
        currentMovement.z = horizontalVelocity.z;

        HandleGravity();

        characterController.Move(currentMovement * Time.deltaTime);

        // Aggiorna direzione per rotazione
        movementDirection = desiredDirection;
    }

    private void HandleRotation()
    {
        Vector3 targetDirection = movementDirection;
        targetDirection.y = 0;

        // Evita rotazioni inutili quando non c'è input
        if (targetDirection.sqrMagnitude < 0.01f)
            return;

        targetDirection.Normalize();

        Quaternion targetRotation = Quaternion.LookRotation(targetDirection);

        // Più reattivo quando sei fermo
        float currentRotSpeed = (new Vector3(currentMovement.x, 0, currentMovement.z).magnitude > 0.1f)
            ? rotationSpeed
            : rotationSpeed * 2f;

        // SNAP per cambi di direzione bruschi (tipo action games)
        float dot = Vector3.Dot(transform.forward, targetDirection);
        if (dot < 0.5f) // angolo ampio
        {
            transform.rotation = targetRotation;
        }
        else
        {
            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                targetRotation,
                currentRotSpeed * Time.deltaTime
            );
        }
    }

    // =========================
    // JUMP & GRAVITY
    // =========================

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

    private void HandleGravity()
    {
        if (!characterController.isGrounded)
        {
            currentMovement.y += Physics.gravity.y * gravityMultiplier * Time.deltaTime;
        }
    }

    // =========================
    // ATTACK
    // =========================

    private void HandleAttack()
    {
        if (!canAttack) return;

        if (playerInputManager.AttackInput && Time.time >= lastAttackTime + attackCooldown)
        {
            lastAttackTime = Time.time;
            StartCoroutine(AttackRoutine());
        }
    }

    /*private void HandleAttack()
    {
        if (!canAttack) return;

        if (playerInputManager.AttackInput && Time.time >= lastAttackTime + attackCooldown)
        {
            lastAttackTime = Time.time;
            attackPerformed = true;
        }
    }

    public void TriggerAttack()
    {
        if (!isCheckingAttack)
        {
            StartCoroutine(AttackInputWindow());
        }
    }

    private IEnumerator AttackInputWindow()
    {
        isCheckingAttack = true;

        float timer = 0f;

        while (timer < 60f/(bpm*4)) // finestra di 1/3 di battito
        {
            if (attackPerformed)
            {
                attackPerformed = false;
                StartCoroutine(AttackRoutine());
                break;
            }

            timer += Time.deltaTime;
            yield return null;
        }

        isCheckingAttack = false;
    }*/

    private IEnumerator AttackRoutine()
    {
        canAttack = false;
        canMove = false;

        PerformAttack();

        yield return new WaitForSeconds(attackHitboxDuration + attackCooldown);

        canAttack = true;
        canMove = true;
    }

    private void PerformAttack()
    {
        Debug.Log("Attacco eseguito!");

        Debug.DrawRay(transform.position, transform.forward * attackRange, Color.red, 0.2f);
        audioSource.PlayOneShot(attackSound);

        RaycastHit hit;
        if (Physics.Raycast(transform.position, transform.forward, out hit, attackRange))
        {
            Debug.Log("Colpito: " + hit.collider.name);

            /*var health = hit.collider.GetComponent<Health>();
            if (health != null)
            {
                health.TakeDamage(attackDamage);
            }*/
        }

        StartCoroutine(ActivateHitbox());
    }

    private IEnumerator ActivateHitbox()
    {
        if (attackHitbox == null) yield break;

        attackHitbox.SetActive(true);
        yield return new WaitForSeconds(attackHitboxDuration);
        attackHitbox.SetActive(false);
    }
}