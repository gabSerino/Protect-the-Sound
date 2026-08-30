using UnityEngine;
using System.Collections;
using System;

public class EnemyBase : MonoBehaviour
{
    [Header("Configurazione")]
    public EnemyStats stats;

    [Header("Loot System")]
    [Tooltip("Trascina qui il tuo prefab 'Item' generico")]
    public GameObject genericItemPrefab;

    [Header("Grafica")]
    [Tooltip("Trascina qui il figlio 'sprite' che contiene lo SpriteRenderer")]
    public SpriteRenderer spriteRenderer;
    public Sprite deadSprite;
    public float deathDelay = 0.5f;

    public float CurrentHealth { get; private set; }
    public bool IsDead { get; private set; }

    public Action<float> OnHealthChanged;

    // EVENTO GLOBALE DI MORTE (usato dal Player per il Level Up)
    public static event Action OnEnemyDied;

    // Variabile per ricordare com'era il nemico da vivo
    private Sprite originalSprite;

    private void Awake()
    {
        if (stats != null)
        {
            CurrentHealth = stats.maxHealth;
        }
        if (spriteRenderer != null)
        {
            originalSprite = spriteRenderer.sprite; // Salviamo lo sprite originale
        }
    }

    // --- AGGIUNTO PER IL POOLING: Si avvia ogni volta che il nemico "rinasce" ---
    private void OnEnable()
    {
        IsDead = false;
        if (stats != null) CurrentHealth = stats.maxHealth;
        OnHealthChanged?.Invoke(CurrentHealth); // Aggiorna eventuale barra della vita

        // Riaccende i collider
        Collider[] colliders = GetComponentsInChildren<Collider>();
        foreach (Collider col in colliders)
        {
            col.enabled = true;
        }

        // Riaccende l'AI
        if (GetComponent<EnemyAI_Brain>() != null) GetComponent<EnemyAI_Brain>().enabled = true;
        if (GetComponent<UnityEngine.AI.NavMeshAgent>() != null) GetComponent<UnityEngine.AI.NavMeshAgent>().enabled = true;

        // Rimette la grafica originale (toglie la grafica da morto)
        if (spriteRenderer != null && originalSprite != null)
        {
            spriteRenderer.sprite = originalSprite;
        }
    }

    public void TakeDamage(float amount)
    {
        if (IsDead) return;

        CurrentHealth -= amount;
        OnHealthChanged?.Invoke(CurrentHealth);

        if (CurrentHealth <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        if (IsDead) return;
        IsDead = true;

        GameOverManager.AggiungiUccisione();
        OnEnemyDied?.Invoke();

        Collider[] colliders = GetComponentsInChildren<Collider>();
        foreach (Collider col in colliders)
        {
            col.enabled = false;
        }

        GetComponent<EnemyAI_Brain>().enabled = false;
        UnityEngine.AI.NavMeshAgent agent = GetComponent<UnityEngine.AI.NavMeshAgent>();
        if (agent != null) agent.enabled = false;

        DropLoot();
        StartCoroutine(DeathSequence());
    }

    private void DropLoot()
    {
        if (stats == null || stats.lootTable == null || stats.lootTable.drops.Length == 0 || genericItemPrefab == null) return;

        if (UnityEngine.Random.value <= stats.dropChance)
        {
            float totalWeight = 0f;
            foreach (LootDrop drop in stats.lootTable.drops) totalWeight += drop.weight;

            float randomVal = UnityEngine.Random.Range(0, totalWeight);
            ItemData itemToDrop = null;

            foreach (LootDrop drop in stats.lootTable.drops)
            {
                if (randomVal <= drop.weight)
                {
                    itemToDrop = drop.item;
                    break;
                }
                randomVal -= drop.weight;
            }

            if (itemToDrop != null)
            {
                GameObject droppedItemObj = Instantiate(genericItemPrefab, transform.position, Quaternion.identity);
                Item itemScript = droppedItemObj.GetComponent<Item>();

                if (itemScript != null) itemScript.Initialize(itemToDrop);
            }
        }
    }

    private IEnumerator DeathSequence()
    {
        if (spriteRenderer != null && deadSprite != null)
            spriteRenderer.sprite = deadSprite;

        yield return new WaitForSeconds(deathDelay);

        // --- MODIFICATO PER IL POOLING: Non lo distruggiamo, lo spegniamo! ---
        gameObject.SetActive(false);
    }
}