using UnityEngine;
using System.Collections; 

public class EnemySpawner : MonoBehaviour
{
    [Header("Configurazione Spawn")]
    public GameObject enemyPrefab;
    public float minSpawnDelay = 0.5f;
    public float maxSpawnDelay = 3.0f;

    void Start()
    {
        StartCoroutine(SpawnRoutine());
    }

    private IEnumerator SpawnRoutine()
    {
        while (true)
        {
            float randomDelay = Random.Range(minSpawnDelay, maxSpawnDelay);
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