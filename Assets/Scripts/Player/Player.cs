using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System;

public class Player : MonoBehaviour
{
    // =========================
    // SERIALIZED FIELDS
    // =========================

    [Header("Movement Settings")]
    [SerializeField] private float walkingSpeed = 8f;
    [SerializeField] private float rotationSpeed = 10f;
    [SerializeField] private float acceleration = 100f;
    [SerializeField] private float deceleration = 200f;

    [Header("Attack Settings")]
    [SerializeField] private AttackType attackType = AttackType.DEFAULT;
    [SerializeField] private float attackRange = 2f;
    [SerializeField] private float attackDamage = 15f;
    [SerializeField] private float attackBoxDuration = 0.15f;       // quanto dura il "commit" dell'attacco
    [SerializeField] private float attackTime = 0.3f;              // pausa prima del prossimo attacco
    [SerializeField] private float attackMoveSpeedMultiplier = 0.5f; // rallenta invece di bloccare

    [Header("Rhythm Settings")]
    [SerializeField] private float bpm = 120f;
    [SerializeField] private bool useBpmCooldown = false;           // toggle dall'Inspector

    [Header("Health Settings")]
    private float maxHealthPoints = 100f;

    [Header("Music Points Settings")]
    private float maxMusicPoints = 100f;

    [Header("Invulnerability Settings")]
    public float invulnerabilityDuration = 2f;
    public float flickerInterval = 0.1f;

    [Header("UI")]
    public Slider healthSlider;
    public UIJuice uiJuice;

    [Header("References")]
    [SerializeField] private PlayerInputManager playerInputManager;
    [SerializeField] private CharacterController characterController;
    [SerializeField] private Camera gameCamera;
    [SerializeField] private GameObject playerCapsule;
    [SerializeField] private GameObject attackHitbox;

    // DEFAULT STATS
    private const float DEFAULT_WALKING_SPEED = 8f;
    private const float DEFAULT_ROTATION_SPEED = 10f;
    private const float DEFAULT_ACCELERATION = 100f;
    private const float DEFAULT_DECELERATION = 200f;
    private const float DEFAULT_ATTACK_RANGE = 2f;
    private const float DEFAULT_ATTACK_DAMAGE = 15f;
    private const float DEFAULT_ATTACK_BOX_DURATION = 0.15f;
    private const float DEFAULT_ATTACK_TIME = 0.3f;
    private const float DEFAULT_ATTACK_MOVE_SPEED_MULTIPLIER = 0.5f;
    private const float DEFAULT_INVULNERABILITY_DURATION = 2f;
    private const float DEFAULT_FLICKER_INTERVAL = 0.1f;

    private const float DEFAULT_MAX_HEALTH_POINTS = 100f;
    private const float DEFAULT_MAX_MUSIC_POINTS = 100f;


    // =========================
    // PROPERTIES
    // =========================

    public float currentHealthPoints { get; private set; }

    public float currentMusicPoints { get; private set; }



    // =========================
    // PRIVATE FIELDS
    // =========================

    // Movement
    private Vector3 currentMovement;
    private Vector3 movementDirection;

    // Attack / Hitbox
    private HitboxDamage hitboxDamage;
    private Renderer hitboxRenderer;   // Per debug, mostra il hitbox quando attivo
    private Collider hitboxCollider;   // Per disabilitare il collider quando non attivo

    // Controller flags
    private bool canMove = true;
    private bool canAttack = true;
    private bool isAttacking = false;  // blocca rotazione durante attacco e cooldown

    // Health / Invulnerability
    private CharacterController _controller;
    private Renderer[] _playerRenderers;
    private bool _isInvulnerable = false;

    // Inventory
    private Inventory _inventory;


    // =========================
    // UNITY CALLBACKS
    // =========================

    void Awake()
    {
        _controller = GetComponent<CharacterController>();

        // Cerca i Renderer sul Player e su tutti i figli (Face, Player Capsule, ecc.)
        _playerRenderers = GetComponentsInChildren<Renderer>();

        if (_playerRenderers == null || _playerRenderers.Length == 0)
            Debug.LogError("Attenzione: Nessun Renderer trovato sul Player o nei suoi figli! Il lampeggio non funzionerà.");
    }

    void Start()
    {
        hitboxDamage = attackHitbox.GetComponent<HitboxDamage>();
        hitboxCollider = attackHitbox.GetComponent<Collider>();
        hitboxRenderer = attackHitbox.GetComponent<Renderer>();

        hitboxCollider.enabled = false;
        hitboxRenderer.enabled = false;

        RefreshBpmCooldown();
        ResetHealth();
        ResetMusicPoints();
    }

