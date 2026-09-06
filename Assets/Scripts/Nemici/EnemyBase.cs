using UnityEngine;
using System.Collections;
using System;
using FMODUnity;

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

    // Suoni
    public FMODUnity.EventReference deathSound;
    public FMODUnity.EventReference defaultSound;

    // Variabile per ricordare com'era il nemico da vivo
    private Sprite originalSprite;
    private FMOD.Studio.EventInstance defaultSoundInstance;
    private bool defaultSoundPlaying;
    private Animator anim;

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
        anim = GetComponentInChildren<Animator>();

    }

    // --- AGGIUNTO PER IL POOLING: Si avvia ogni volta che il nemico "rinasce" ---
    private void OnEnable()
    {
        StopDefaultSound();
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
        PlayDefaultSound();
    }

    private void OnDisable()
    {
        StopDefaultSound();
    }

    private void PlayDefaultSound()
    {
        if (defaultSound.IsNull || defaultSoundPlaying) return;

        defaultSoundInstance = FMODUnity.RuntimeManager.CreateInstance(defaultSound);
        if (!defaultSoundInstance.isValid()) return;

        defaultSoundInstance.set3DAttributes(FMODUnity.RuntimeUtils.To3DAttributes(transform));
        defaultSoundInstance.start();
        defaultSoundPlaying = true;
    }

    private void StopDefaultSound()
    {
        if (!defaultSoundInstance.isValid())
        {
            defaultSoundPlaying = false;
            return;
        }

        defaultSoundInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
        defaultSoundInstance.release();
        defaultSoundInstance = default;
        defaultSoundPlaying = false;
    }

    private void PlayDeathSound()
    {
        FMODUnity.RuntimeManager.PlayOneShot(deathSound);
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
        else anim.SetTrigger("damage");

    }

    private void Die()
    {
        if (IsDead) return;
        IsDead = true;
        StopDefaultSound();
        PlayDeathSound();

        if(TutorialManager.Instance != null) TutorialManager.Instance.RegisterEnemyKilled();

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

    private void OnDestroy()
    {
        StopDefaultSound();
    }

    private void DropLoot()
    {
        if (stats == null || stats.lootTable == null || stats.lootTable.drops.Length == 0 || genericItemPrefab == null) return;
        float value = UnityEngine.Random.value;
        if(TutorialManager.Instance != null) value = stats.dropChance;
        if (value <= stats.dropChance)
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

        anim.SetTrigger("death");
        yield return new WaitForSeconds(deathDelay);

        // --- MODIFICATO PER IL POOLING: Non lo distruggiamo, lo spegniamo! ---
        gameObject.SetActive(false);
    }
}