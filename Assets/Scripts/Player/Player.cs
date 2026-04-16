using UnityEngine;
using System.Collections;

public class Player : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float walkingSpeed = 5f;
    [SerializeField] private float rotationSpeed = 10f;
    [SerializeField] private float acceleration = 25f;
    [SerializeField] private float deceleration = 30f;


    [Header("Attack Settings")]
    [SerializeField] private float attackRange = 1f;
    [SerializeField] private float attackDamage = 1f;
    [SerializeField] private float attackDuration = 0.15f;   // quanto dura il "commit" dell'attacco
    [SerializeField] private float attackCooldown = 0.3f;    // pausa prima del prossimo attacco
    [SerializeField] private float attackHitboxDuration = 0.1f;
    [SerializeField] private float attackMoveSpeedMultiplier = 0.4f; // rallenta invece di bloccare

    [Header("Rhythm Settings")]
    [SerializeField] private float bpm = 120f;
    [SerializeField] private bool useBpmCooldown = false;     // toggle dall'Inspector

    [Header("References")]
    [SerializeField] private PlayerInputManager playerInputManager;
    [SerializeField] private CharacterController characterController;
    [SerializeField] private Camera gameCamera;
    [SerializeField] private GameObject playerCapsule;
    [SerializeField] private GameObject attackHitbox;
    

    private Vector3 currentMovement;
    private Vector3 movementDirection;
    private HitboxDamage hitboxDamage;
    private Renderer hitboxRenderer; // Per debug, mostra il hitbox quando attivo
    private Collider hitboxCollider; // Per disabilitare il collider quando non attivo


    // CONTROLLER FLAGS
    private bool canMove = true;
    private bool canAttack = true;

    void Start()
    {
        hitboxDamage = attackHitbox.GetComponent<HitboxDamage>();
        hitboxCollider = attackHitbox.GetComponent<Collider>();
        hitboxRenderer = attackHitbox.GetComponent<Renderer>();
        hitboxCollider.enabled = false;
        hitboxRenderer.enabled = false;
        RefreshBpmCooldown();
    }

    public void SetBpm(float newBpm)
    {
        bpm = newBpm;
        RefreshBpmCooldown();
    }

    private void RefreshBpmCooldown()
    {
        if (useBpmCooldown)
            attackCooldown = 60f / (bpm * 2f);
    }

    void Update()
    {
        HandleMovement();
        HandleRotation();
        HandleAttack();

        
        attackHitbox.transform.position = playerCapsule.transform.position + playerCapsule.transform.forward * attackRange/2f;
        attackHitbox.transform.localScale = new Vector3(1f, 1f, attackRange);
    }

    // =========================
    // CONTROLS ENABLE / DISABLE
    // =========================

    public void EnableAllControls()
    {
        canMove = true;
        canAttack = true;
    }

    public void DisableAllControls()
    {
        canMove = false;
        canAttack = false;
    }

    public void EnableMovement(bool value)
    {
        canMove = value;
    }

    public void EnableAttack(bool value)
    {
        canAttack = value;
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

        float accel = acceleration;
        float decel = deceleration;

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

    /*private void HandleJump()
    {
        if (characterController.isGrounded && canJump)
        {
            currentMovement.y = -0.5f;

            if (playerInputManager.JumpInput)
            {
                currentMovement.y = jumpForce;
            }
        }
    }*/

    // =========================
    // ATTACK
    // =========================

    private void HandleAttack()
    {
        if (!canAttack) return;
        if (!playerInputManager.AttackInput) return;

        StartCoroutine(AttackRoutine());
    }

    private IEnumerator AttackRoutine()
    {
        canAttack = false;

        // Il giocatore si muove, ma più lentamente durante l'attacco
        float originalSpeed = walkingSpeed;
        walkingSpeed *= attackMoveSpeedMultiplier;
        bool isOnBeat = IsOnBeat(out float damageMultiplier);
        hitboxDamage.SetHitboxDamage(attackDamage*damageMultiplier);

        // Avvia l'attacco

        PerformAttack();

        yield return new WaitForSeconds(attackDuration);

        // Ripristina velocità, riabilita attacco dopo il cooldown
        walkingSpeed = originalSpeed;

        yield return new WaitForSeconds(attackCooldown);

        canAttack = true;
    }

    private void PerformAttack()
    {
        Debug.Log("Attacco eseguito!");
        StartCoroutine(ActivateAttackHitbox());
    }

    private IEnumerator ActivateAttackHitbox()
    {
        if (attackHitbox == null) yield break;

        hitboxCollider.enabled = true;
        hitboxRenderer.enabled = true;
        yield return new WaitForSeconds(attackHitboxDuration);
        hitboxCollider.enabled = false;
        hitboxRenderer.enabled = false;
        attackHitbox.SetActive(false);
        attackHitbox.SetActive(true); // resetta il collider per la prossima attivazione
    }

    bool IsOnBeat(out float damageMultiplier)
    {
        if (RhythmManager.Instance == null)
        {
            damageMultiplier = 0.5f;
            return false;
        }

        return RhythmManager.Instance.IsOnBeat(RhythmManager.Instance.perfectInputWindow, RhythmManager.Instance.goodInputWindow, out damageMultiplier);
    }
}