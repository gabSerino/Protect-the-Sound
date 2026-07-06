using UnityEngine;
using System.Collections;

public class EnemySpawner : MonoBehaviour
{
    [Header("Configurazione Spawn Base")]
    public GameObject enemyPrefab;
    public float baseMinSpawnDelay = 0.5f;
    public float baseMaxSpawnDelay = 3.0f;

    [Header("Scaling Difficoltà")]
    [Tooltip("Lascia vuoto per trovare il player in automatico all'avvio")]
    public Player playerReference;

    [Tooltip("Di quanti secondi si riduce l'attesa per ogni livello del player?")]
    public float reductionPerLevel = 0.15f;

    [Tooltip("Il tempo minimo assoluto di spawn (per non far spawnare mille nemici al secondo)")]
    public float absoluteMinSpawnDelay = 0.2f;

    void Start()
    {
        // Se non hai trascinato il Player nell'Inspector, lo cerca da solo
        if (playerReference == null)
        {
            playerReference = FindObjectOfType<Player>();
        }

        StartCoroutine(SpawnRoutine());
    }

    private IEnumerator SpawnRoutine()
    {
        while (true)
        {
            // Legge il livello attuale del giocatore (se non lo trova, usa 1 di default)
            int currentLevel = (playerReference != null) ? playerReference.currentLevel : 1;

            // Calcola la riduzione totale (al livello 1 la riduzione è 0)
            float totalReduction = (currentLevel - 1) * reductionPerLevel;

            // Calcola i nuovi tempi di attesa. 
            // Mathf.Max prende il numero più grande tra i due, impedendo ai delay di scendere sotto il limite assoluto.
            float currentMinDelay = Mathf.Max(absoluteMinSpawnDelay, baseMinSpawnDelay - totalReduction);
            float currentMaxDelay = Mathf.Max(absoluteMinSpawnDelay, baseMaxSpawnDelay - totalReduction);

            // Estrae un tempo casuale con i nuovi parametri calcolati
            float randomDelay = Random.Range(currentMinDelay, currentMaxDelay);

            yield return new WaitForSeconds(randomDelay);

            SpawnEnemy();
        }
    }

    void SpawnEnemy()
    {
        if (enemyPrefab != null)
        {
            Instantiate(enemyPrefab, transform.position, Quaternion.identity);
        }
    }
}