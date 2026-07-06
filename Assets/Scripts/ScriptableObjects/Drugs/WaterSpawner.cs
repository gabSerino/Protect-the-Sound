using UnityEngine;
using System.Collections;

public class WaterSpawner : MonoBehaviour
{
    [Header("Impostazioni Oggetto")]
    [Tooltip("Trascina qui il tuo PREFAB GENERICO (quello chiamato Item)")]
    public GameObject itemPrefab;

    [Tooltip("Trascina qui lo SCRIPTABLE OBJECT dell'Acqua (ItemData)")]
    public ItemData waterData;

    [Header("Impostazioni Spawn")]
    [Tooltip("Ogni quanti secondi deve spawnare una nuova bottiglia d'acqua?")]
    public float spawnInterval = 15f;

    [Header("Area di Spawn")]
    [Tooltip("Se maggiore di 0, l'acqua spawnerà in un punto a caso in questo raggio. Metti 0 per spawnare esattamente al centro.")]
    public float spawnRadius = 2f;

    void Start()
    {
        if (itemPrefab == null || waterData == null)
        {
            Debug.LogWarning("Attenzione: Manca il prefab generico o l'ItemData dell'acqua nel WaterSpawner!");
            return;
        }

        // Avvia il ciclo infinito di spawn
        StartCoroutine(SpawnRoutine());
    }

    private IEnumerator SpawnRoutine()
    {
        while (true)
        {
            // Attende il tempo stabilito
            yield return new WaitForSeconds(spawnInterval);

            // Calcola la posizione
            Vector3 spawnPosition = transform.position;

            // Aggiunge la casualità se il raggio è maggiore di 0
            if (spawnRadius > 0f)
            {
                Vector2 randomCircle = Random.insideUnitCircle * spawnRadius;
                spawnPosition += new Vector3(randomCircle.x, 0f, randomCircle.y);
            }

            // 1. Crea l'oggetto in scena usando il Prefab Generico
            GameObject spawnedItem = Instantiate(itemPrefab, spawnPosition, Quaternion.identity);

            // 2. Prende lo script "Item" appena nato
            Item itemScript = spawnedItem.GetComponent<Item>();

            // 3. Gli inietta i dati dell'acqua, facendolo diventare a tutti gli effetti un'Acqua!
            if (itemScript != null)
            {
                itemScript.Initialize(waterData);
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, spawnRadius);
    }
}