    void Update()
    {
        HandleMovement();
        HandleRotation();
        HandleAttack();

        attackHitbox.transform.position = playerCapsule.transform.position + playerCapsule.transform.forward * attackRange / 2f;
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

    public void EnableMovement(bool value) => canMove = value;

    public void EnableAttack(bool value) => canAttack = value;


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

        if (input.magnitude > 0.1f)
        {
            // Accelerazione
            horizontalVelocity = Vector3.MoveTowards(
                horizontalVelocity,
                desiredDirection * targetSpeed,
                acceleration * Time.deltaTime
            );
        }
        else
        {
            // Decelerazione
            horizontalVelocity = Vector3.MoveTowards(
                horizontalVelocity,
                Vector3.zero,
                deceleration * Time.deltaTime
            );
        }

        // Mantieni Y separata
        currentMovement.x = horizontalVelocity.x;
        currentMovement.z = horizontalVelocity.z;

        characterController.Move(currentMovement * Time.deltaTime);

        // Aggiorna direzione per la rotazione
        movementDirection = desiredDirection;
    }

    private void HandleRotation()
    {
        // Lock durante attacco e cooldown
        if (isAttacking) return;

        Vector3 rawDirection = movementDirection;
        rawDirection.y = 0;

        if (rawDirection.sqrMagnitude < 0.01f) return;

        // Snap alle 8 direzioni cardinali/ordinali
        Vector3 snappedDirection = SnapTo8Directions(rawDirection);
        transform.rotation = Quaternion.LookRotation(snappedDirection);
    }

    private Vector3 SnapTo8Directions(Vector3 direction)
    {
        float angle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg;
        float snapped = Mathf.Round(angle / 45f) * 45f;
        float rad = snapped * Mathf.Deg2Rad;
        return new Vector3(Mathf.Sin(rad), 0f, Mathf.Cos(rad));
    }


    // =========================
    // ATTACK
    // =========================

    private void HandleAttack()
    {
        if (!canAttack) return;
        if (!playerInputManager.AttackInput) return;

        Debug.Log("Attacco eseguito!");
        StartCoroutine(AttackRoutine());
    }

    private IEnumerator AttackRoutine()
    {
        canAttack = false;
        isAttacking = true;  // blocca rotazione

        float originalSpeed = walkingSpeed;
        walkingSpeed *= attackMoveSpeedMultiplier;

        bool isOnBeat = IsOnBeat(out float damageMultiplier);
        hitboxDamage.SetHitboxDamage(attackDamage * damageMultiplier, damageMultiplier);

        hitboxCollider.enabled = true;
        hitboxRenderer.enabled = true;

        yield return new WaitForSeconds(attackBoxDuration);

        hitboxCollider.enabled = false;
        hitboxRenderer.enabled = false;

        // Reset hitbox
        attackHitbox.SetActive(false);
        attackHitbox.SetActive(true);

        walkingSpeed = originalSpeed;

        yield return new WaitForSeconds(attackTime - attackBoxDuration);

        isAttacking = false;  // sblocca rotazione
        canAttack = true;
    }


    // =========================
    // RHYTHM
    // =========================

    public void SetBpm(float newBpm)
    {
        bpm = newBpm;
        RefreshBpmCooldown();
    }

    private void RefreshBpmCooldown()
    {
        if (useBpmCooldown)
            attackTime = 60f / (bpm * 2f);
    }

    private bool IsOnBeat(out float damageMultiplier)
    {
        if (RhythmManager.Instance == null)
        {
            damageMultiplier = 0.5f;
            return false;
        }

        return RhythmManager.Instance.IsOnBeat(
            RhythmManager.Instance.perfectInputWindow,
            RhythmManager.Instance.goodInputWindow,
            out damageMultiplier
        );
    }


    // =========================
    // HEALTH & DAMAGE
    // =========================

    public void TakeDamage(float amount)
    {
        if (_isInvulnerable) return;

        currentHealthPoints = Mathf.Clamp(currentHealthPoints - amount, 0, maxHealthPoints);
        UpdateUI();

        if (uiJuice != null) uiJuice.Shake();

        if (currentHealthPoints <= 0)
            Respawn();
    }

    private void Respawn()
    {
        ResetHealth();

        if (_controller != null) _controller.enabled = false;
        transform.position = Vector3.zero;
        transform.rotation = Quaternion.identity;
        if (_controller != null) _controller.enabled = true;

        if (_playerRenderers != null && _playerRenderers.Length > 0)
            StartCoroutine(MultiRendererInvulnerabilityRoutine());
    }

