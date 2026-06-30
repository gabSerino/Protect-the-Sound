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

    [Header("Movement Settings")]
    [SerializeField] private float walkingSpeed = 8f;
    //[SerializeField] private float rotationSpeed = 10f;
    [SerializeField] private float acceleration = 100f;
    [SerializeField] private float deceleration = 200f;

    [Header("Dash Settings")]
    [SerializeField] private float dashSpeed = 25f;
    [SerializeField] private float dashDuration = 0.2f;
    [SerializeField] private float dashCooldown = 1f;
    [SerializeField] private float dashInvincibilityTime = 0.3f; 

    private bool isDashing = false;
    private bool canDash = true;

    [Header("Dodge Settings")]
    [SerializeField] private float dodgeSpeed = 30f;
    [SerializeField] private float dodgeDuration = 0.2f;
    [SerializeField] private float dodgeCooldown = 1f;
    [SerializeField] private float dodgeInvincibilityTime = 0.25f; 

    private bool isDodging = false;
    private bool canDodge = true;

    private Vector2 virtualAimPosition = Vector2.zero;

    [Header("Virtual Mouse Settings")]
    [SerializeField] private float mouseSensitivity = 1f;
    [SerializeField] private float maxAimRadius = 200f; // Il raggio massimo (in pixel) della tua scatola invisibile
    [SerializeField] private float minAimDeadzone = 20f;  // Per evitare micro-sfarfallii al centro

    [Header("Attack Settings")]
    [SerializeField] private AttackType attackType = AttackType.DEFAULT;
    [SerializeField] private float attackRange = 2f;
    [SerializeField] private float attackWidth = 1f;
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

    [Header("Mental Status Settings")]
    private PlayerMentalStatus mentalStatus = PlayerMentalStatus.DEFAULT;

    [Header("Invulnerability Settings")]
    [SerializeField] private float invulnerabilityDuration = 2f;
    [SerializeField] private float flickerInterval = 0.1f;

    [Header("Inventory Settings")]
    [SerializeField] private int inventorySize = 3;
    [SerializeField] private float itemUseDelay = 0.5f;

    [Header("UI")]
    public Slider healthSlider;
    public UIJuice uiJuice;
    public InventoryUI inventoryUI;

    [Header("References")]
    [SerializeField] private PlayerInputManager playerInputManager;
    [SerializeField] private CharacterController characterController;
    [SerializeField] private Camera gameCamera;
    [SerializeField] private GameObject playerCapsule;
    [SerializeField] private GameObject attackHitbox;

    [Header("Grafica Player (Per il Lampeggio)")]
    [SerializeField] private Renderer[] playerRenderers;

    // DEFAULT STATS
    private const float DEFAULT_WALKING_SPEED = 8f;
    private const float DEFAULT_ROTATION_SPEED = 10f;
    private const float DEFAULT_ACCELERATION = 100f;
    private const float DEFAULT_DECELERATION = 200f;
    private const float DEFAULT_ATTACK_RANGE = 2f;
    private const float DEFAULT_ATTACK_WIDTH = 1f;
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
    private Renderer hitboxRenderer;
    private Collider hitboxCollider;

    // Controller flags
    private bool canMove = true;
    private bool canAttack = true;
    private bool isAttacking = false;
    private bool canUseInventory = true;

    // Health / Invulnerability
    private CharacterController controller;
    private bool isInvulnerable = false;
    private Coroutine activeInvincibilityCoroutine;
    // Inventory
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

        if (playerRenderers == null || playerRenderers.Length == 0)
            Debug.LogWarning("Attenzione: Nessun Renderer trovato sul Player o nei suoi figli! Il lampeggio non funzionerà.");
    }

    void Start()
    {
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
        HandleDodge();
        HandleDash();

        // Se non sta schivando e non sta scattando, muoviti normalmente
        if (!isDodging && !isDashing)
        {
            HandleMovement();
            HandleRotation();
        }

        HandleAttack();
        HandleInventory();

        attackHitbox.transform.position = playerCapsule.transform.position + playerCapsule.transform.forward * attackRange / 2f;
        attackHitbox.transform.localScale = new Vector3(attackWidth, 1f, attackRange);

        if (transform.position.y > 0.05f)
        {
            Vector3 bloccatoAlSuolo = transform.position;
            bloccatoAlSuolo.y = 0f; // Lo inchioda a terra!
            transform.position = bloccatoAlSuolo;
        }
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
        // Se c'è già un'altra invincibilità in corso (es. hai appena schivato e ora scatti), la fermiamo
        if (activeInvincibilityCoroutine != null)
        {
            StopCoroutine(activeInvincibilityCoroutine);
        }

        // Avviamo la nuova coroutine e la salviamo nella variabile
        activeInvincibilityCoroutine = StartCoroutine(TemporaryInvincibilityRoutine(time));
    }

    private IEnumerator TemporaryInvincibilityRoutine(float time)
    {
        isInvulnerable = true;

        yield return new WaitForSeconds(time);

        isInvulnerable = false;
        activeInvincibilityCoroutine = null; // Svuotiamo la variabile quando ha finito
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
    }

    private void HandleRotation()
    {
        Vector3 targetDirection = Vector3.zero;

        Vector2 mouseDelta = playerInputManager.AttackDirectionInput;
        virtualAimPosition += mouseDelta * mouseSensitivity;

        if (virtualAimPosition.magnitude > maxAimRadius)
        {
            virtualAimPosition = virtualAimPosition.normalized * maxAimRadius;
        }

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

        if (targetDirection.sqrMagnitude < 0.01f) return;

        Vector3 snappedDirection = SnapTo8Directions(targetDirection.normalized);
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

    // =========================
    // DASH E DODGE
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

        // RICHIAMO LA NUOVA FUNZIONE!
        GrantTemporaryInvincibility(dashInvincibilityTime);

        Vector3 dashDirection = (currentMovement.sqrMagnitude > 0.1f) ? currentMovement.normalized : transform.forward;
        dashDirection.y = 0f;

        float startTime = Time.time;

        while (Time.time < startTime + dashDuration)
        {
            characterController.Move(dashDirection * dashSpeed * Time.deltaTime);
            yield return null;
        }

        isDashing = false;

        yield return new WaitForSeconds(dashCooldown);
        canDash = true;
    }

    private void HandleDodge()
    {
        if (!canMove || !canDodge || isAttacking || isDashing) return;

        if (playerInputManager.DodgeInput)
        {
            StartCoroutine(DodgeRoutine());
            playerInputManager.ConsumeDodgeInput();
        }
    }

    private IEnumerator DodgeRoutine()
    {
        canDodge = false;
        isDodging = true;

        GrantTemporaryInvincibility(dodgeInvincibilityTime);

        Vector3 direction = (currentMovement.sqrMagnitude > 0.1f)
            ? currentMovement.normalized
            : -transform.forward;

        direction.y = 0f;

        float startTime = Time.time;

        while (Time.time < startTime + dodgeDuration)
        {
            characterController.Move(direction * dodgeSpeed * Time.deltaTime);
            yield return null;
        }

        isDodging = false;

        yield return new WaitForSeconds(dodgeCooldown);
        canDodge = true;
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
        if (isInvulnerable) return;

        currentHealthPoints = Mathf.Clamp(currentHealthPoints - amount, 0, maxHealthPoints);
        UpdateUI();

        if (uiJuice != null) uiJuice.Shake();

        if (currentHealthPoints <= 0)
            Respawn();
    }

    private void Respawn()
    {
        ResetHealth();

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
        bool currentlyVisible = true;

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
                UseItem(itemToUse);
                RemoveItem(inventory.GetSelectedIndex());
                inventory.SortItems();
                UpdateInventoryUI();
                DelayAfterItemUse(itemUseDelay);
            }
            playerInputManager.ConsumeUseItemInput();
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

    public void AddItem(ItemData item)
    {
        inventory.AddItemInHead(item);
        UpdateInventoryUI();
    }

    public void AddItem(ItemData item, int index)
    {
        inventory.AddItem(item, index);
        UpdateInventoryUI();
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

    public void UseItem(ItemData itemData)
    {
        if (itemData == null) return;
        ApplyModifiers(itemData);
        switch (itemData.itemType)
        {
            case ItemType.WATER:
                ApplyMentalStatus(PlayerMentalStatus.DEFAULT);
                break;
            case ItemType.DRUG:
                if (IsGettingBadTrip(itemData.badTripChance))
                    ApplyMentalStatus(PlayerMentalStatus.BADTRIP);
                else
                    ApplyMentalStatus(PlayerMentalStatus.STUNNED);
                break;
        }
    }

    private bool IsGettingBadTrip(float badTripChance)
    {
        return Convert.ToBoolean(Random.Range(0, 100) < badTripChance * 100);
    }

    private void ApplyMentalStatus(PlayerMentalStatus mentalStatus)
    {
        this.mentalStatus = mentalStatus;
    }

    private void ApplyModifiers(ItemData itemData)
    {
        walkingSpeed = DEFAULT_WALKING_SPEED * itemData.speedMultiplier;
        attackTime = DEFAULT_ATTACK_TIME / itemData.attackRateMultiplier;
        ChangeAttackType(itemData.attackType);

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

    private void ApplyDamageOverTime(float damageChangeTime, AnimationCurve damageCurve)
    {
        StartCoroutine(DamageOverTimeRoutine(damageChangeTime, damageCurve));
    }

    private IEnumerator DamageOverTimeRoutine(float damageChangeTime, AnimationCurve damageCurve)
    {
        float elapsedTime = 0f;
        float initialAttackDamage = DEFAULT_ATTACK_DAMAGE;

        while (elapsedTime <= damageChangeTime)
        {
            attackDamage = initialAttackDamage * damageCurve.Evaluate(elapsedTime / damageChangeTime);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
    }
}