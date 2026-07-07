using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System;
using System.Collections.Generic;
using Random = UnityEngine.Random;
using Unity.VisualScripting;

public class Player : MonoBehaviour
{
    // =========================
    // SERIALIZED FIELDS
    // =========================

    [Header("Movement Settings (Base)")]
    [SerializeField] private float walkingSpeed;
    [SerializeField] private float baseWalkingSpeed = 8f;
    [SerializeField] private float rotationSpeed = 720f; // gradi al secondo, regola a piacere
    [SerializeField] private float acceleration = 100f;
    [SerializeField] private float deceleration = 200f;

    [Header("Knockback Settings")]
    [SerializeField] private float knockbackDecay = 10f;
    private Vector3 knockbackVelocity = Vector3.zero;

    [Header("Dash Settings")]
    [SerializeField] private float dashSpeed = 25f;
    [SerializeField] private float dashDuration = 0.2f;
    [SerializeField] private float dashCooldown = 1f;
    [SerializeField] private float dashInvincibilityTime = 0.3f;

    private bool isDashing = false;
    private bool canDash = true;
    private Vector2 virtualAimPosition = Vector2.zero;

    [Header("Virtual Mouse Settings")]
    [SerializeField] private float mouseSensitivity = 1f;
    [SerializeField] private float maxAimRadius = 200f;
    [SerializeField] private float minAimDeadzone = 20f;

    [Header("Attack Settings (Base)")]
    [SerializeField] private AttackType attackType = AttackType.DEFAULT;
    [SerializeField] private float attackDamage;
    [SerializeField] private float baseAttackDamage = 15f;
    [SerializeField] private float attackRange = 2f;
    [SerializeField] private float attackWidth = 2f;

    [SerializeField] private float attackBoxDuration = 0.2f;
    [SerializeField] private float attackTime = 0.4f;
    [SerializeField] private float attackMoveSpeedMultiplier = 0f;

    [Header("Rhythm Settings")]
    [SerializeField] private float bpm = 120f;
    [SerializeField] private bool useBpmCooldown = false;

    [Header("Health Settings (Base)")]
    [SerializeField] private float baseMaxHealth = 100f;
    private float maxHealthPoints;

    [Header("Leveling Settings")]
    [SerializeField] private int killsToLevelUp = 5;
    [SerializeField] private float healthIncreasePerLevel = 20f;
    [SerializeField] private float damageIncreasePerLevel = 5f;
    [SerializeField] private float speedIncreasePerLevel = 1.5f;

    [SerializeField] private int currentKills = 0;
    [field: SerializeField] public int currentLevel { get; private set; } = 1;

    [Header("Death Settings")]
    [SerializeField] private float respawnDelay = 2f;
    [SerializeField] private float hideDuration = 1.5f;
    [field: SerializeField] public bool IsDead { get; private set; } = false;

    [Header("Music Settings")]
    [SerializeField] private float timePerMusicPointDecrease = 0.3f;
    public float maxMusicPoints = 100f;
    public float musicPtsThreshold = 75f;

    [Header("Mental Status Settings")]
    public PlayerMentalStatus mentalStatus = PlayerMentalStatus.DEFAULT;
    public DrugType consumedDrug = DrugType.NONE;

    [Header("Invulnerability Settings")]
    [SerializeField] private float invulnerabilityDuration = 2f;
    [SerializeField] private float flickerInterval = 0.1f;

    [Header("Inventory Settings")]
    [SerializeField] private int inventorySize = 3;
    [SerializeField] private float itemUseDelay = 0.5f;
    private Coroutine activeDrugCoroutine;

    [Header("UI")]
    public Slider healthSlider;
    public UIJuice uiJuice;
    public InventoryUI inventoryUI;

    [Header("Audio")]
    [SerializeField] private FMODUnity.EventReference attackSoundEvent = new FMODUnity.EventReference();
    [SerializeField] private string attackSoundParameter = "Attack Type";
    [SerializeField] private FMODUnity.EventReference dashSoundEvent = new FMODUnity.EventReference();
    [SerializeField] private FMODUnity.EventReference hitSoundEvent = new FMODUnity.EventReference();
    [SerializeField] private FMODUnity.EventReference badTripSoundEvent = new FMODUnity.EventReference(); // qui metti lo Snapshot
    private FMOD.Studio.EventInstance badTripInstance;
    [SerializeField] private FMODUnity.EventReference changeMusicSoundEvent = new FMODUnity.EventReference();
    [SerializeField] private FMODUnity.EventReference consumeItemSoundEvent = new FMODUnity.EventReference();
    [SerializeField] private string consumeItemSoundParameter = "Item";

    [Header("References")]
    [SerializeField] private PlayerInputManager playerInputManager;
    [SerializeField] private CharacterController characterController;
    [SerializeField] private Camera gameCamera;
    [SerializeField] private GameObject playerCapsule;
    [SerializeField] private GameObject attackHitbox;
    [SerializeField] private Animator playerAnimator;
    [SerializeField] private GameObject badTripVolume; 

    [Header("Grafica Player (Per il Lampeggio)")]
    [SerializeField] private Renderer[] playerRenderers;

