using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System;
using System.Collections.Generic;
using Random = UnityEngine.Random;

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
    private bool canUseInventory = true;

    // Health / Invulnerability
    private CharacterController controller;
    private Renderer[] playerRenderers;
    private bool isInvulnerable = false;

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

        // Cerca i Renderer sul Player e su tutti i figli (Face, Player Capsule, ecc.)
        playerRenderers = GetComponentsInChildren<Renderer>();

        if (playerRenderers == null || playerRenderers.Length == 0)
            Debug.LogError("Attenzione: Nessun Renderer trovato sul Player o nei suoi figli! Il lampeggio non funzionerà.");
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
        HandleMovement();
        HandleRotation();
        HandleAttack();
        HandleInventory();

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
        playerInputManager.ConsumeAttackInput();
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

        // Ripristino finale di tutti i Renderer
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
            UpdateInventoryUI(); // <-- AGGIUNTO: Aggiorna la cornice visiva
            playerInputManager.ConsumeGoLeftInventorySlotInput();
        }
        if (playerInputManager.GoRightInventorySlotInput)
        {
            GoRightInventorySlot();
            UpdateInventoryUI(); // <-- AGGIUNTO: Aggiorna la cornice visiva
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
                UpdateInventoryUI(); // <-- AGGIUNTO: Spegne l'icona dell'oggetto consumato
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
            // Passiamo sia l'inventario che l'attackType attuale del Player
            inventoryUI.RefreshUI(inventory, attackType);
        }
    }

    // =========================
    // ITEMS
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

    public void UseItem(ItemData itemData)    // Applica moltiplicatori su stato corrente
    {
        if (itemData == null) return;
        ApplyMultipliers(itemData);
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
        return Convert.ToBoolean(Random.Range(0, 100) < badTripChance*100);
    }

    private void ApplyMentalStatus(PlayerMentalStatus mentalStatus)
    {
        this.mentalStatus = mentalStatus;
    }

    private void ApplyMultipliers(ItemData itemData)    // Applica moltiplicatori su stato default
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

    private void ApplyDamageOverTime(float damageChangeTime, AnimationCurve damageCurve)
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