    private IEnumerator MultiRendererInvulnerabilityRoutine()
    {
        _isInvulnerable = true;

        float timer = 0;
        bool currentlyVisible = true;

        while (timer < invulnerabilityDuration)
        {
            currentlyVisible = !currentlyVisible;

            foreach (Renderer r in _playerRenderers)
            {
                if (r != null) r.enabled = currentlyVisible;
            }

            yield return new WaitForSeconds(flickerInterval);
            timer += flickerInterval;
        }

        // Ripristino finale di tutti i Renderer
        foreach (Renderer r in _playerRenderers)
        {
            if (r != null) r.enabled = true;
        }

        _isInvulnerable = false;
    }

    private void ResetHealth()
    {
        currentHealthPoints = maxHealthPoints;
        UpdateUI();
    }

    public void Heal(float amount)
    {
        currentHealthPoints = Mathf.Clamp(currentHealthPoints + amount, 0, maxHealthPoints);
        UpdateUI();
    }

    public void SetHealth(float amount)
    {
        currentHealthPoints = Mathf.Clamp(amount, 0, maxHealthPoints);
        UpdateUI();
    }

    public void SetMaxHealth(float amount)
    {
        maxHealthPoints = amount;
        currentHealthPoints = Mathf.Clamp(currentHealthPoints, 0, maxHealthPoints);
        UpdateUI();
    }

    private void ResetMusicPoints()
    {
        currentMusicPoints = 0;
    }

    public void SetMusicPoints(float amount)
    {
        currentMusicPoints = Mathf.Clamp(currentMusicPoints + amount, 0, maxMusicPoints);
    }

    public void SetMaxMusicPoints(float amount)
    {
        maxMusicPoints = amount;
        currentMusicPoints = Mathf.Clamp(currentMusicPoints, 0, maxMusicPoints);
    }

    private void UpdateUI()
    {
        if (healthSlider != null)
        {
            healthSlider.maxValue = maxHealthPoints;
            healthSlider.value = currentHealthPoints;
        }
    }


    // =========================
    // DRUGS
    // =========================

    /*
    public void ApplyDrug(DrugData drugData)    // Applica moltiplicatori su stato corrente
    {
        walkingSpeed *= drugData.speedMultiplier;
        attackTime /= drugData.attackRateMultiplier;
        attackType = drugData.attackType;

        float previousMaxHealth = this.maxHealthPoints;
        maxHealthPoints *= drugData.healthMultiplier;

        currentHealthPoints = Mathf.Clamp(currentHealthPoints, 0, maxHealthPoints);
        if (maxHealthPoints > previousMaxHealth)
            currentHealthPoints += maxHealthPoints - previousMaxHealth;

        UpdateUI();

        if (drugData.damageOverTime)
            ApplyDamageOverTime(drugData.damageChangeTime, drugData.damageCurve);
        else
            attackDamage *= drugData.damageMultiplier;
    }*/

    public void ApplyMultipliers(ItemData itemData)    // Applica moltiplicatori su stato default
    {
        walkingSpeed = DEFAULT_WALKING_SPEED * itemData.speedMultiplier;
        attackTime = DEFAULT_ATTACK_TIME / itemData.attackRateMultiplier;
        attackType = itemData.attackType;

        float previousMaxHealth = this.maxHealthPoints;
        maxHealthPoints = DEFAULT_MAX_HEALTH_POINTS * itemData.healthMultiplier;

        currentHealthPoints = Mathf.Clamp(currentHealthPoints, 0, maxHealthPoints);
        if (maxHealthPoints > previousMaxHealth)
            currentHealthPoints += maxHealthPoints - previousMaxHealth;

        UpdateUI();

        if (itemData.damageOverTime)
            ApplyDamageOverTime(itemData.damageChangeTime, itemData.damageCurve);
        else
            attackDamage = DEFAULT_ATTACK_DAMAGE * itemData.damageMultiplier;
    }

    public void ApplyDamageOverTime(float damageChangeTime, AnimationCurve damageCurve)
    {
        StartCoroutine(DamageOverTimeRoutine(damageChangeTime, damageCurve));
    }

    private IEnumerator DamageOverTimeRoutine(float damageChangeTime, AnimationCurve damageCurve)
    {
        float elapsedTime = 0f;
        float initialAttackDamage =
        //    attackDamage;    // su stato corrente
            DEFAULT_ATTACK_DAMAGE;    // su stato default

        while (elapsedTime <= damageChangeTime)
        {
            Debug.Log("Applying damage over time...");
            attackDamage = initialAttackDamage * damageCurve.Evaluate(elapsedTime / damageChangeTime);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
    }
}