    // ALTRE COSTANTI DI GIOCO
    private const float DEFAULT_ATTACK_RANGE = 2f;
    private const float DEFAULT_ATTACK_WIDTH = 2f;
    private const float DEFAULT_ATTACK_BOX_DURATION = 0.2f;
    private const float DEFAULT_ATTACK_TIME = 0.4f;
    private const float DEFAULT_ATTACK_MOVE_SPEED_MULTIPLIER = 0.5f;
    private const float DEFAULT_INVULNERABILITY_DURATION = 2f;
    private const float DEFAULT_FLICKER_INTERVAL = 0.1f;
    private const float DEFAULT_MAX_MUSIC_POINTS = 100f;

    // =========================
    // PROPERTIES & CURRENT STATS
    // =========================
    public float currentHealthPoints { get; private set; }
    public float currentMusicPoints { get; private set; }
    public MusicType selectedMusicType = MusicType.DEFAULT;

    // Valori attuali che possono essere buffati dai modificatori
    

    // =========================
    // PRIVATE FIELDS
    // =========================

    private Vector3 currentMovement;
    private Vector3 movementDirection;
    private HitboxDamage hitboxDamage;
    private Renderer hitboxRenderer;
    private Collider hitboxCollider;

    private bool canMove = true;
    private bool canAttack = true;
    private bool isAttacking = false;
    private bool canUseInventory = true;
    private bool musicDrugCombo = false;
    public bool canChangeMusicType = false;

    private CharacterController controller;
    private bool isInvulnerable = false;
    private Coroutine activeInvincibilityCoroutine;
    private Inventory inventory;

    // =========================
    // UNITY CALLBACKS
    // =========================

    void Awake()
    {
        controller = GetComponent<CharacterController>();
        hitboxDamage = attackHitbox.GetComponent<HitboxDamage>();
        hitboxCollider = attackHitbox.GetComponent<Collider>();
        hitboxRenderer = attackHitbox.GetComponent<Renderer>();

        if(badTripVolume != null)
        {
            badTripVolume.SetActive(false);
        }

        // 1. Prendiamo tutti i renderer presenti nel Player e nei figli
        Renderer[] allRenderers = GetComponentsInChildren<Renderer>(true);
        List<Renderer> validRenderers = new List<Renderer>();

        // 2. Li controlliamo uno ad uno
        foreach (Renderer r in allRenderers)
        {
            // Escludiamo esplicitamente gli oggetti di servizio in base al loro nome
            if (r.gameObject.name == "Attack Hitbox" ||
                r.gameObject.name == "Face" ||
                r.gameObject.name == "Player Capsule"||
                r.gameObject.name == "direzione attacco")
            {
                continue; // Salta questo oggetto e passa al prossimo
            }

            // Se non è uno degli oggetti vietati, aggiungilo alla lista di quelli che lampeggeranno
            validRenderers.Add(r);
        }

        // 3. Salviamo la lista pulita nel nostro array
        playerRenderers = validRenderers.ToArray();

        if (playerRenderers == null || playerRenderers.Length == 0)
            Debug.LogWarning("Attenzione: Nessun Renderer valido trovato sul Player o nei suoi figli!");
    }

    void Start()
    {
        walkingSpeed = baseWalkingSpeed;
        attackDamage = baseAttackDamage;
        maxHealthPoints = baseMaxHealth;

        hitboxCollider.enabled = false;
        hitboxRenderer.enabled = false;

        RefreshBpmCooldown();
        ResetHealth();
        ResetMusicPoints();

        inventory = new Inventory(inventorySize);
        UpdateInventoryUI();
    }

    void Update()
    {
        HandleMusicChange();
        VerifyMusicDrugCombo();
        if (musicDrugCombo)
            RemoveDebuffs();

        // --- RISOLUZIONE BUG "INACTIVE CONTROLLER" ---
        if (IsDead) return;

        HandleDash();

        if (!isDashing)
        {
            HandleMovement();
            HandleRotation();
        }

        // --- SISTEMA KNOCKBACK ---
        if (knockbackVelocity.sqrMagnitude > 0.1f)
        {
            characterController.Move(knockbackVelocity * Time.deltaTime);
            knockbackVelocity = Vector3.Lerp(knockbackVelocity, Vector3.zero, knockbackDecay * Time.deltaTime);
        }

        HandleAttack();
        HandleInventory();

        attackHitbox.transform.position = playerCapsule.transform.position + playerCapsule.transform.forward * attackRange / 2f;
        attackHitbox.transform.localScale = new Vector3(attackWidth, 0.1f, attackRange);

        if (transform.position.y > 0.05f)
        {
            Vector3 bloccatoAlSuolo = transform.position;
            bloccatoAlSuolo.y = 0f;
            transform.position = bloccatoAlSuolo;
        }
    }

    // =========================
    // LEVEL UP SYSTEM
    // =========================

    private void OnEnable()
    {
        EnemyBase.OnEnemyDied += HandleEnemyKilled;
    }

    private void OnDisable()
    {
        EnemyBase.OnEnemyDied -= HandleEnemyKilled;
    }

