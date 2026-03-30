using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    public GameObject enemyPrefab; // Trascina qui il Prefab del nemico
    public float spawnRate = 2f;   // Secondi tra uno spawn e l'altro

    void Start()
    {
        // Avvia la generazione ripetuta
        InvokeRepeating("SpawnEnemy", 0f, spawnRate);
    }

    void SpawnEnemy()
    {
        Instantiate(enemyPrefab, transform.position, Quaternion.identity);
    }
}