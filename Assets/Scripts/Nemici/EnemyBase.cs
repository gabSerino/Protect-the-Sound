using UnityEngine;
using System.Collections;

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

    public System.Action<float> OnHealthChanged;

    private void Awake()
    {
        if (stats != null)
        {
            CurrentHealth = stats.maxHealth;
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

        // Disabilita tutti i collider (sia sul padre che sulla Capsula figlia)
        Collider[] colliders = GetComponentsInChildren<Collider>();
        foreach (Collider col in colliders)
        {
            col.enabled = false;
        }

        // Spegne l'AI e il movimento
        GetComponent<EnemyAI_Brain>().enabled = false;
        UnityEngine.AI.NavMeshAgent agent = GetComponent<UnityEngine.AI.NavMeshAgent>();
        if (agent != null) agent.enabled = false;

        DropLoot();

        StartCoroutine(DeathSequence());
    }

    private void DropLoot()
    {
        // 1. Controlli di sicurezza aggiornati (verifica che la Loot Table non sia vuota)
        if (stats == null || stats.lootTable == null || stats.lootTable.drops.Length == 0 || genericItemPrefab == null) return;

        // 2. Lancio della moneta per decidere SE droppare qualcosa
        if (Random.value <= stats.dropChance)
        {
            float totalWeight = 0f;

            // 3. Legge i drop dalla Loot Table
            foreach (LootDrop drop in stats.lootTable.drops)
            {
                totalWeight += drop.weight;
            }

            float randomVal = Random.Range(0, totalWeight);
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

            // 4. Istanzia l'oggetto
            if (itemToDrop != null)
            {
                GameObject droppedItemObj = Instantiate(genericItemPrefab, transform.position, Quaternion.identity);
                Item itemScript = droppedItemObj.GetComponent<Item>();

                if (itemScript != null)
                {
                    itemScript.Initialize(itemToDrop);
                }

                Debug.Log($"{gameObject.name} ha droppato {itemToDrop.displayName} usando la Loot Table!");
            }
        }
    }

    private IEnumerator DeathSequence()
    {
        if (spriteRenderer != null && deadSprite != null)
            spriteRenderer.sprite = deadSprite;

        yield return new WaitForSeconds(deathDelay);
        Destroy(gameObject);
    }
}