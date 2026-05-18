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
    [SerializeField] private float attackMoveSpeedMultiplier = 0.65f; // rallenta invece di bloccare
    [SerializeField] private float attackMoveEaseTime = 0.1f; // tempo per ridurre/riportare la velocità gradualmente

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
    private bool isAttacking = false;   // blocca rotazione durante attacco e cooldown

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
        // Lock durante attacco e cooldown
        if (isAttacking) return;

        Vector3 rawDirection = movementDirection;
        rawDirection.y = 0;

        if (rawDirection.sqrMagnitude < 0.01f)
            return;

        // Snap alle 8 direzioni cardinali/ordinali
        Vector3 snappedDirection = SnapTo8Directions(rawDirection);

        Quaternion targetRotation = Quaternion.LookRotation(snappedDirection);

        // Con lo snap non serve più lo Slerp graduato né il dot-check:
        // la rotazione è sempre un salto netto a uno degli 8 angoli
        transform.rotation = targetRotation;
    }

    private Vector3 SnapTo8Directions(Vector3 direction)
    {
        float angle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg;
        float snapped = Mathf.Round(angle / 45f) * 45f;
        float rad = snapped * Mathf.Deg2Rad;
        return new Vector3(Mathf.Sin(rad), 0f, Mathf.Cos(rad));
    }

    private IEnumerator AttackRoutine()
    {
        canAttack = false;
        isAttacking = true;   // blocca rotazione

        float originalSpeed = walkingSpeed;

        walkingSpeed *= attackMoveSpeedMultiplier;

        bool isOnBeat = IsOnBeat(out float damageMultiplier);

        hitboxDamage.SetHitboxDamage(attackDamage * damageMultiplier, damageMultiplier);

        hitboxCollider.enabled = true;
        hitboxRenderer.enabled = true;

        yield return new WaitForSeconds(attackDuration);

        hitboxCollider.enabled = false;
        hitboxRenderer.enabled = false;

        attackHitbox.SetActive(false);
        attackHitbox.SetActive(true);

        walkingSpeed = originalSpeed;

        yield return new WaitForSeconds(attackCooldown);

        isAttacking = false;  // sblocca rotazione
        canAttack = true;
    }

    private void HandleAttack()
    {
        if (!canAttack) return;
        if (!playerInputManager.AttackInput) return;

        Debug.Log("Attacco eseguito!");
        StartCoroutine(AttackRoutine());
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