    private void OnDestroy()
    {
        if (badTripInstance.isValid())
        {
            badTripInstance.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
            badTripInstance.release();
        }
    }

    private void HandleEnemyKilled()
    {
        currentKills++;

        if (currentKills >= killsToLevelUp)
        {
            LevelUp();
        }
    }

    private void LevelUp()
    {
        currentKills = 0;
        currentLevel++;

        if (uiJuice != null) uiJuice.Shake();

        int randomStat = UnityEngine.Random.Range(0, 3);

        switch (randomStat)
        {
            case 0: // POTENZIA SALUTE
                baseMaxHealth += healthIncreasePerLevel;
                maxHealthPoints += healthIncreasePerLevel;
                Heal(healthIncreasePerLevel);
                Debug.Log($"Level Up {currentLevel}! Max Health aumentata a {baseMaxHealth}");
                break;

            case 1: // POTENZIA DANNO
                baseAttackDamage += damageIncreasePerLevel;
                attackDamage += damageIncreasePerLevel;
                Debug.Log($"Level Up {currentLevel}! Danno aumentato a {baseAttackDamage}");
                break;

            case 2: // POTENZIA VELOCITÀ
                baseWalkingSpeed += speedIncreasePerLevel;
                walkingSpeed += speedIncreasePerLevel;
                Debug.Log($"Level Up {currentLevel}! Velocità aumentata a {baseWalkingSpeed}");
                break;
        }
        UpdateUI();
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
    // TEMPORARY INVINCIBILITY
    // =========================

    public void GrantTemporaryInvincibility(float time)
    {
        if (activeInvincibilityCoroutine != null)
        {
            StopCoroutine(activeInvincibilityCoroutine);
        }
        activeInvincibilityCoroutine = StartCoroutine(TemporaryInvincibilityRoutine(time));
    }

    private IEnumerator TemporaryInvincibilityRoutine(float time)
    {
        isInvulnerable = true;
        yield return new WaitForSeconds(time);
        isInvulnerable = false;
        activeInvincibilityCoroutine = null;
    }

    // =========================
    // MOVEMENT
    // =========================

    private void HandleMovement()
    {
        Vector2 input = canMove ? playerInputManager.MoveInput : Vector2.zero;

        Vector3 desiredDirection = gameCamera.transform.forward * input.y + gameCamera.transform.right * input.x;
        desiredDirection.y = 0;
        desiredDirection.Normalize();

        float targetSpeed = walkingSpeed * input.magnitude;
        Vector3 horizontalVelocity = new Vector3(currentMovement.x, 0, currentMovement.z);

        if (input.magnitude > 0.1f)
        {
            horizontalVelocity = Vector3.MoveTowards(
                horizontalVelocity,
                desiredDirection * targetSpeed,
                acceleration * Time.deltaTime
            );
        }
        else
        {
            horizontalVelocity = Vector3.MoveTowards(
                horizontalVelocity,
                Vector3.zero,
                deceleration * Time.deltaTime
            );
        }

        currentMovement.x = horizontalVelocity.x;
        currentMovement.z = horizontalVelocity.z;
        characterController.Move(currentMovement * Time.deltaTime);

        playerAnimator.SetFloat("X", horizontalVelocity.x);
        playerAnimator.SetFloat("Y", horizontalVelocity.z);
        playerAnimator.SetFloat("speed", input.magnitude);
    }

   private void HandleRotation()
{
    Vector3 targetDirection = Vector3.zero;
    Vector2 aimInput = playerInputManager.AttackDirectionInput;
    bool isGamepadInput = aimInput.sqrMagnitude > 0f && IsLikelyGamepadInput(aimInput);

    if (isGamepadInput)
    {
        // Lo stick è una posizione assoluta, non un delta: niente accumulo.
        bool isAimingWithStick = aimInput.magnitude > 0.2f; // deadzone dedicata allo stick

        if (isAimingWithStick)
        {
            targetDirection = gameCamera.transform.forward * aimInput.y + gameCamera.transform.right * aimInput.x;
            targetDirection.y = 0f;
        }
        else if (!isAttacking)
        {
            targetDirection = movementDirection;
            targetDirection.y = 0f;
        }
        else
        {
            return;
        }
    }
    else
    {
        // Mouse: delta accumulato, come prima.
        virtualAimPosition += aimInput * mouseSensitivity;

        if (virtualAimPosition.magnitude > maxAimRadius)
            virtualAimPosition = virtualAimPosition.normalized * maxAimRadius;

        bool isAimingWithMouse = virtualAimPosition.sqrMagnitude > (minAimDeadzone * minAimDeadzone);

        if (isAimingWithMouse)
        {
            Vector3 mouseWorldDirection = new Vector3(virtualAimPosition.x, 0f, virtualAimPosition.y);
            targetDirection = gameCamera.transform.forward * mouseWorldDirection.z + gameCamera.transform.right * mouseWorldDirection.x;
            targetDirection.y = 0f;
        }
        else if (!isAttacking)
        {
            targetDirection = movementDirection;
            targetDirection.y = 0f;
        }
        else
        {
            return;
        }
    }

    if (targetDirection.sqrMagnitude < 0.01f) return;

    Vector3 snappedDirection = targetDirection.normalized;
    playerAnimator.SetFloat("X_atk", snappedDirection.x);
    playerAnimator.SetFloat("Y_atk", snappedDirection.z);

    // Smoothing sulla rotazione per eliminare lo "snap" del mouse.
    Quaternion targetRotation = Quaternion.LookRotation(snappedDirection);
    transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
}

private bool IsLikelyGamepadInput(Vector2 input)
{
    // Placeholder semplice: se il tuo PlayerInputManager sa già distinguere
    // il dispositivo, sostituisci questo metodo con quel valore diretto.
    return UnityEngine.InputSystem.Gamepad.current != null &&
           UnityEngine.InputSystem.Gamepad.current.rightStick.ReadValue().sqrMagnitude > 0.01f;
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

        StartCoroutine(AttackRoutine());
        playerInputManager.ConsumeAttackInput();
    }

    private IEnumerator AttackRoutine()
    {
        canAttack = false;
        isAttacking = true;

        float originalSpeed = walkingSpeed;
        walkingSpeed *= attackMoveSpeedMultiplier;

        bool isOnBeat = IsOnBeat(out float damageMultiplier);
        hitboxDamage.SetHitboxDamage(attackDamage * damageMultiplier, damageMultiplier);

        playerAnimator.SetTrigger("Attack");
        PlayAttackSound();

        hitboxCollider.enabled = true;
        hitboxRenderer.enabled = true;

        yield return new WaitForSeconds(attackBoxDuration);

        hitboxCollider.enabled = false;
        hitboxRenderer.enabled = false;

        attackHitbox.SetActive(false);
        attackHitbox.SetActive(true);

        walkingSpeed = originalSpeed;

        yield return new WaitForSeconds(attackTime - attackBoxDuration);

        isAttacking = false;
        canAttack = true;
    }

    private void PlayAttackSound()
    {
        if (attackSoundEvent.IsNull) return;
        FMOD.Studio.EventInstance attackInstance = FMODUnity.RuntimeManager.CreateInstance(attackSoundEvent);
        attackInstance.setParameterByName(attackSoundParameter, GetAttackTypeParamValue(attackType));
        attackInstance.set3DAttributes(FMODUnity.RuntimeUtils.To3DAttributes(transform));
        attackInstance.start();
        attackInstance.release();
    }

    private float GetAttackTypeParamValue(AttackType type)
    {
        switch (type)
        {
            case AttackType.DEFAULT: return 0f;
            case AttackType.CLAYMORE: return 1f;
            case AttackType.DAGGERS: return 2f;
            case AttackType.LONGSWORD: return 3f;
            case AttackType.WHIP: return 4f;
            default: return 0f;
        }
    }

    // =========================
    // DASH
    // =========================

    private void HandleDash()
    {
        if (!canMove || !canDash || isAttacking) return;

        if (playerInputManager.DashInput)
        {
            StartCoroutine(DashRoutine());
            playerInputManager.ConsumeDashInput();
        }
    }

    private IEnumerator DashRoutine()
    {
        canDash = false;
        isDashing = true;
        playerAnimator.SetBool("isDashing", true);

        GrantTemporaryInvincibility(dashInvincibilityTime);

        Vector3 dashDirection = (currentMovement.sqrMagnitude > 0.1f) ? currentMovement.normalized : transform.forward;
        dashDirection.y = 0f;

        PlayDashSound();

        float startTime = Time.time;

        while (Time.time < startTime + dashDuration)
        {
            characterController.Move(dashDirection * dashSpeed * Time.deltaTime);
            yield return null;
        }

        isDashing = false;
        playerAnimator.SetBool("isDashing", false);

        yield return new WaitForSeconds(dashCooldown);
        canDash = true;
    }

    private void PlayDashSound()
    {
        if (dashSoundEvent.IsNull) return;
        FMOD.Studio.EventInstance dashInstance = FMODUnity.RuntimeManager.CreateInstance(dashSoundEvent);
        dashInstance.set3DAttributes(FMODUnity.RuntimeUtils.To3DAttributes(transform));
        dashInstance.start();
        dashInstance.release();
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
    // HEALTH, DAMAGE & DEATH
    // =========================

    // Overload normale
    public void TakeDamage(float amount)
    {
        TakeDamage(amount, transform.position, 0f);
    }

    // Overload completo con gestione Knockback
    public void TakeDamage(float amount, Vector3 attackerPosition, float knockbackForce)
    {
        if (isInvulnerable || IsDead) return;

        if (knockbackForce > 0f)
        {
            Vector3 direction = (transform.position - attackerPosition).normalized;
            direction.y = 0f;
            knockbackVelocity = direction * knockbackForce;
        }

        PlayHitSound();
        //playerAnimator.SetTrigger("Hit");
        currentHealthPoints = Mathf.Clamp(currentHealthPoints - amount, 0, maxHealthPoints);
        UpdateUI();

        if (uiJuice != null) uiJuice.Shake();

        if (currentHealthPoints <= 0)
        {
            StartCoroutine(DeathRoutine());
        }
    }

    private IEnumerator DeathRoutine()
    {
        IsDead = true;
        DisableAllControls();

        // NOVITÀ: Disattiviamo SUBITO il CharacterController. 
        // Il corpo rimane visibile a terra per la "Fase 1", ma diventa intangibile.
        // I nemici e gli oggetti fisici ora gli passeranno attraverso come se fosse un fantasma!
        if (controller != null) controller.enabled = false;

        // FASE 1: Il cadavere rimane fermo a terra
        yield return new WaitForSeconds(respawnDelay);

        // FASE 2: Nasconde i renderer (il cadavere svanisce)
        foreach (Renderer r in playerRenderers)
        {
            if (r != null) r.enabled = false;
        }

        // (Niente più teletrasporto a -1000! Il player resta esattamente dov'è, 
        // invisibile e intangibile. La telecamera resta tranquilla.)

        // Aspetta i secondi di assenza (schermo vuoto per il player)
        yield return new WaitForSeconds(hideDuration);

        // FASE 3: Respawn effettivo (ci riporta a Vector3.zero e riaccende tutto)
        Respawn();

        EnableAllControls();
        IsDead = false;
    }

    private void PlayHitSound()
    {
        if (hitSoundEvent.IsNull) return;
        FMOD.Studio.EventInstance hitInstance = FMODUnity.RuntimeManager.CreateInstance(hitSoundEvent);
        hitInstance.set3DAttributes(FMODUnity.RuntimeUtils.To3DAttributes(transform));
        hitInstance.start();
        hitInstance.release();
    }

    private void Respawn()
    {
        ResetHealth();

        // Riattiviamo la posizione al centro PRIMA di accendere il controller
        if (controller != null) controller.enabled = false;
        transform.position = Vector3.zero;
        transform.rotation = Quaternion.identity;
        if (controller != null) controller.enabled = true;

        if (playerRenderers != null && playerRenderers.Length > 0)
            StartCoroutine(MultiRendererInvulnerabilityRoutine());
    }

    private IEnumerator MultiRendererInvulnerabilityRoutine()
    {
        isInvulnerable = true;
        float timer = 0;
        bool currentlyVisible = false;

        while (timer < invulnerabilityDuration)
        {
            currentlyVisible = !currentlyVisible;

            foreach (Renderer r in playerRenderers)
            {
                if (r != null) r.enabled = currentlyVisible;
            }

            yield return new WaitForSeconds(flickerInterval);
            timer += flickerInterval;
        }

        foreach (Renderer r in playerRenderers)
        {
            if (r != null) r.enabled = true;
        }

        isInvulnerable = false;
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
        currentMusicPoints = 0f;
    }

    public void SetMusicPoints(float amount)
    {
        currentMusicPoints = Mathf.Clamp(currentMusicPoints + amount, 0, maxMusicPoints);
    }

    public void AddMusicPoints(float amount)
    {
        currentMusicPoints = Mathf.Clamp(currentMusicPoints + amount, 0, maxMusicPoints);
    }

    public void SetMaxMusicPoints(float amount)
    {
        maxMusicPoints = amount;
        currentMusicPoints = Mathf.Clamp(currentMusicPoints, 0, maxMusicPoints);
    }

    private void ChangeSelectedMusicType(MusicType musicType) => selectedMusicType = musicType;

    private void ConfirmMusicType()
    {
        RhythmManager.Instance.SetMusicStyle(selectedMusicType);
    }

    private void HandleMusicChange()
    {
        if (currentMusicPoints < musicPtsThreshold || RhythmManager.Instance.musicType != MusicType.DEFAULT)
        {
            canChangeMusicType = false;
            return;
        }
        ;
        canChangeMusicType = true;
        if (playerInputManager.SongSwitchInput)
        {
            ChangeSelectedMusicType((MusicType)(((int)selectedMusicType + 1) % 5));
            playerInputManager.ConsumeSongSwitchInput();
            PlayMusicChangeSound();
            Debug.Log($"Selected music type: {selectedMusicType}");
        }
        if (selectedMusicType == RhythmManager.Instance.musicType) return;
        if (playerInputManager.SongConfirmInput)
        {
            ConfirmMusicType();
            playerInputManager.ConsumeSongConfirmInput();
            DecreaseMusicPointsOverTime(1f, timePerMusicPointDecrease);
        }
    }

    private void PlayMusicChangeSound()
    {
        if (changeMusicSoundEvent.IsNull) return;
        FMOD.Studio.EventInstance musicChangeInstance = FMODUnity.RuntimeManager.CreateInstance(changeMusicSoundEvent);
        musicChangeInstance.set3DAttributes(FMODUnity.RuntimeUtils.To3DAttributes(transform));
        musicChangeInstance.start();
        musicChangeInstance.release();
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
    // INVENTORY
    // =========================

    private void HandleInventory()
    {
        if (!canUseInventory) return;

        if (playerInputManager.GoLeftInventorySlotInput)
        {
            GoLeftInventorySlot();
            UpdateInventoryUI();
            playerInputManager.ConsumeGoLeftInventorySlotInput();
        }
        if (playerInputManager.GoRightInventorySlotInput)
        {
            GoRightInventorySlot();
            UpdateInventoryUI();
            playerInputManager.ConsumeGoRightInventorySlotInput();
        }
        if (playerInputManager.UseItemInput)
        {
            ItemData itemToUse = inventory.GetSelectedItem();
            if (itemToUse != null)
            {
                // MODIFICA QUI: Rimuove l'oggetto solo se UseItem restituisce true
                if (UseItem(itemToUse))
                {
                    RemoveItem(inventory.GetSelectedIndex());
                    inventory.SortItems();
                    UpdateInventoryUI();
                    DelayAfterItemUse(itemUseDelay);
                }
            }
            playerInputManager.ConsumeUseItemInput();
        }
        if (playerInputManager.DeleteItemInput)
        {
            RemoveItem(inventory.GetSelectedIndex());
            inventory.SortItems();
            UpdateInventoryUI();
            playerInputManager.ConsumeDeleteItemInput();
        }
    }

    private void DelayAfterItemUse(float delayTime)
    {
        StartCoroutine(DelayAfterItemUseRoutine(delayTime));
    }

    private IEnumerator DelayAfterItemUseRoutine(float delayTime)
    {
        yield return new WaitForSeconds(delayTime);
    }

    // Ora restituisce il risultato (true/false) all'oggetto che tenta di farsi raccogliere
    public bool AddItem(ItemData item)
    {
        // Prova ad aggiungere l'oggetto in testa all'inventario
        if (inventory.AddItemInHead(item))
        {
            UpdateInventoryUI();
            return true; // Preso!
        }
        return false; // Rifiutato!
    }

    public bool AddItem(ItemData item, int index)
    {
        if (inventory.AddItem(item, index))
        {
            UpdateInventoryUI();
            return true;
        }
        return false;
    }

    public void RemoveItem(ItemData item)
    {
        inventory.RemoveItem(item);
    }

    public void RemoveItem(int index)
    {
        inventory.RemoveItem(index);
    }

    public void ClearInventory()
    {
        inventory.ClearInventory();
    }

    public void GoLeftInventorySlot()
    {
        int size = inventory.GetInventorySize();
        inventory.SetSelectedIndex((inventory.GetSelectedIndex() + size - 1) % size);
    }

    public void GoRightInventorySlot()
    {
        int size = inventory.GetInventorySize();
        inventory.SetSelectedIndex((inventory.GetSelectedIndex() + 1) % size);
    }

    private void UpdateInventoryUI()
    {
        if (inventoryUI != null && inventory != null)
        {
            inventoryUI.RefreshUI(inventory, attackType);
        }
    }

    // =========================
    // ITEMS
    // =========================


        public bool UseItem(ItemData itemData)
    {
        if (itemData == null) return false;

        // CONTROLLO DROGA: Se cerchi di usare una droga ma ne hai già una attiva, blocca tutto!
        if (itemData.itemType == ItemType.DRUG && consumedDrug != DrugType.NONE)
        {
            Debug.Log("Sei già sotto l'effetto di una droga! Attendi che svanisca.");
            // (Opzionale: qui potresti aggiungere un suono d'errore o un UIJuice shake)
            return false;
        }

        ApplyModifiers(itemData);
        PlayConsumeItemSound(GetItemLabel(itemData));

        switch (itemData.itemType)
        {
            case ItemType.WATER:
                // L'acqua funge da "Cura": pulisce gli effetti delle droghe attive
                if (activeDrugCoroutine != null)
                {
                    StopCoroutine(activeDrugCoroutine);
                    activeDrugCoroutine = null;
                }
                if (mentalStatus == PlayerMentalStatus.BADTRIP)
                {
                    StopBadTripSound();
                    badTripVolume.SetActive(false);
                }
                ApplyDrugStatus(DrugType.NONE);
                ApplyMentalStatus(PlayerMentalStatus.DEFAULT);
                ResetModifiersToDefault();
                return true; // Oggetto consumato con successo

            case ItemType.DRUG:
                ApplyDrugStatus(itemData.drugType);
                if (IsGettingBadTrip(itemData.badTripChance))
                    ApplyMentalStatus(PlayerMentalStatus.BADTRIP);
                else
                    ApplyMentalStatus(PlayerMentalStatus.STUNNED);
                
                // Avvia il timer di scadenza della droga leggendo i secondi dall'oggetto
                activeDrugCoroutine = StartCoroutine(DrugEffectRoutine(itemData.drugDuration));
                return true; // Oggetto consumato con successo
        }

        return false;
    }

    private void PlayConsumeItemSound(string itemLabel)
    {
        if (consumeItemSoundEvent.IsNull) return;

        FMOD.Studio.EventInstance consumeItemInstance =
            FMODUnity.RuntimeManager.CreateInstance(consumeItemSoundEvent);

        consumeItemInstance.setParameterByNameWithLabel(
            consumeItemSoundParameter,
            itemLabel);

        consumeItemInstance.set3DAttributes(FMODUnity.RuntimeUtils.To3DAttributes(transform));
        consumeItemInstance.start();
        consumeItemInstance.release();
    }

    private string GetItemLabel(ItemData itemData)
    {
        if (itemData.itemType == ItemType.WATER)
            return "WATER";

        switch (itemData.drugType)
        {
            case DrugType.MARIJUANA: return "MARIJUANA";
            case DrugType.COCAINE:   return "COCAINE";
            case DrugType.MDMA:      return "MDMA";
            case DrugType.LSD:       return "LSD";
            default:                 return "WATER";
        }
    }

    // ==========================================
    // NUOVE FUNZIONI PER GESTIRE LA SCADENZA
    // ==========================================

    private IEnumerator DrugEffectRoutine(float duration)
    {
        if(mentalStatus == PlayerMentalStatus.BADTRIP)
        {
            ApplyBadTrip();
        }
        // Aspetta i secondi definiti nell'ItemData
        yield return new WaitForSeconds(duration);

        // Tempo scaduto: resetta gli stati
        if (mentalStatus == PlayerMentalStatus.BADTRIP)
        {
            StopBadTripSound();
            badTripVolume.SetActive(false);
        }
        ApplyDrugStatus(DrugType.NONE);
        ApplyMentalStatus(PlayerMentalStatus.DEFAULT);
        ResetModifiersToDefault();
        

        activeDrugCoroutine = null;
        Debug.Log("L'effetto della droga è svanito, torni normale.");
    }

    private void ResetModifiersToDefault()
    {
        walkingSpeed = baseWalkingSpeed;
        attackTime = DEFAULT_ATTACK_TIME;
        attackBoxDuration = DEFAULT_ATTACK_BOX_DURATION;
        attackDamage = baseAttackDamage;
        ChangeAttackType(AttackType.DEFAULT);
        float previousMaxHealth = this.maxHealthPoints;
        maxHealthPoints = baseMaxHealth;
        currentHealthPoints = Mathf.Clamp(currentHealthPoints, 0, maxHealthPoints);
        if (maxHealthPoints > previousMaxHealth)
            currentHealthPoints += maxHealthPoints - previousMaxHealth;
        UpdateUI(); // Questo aggiorna solo la barra della vita
        UpdateInventoryUI(); // Forza la UI (e la faccia!) ad aggiornarsi
    }


    /*
        public void UseItem(ItemData itemData)
        {
            if (itemData == null) return;
            ApplyModifiers(itemData);
            switch (itemData.itemType)
            {
                case ItemType.WATER:
                    ApplyDrugStatus(DrugType.NONE);
                    ApplyMentalStatus(PlayerMentalStatus.DEFAULT);
                    break;
                case ItemType.DRUG:
                    ApplyDrugStatus(itemData.drugType);
                    if (IsGettingBadTrip(itemData.badTripChance))
                    {
                        ApplyMentalStatus(PlayerMentalStatus.BADTRIP);
                        ApplyBadTrip();
                        Debug.Log("BADTRIP");
                    }
                    else
                        ApplyMentalStatus(PlayerMentalStatus.STUNNED);
                    break;
            }
        }

        private void DrugCooldown(float drugCooldown)
        {
            StartCoroutine(DrugCooldownRoutine(drugCooldown));
        }

        private IEnumerator DrugCooldownRoutine(float drugCooldown)
        {
            yield return new WaitForSeconds(drugCooldown);
            ResetDefaultModifiers();
            StopBadTripSound();
        }
    */
    private bool IsGettingBadTrip(float badTripChance)
    {
        return Convert.ToBoolean(Random.Range(0, 100) < badTripChance * 100);
    }

    private void ApplyMentalStatus(PlayerMentalStatus mentalStatus)
    {
        this.mentalStatus = mentalStatus;
    }

    private void ApplyDrugStatus(DrugType drugType)
    {
        this.consumedDrug = drugType;
    }

    private void ApplyModifiers(ItemData itemData)
    {
        ChangeAttackType(itemData.attackType);
        walkingSpeed = baseWalkingSpeed * itemData.speedMultiplier;
        attackTime = DEFAULT_ATTACK_TIME / itemData.attackRateMultiplier;
        attackBoxDuration = DEFAULT_ATTACK_BOX_DURATION / itemData.attackRateMultiplier;

        float previousMaxHealth = this.maxHealthPoints;
        maxHealthPoints = baseMaxHealth * itemData.healthMultiplier;

        currentHealthPoints = Mathf.Clamp(currentHealthPoints, 0, maxHealthPoints);
        if (maxHealthPoints > previousMaxHealth)
            currentHealthPoints += maxHealthPoints - previousMaxHealth;

        UpdateUI();

        if (itemData.damageOverTime)
            ApplyDamageOverTime(itemData.damageChangeTime, itemData.damageCurve);
        else
            attackDamage = baseAttackDamage * itemData.damageMultiplier;
    }

    private void VerifyMusicDrugCombo()
    {
        if (RhythmManager.Instance.musicType == MusicType.RAGGAE && consumedDrug == DrugType.MARIJUANA ||
            RhythmManager.Instance.musicType == MusicType.DnB && consumedDrug == DrugType.COCAINE ||
            RhythmManager.Instance.musicType == MusicType.SYNTHWAVE && consumedDrug == DrugType.LSD ||
            RhythmManager.Instance.musicType == MusicType.BREAKCORE && consumedDrug == DrugType.MDMA)
            musicDrugCombo = true;
        else
            musicDrugCombo = false;
    }

    private void RemoveDebuffs()
    {
        // Se la curva di danno sta abbassando l'attacco sotto il base, la fermiamo
        // così non sovrascrive il fix subito dopo, nello stesso frame.
        if (attackDamage < baseAttackDamage)
        {
            if (activeDamageOverTimeCoroutine != null)
            {
                StopCoroutine(activeDamageOverTimeCoroutine);
                activeDamageOverTimeCoroutine = null;
            }
            attackDamage = baseAttackDamage;
        }

        if (walkingSpeed < baseWalkingSpeed)
        {
            walkingSpeed = baseWalkingSpeed;
        }

        if (attackTime > DEFAULT_ATTACK_TIME)
        {
            attackTime = DEFAULT_ATTACK_TIME;
            attackBoxDuration = DEFAULT_ATTACK_BOX_DURATION;
        }

        float previousMaxHealth = this.maxHealthPoints;
        maxHealthPoints = baseMaxHealth;

        currentHealthPoints = Mathf.Clamp(currentHealthPoints, 0, maxHealthPoints);
        if (maxHealthPoints > previousMaxHealth)
            currentHealthPoints += maxHealthPoints - previousMaxHealth;
        UpdateUI();

        // NOTA: mentalStatus, badTripVolume, badTripInstance, consumedDrug e attackType
        // NON vengono toccati qui: il bad trip resta visivamente/audio attivo,
        // e l'attackType continua a cambiare normalmente tramite ApplyModifiers/ChangeAttackType.
    }

    private void ApplyBadTrip()
    {
        walkingSpeed = baseWalkingSpeed * 0.75f;
        attackTime = DEFAULT_ATTACK_TIME * 2f;
        this.attackDamage = baseAttackDamage * 0.5f;
        PlayBadTripSound();
        badTripVolume.SetActive(true);
    }

    // =========================
    // BAD TRIP SOUND (Evento sempre in play, controllato dal parametro)
    // =========================

    private void PlayBadTripSound()
{
    if (badTripSoundEvent.IsNull) return;

    badTripInstance = FMODUnity.RuntimeManager.CreateInstance(badTripSoundEvent);
    badTripInstance.start();
    // Non rilasciamo subito: ci serve l'istanza per fermarla dopo
}

private void StopBadTripSound()
{
    if (!badTripInstance.isValid()) return;

    badTripInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
    badTripInstance.release();
}
    private void ChangeAttackType(AttackType attackType)
    {
        this.attackType = attackType;
        switch (attackType)
        {
            case AttackType.DEFAULT:
                attackRange = DEFAULT_ATTACK_RANGE;
                attackWidth = DEFAULT_ATTACK_WIDTH;
                break;
            case AttackType.CLAYMORE:
                attackRange = 1.5f * DEFAULT_ATTACK_RANGE;
                attackWidth = 1.25f * DEFAULT_ATTACK_WIDTH;
                break;
            case AttackType.DAGGERS:
                attackRange = 0.5f * DEFAULT_ATTACK_RANGE;
                attackWidth = 2f * DEFAULT_ATTACK_WIDTH;
                break;
            case AttackType.LONGSWORD:
                attackRange = 2f * DEFAULT_ATTACK_RANGE;
                attackWidth = 0.75f * DEFAULT_ATTACK_WIDTH;
                break;
            case AttackType.WHIP:
                attackRange = 2.5f * DEFAULT_ATTACK_RANGE;
                attackWidth = 0.25f * DEFAULT_ATTACK_WIDTH;
                break;
        }
    }

    private Coroutine activeDamageOverTimeCoroutine;

    private void ApplyDamageOverTime(float damageChangeTime, AnimationCurve damageCurve)
    {
        if (activeDamageOverTimeCoroutine != null)
            StopCoroutine(activeDamageOverTimeCoroutine);

        activeDamageOverTimeCoroutine = StartCoroutine(DamageOverTimeRoutine(damageChangeTime, damageCurve));
    }

    private IEnumerator DamageOverTimeRoutine(float damageChangeTime, AnimationCurve damageCurve)
    {
        float elapsedTime = 0f;
        float initialAttackDamage = baseAttackDamage;

        while (elapsedTime <= damageChangeTime)
        {
            attackDamage = initialAttackDamage * damageCurve.Evaluate(elapsedTime / damageChangeTime);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        activeDamageOverTimeCoroutine = null;
    }
    private void DecreaseMusicPointsOverTime(float amount, float interval)
    {
        StartCoroutine(DecreaseMusicPointsRoutine(amount, interval));
    }

    private IEnumerator DecreaseMusicPointsRoutine(float amount, float interval)
    {
        yield return new WaitForSeconds(1.5f);
        while (currentMusicPoints > 0)
        {
            currentMusicPoints = Mathf.Clamp(currentMusicPoints - amount, 0, maxMusicPoints);
            yield return new WaitForSeconds(interval);
        }
        RhythmManager.Instance.SetMusicStyle(MusicType.DEFAULT);
        ChangeSelectedMusicType(MusicType.DEFAULT);
    }
}