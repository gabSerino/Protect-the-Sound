using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class EnemySpawner : MonoBehaviour
{
    [Header("Configurazione Spawn Base")]
    public GameObject enemyPrefab;
    public float baseMinSpawnDelay = 1.0f;
    public float baseMaxSpawnDelay = 3.0f;

    [Header("Scaling Difficoltà")]
    public Player playerReference;
    public float timeMultiplierPerLevel = 0.85f;
    public float absoluteMinSpawnDelay = 0.2f;

    [Header("Object Pooling")]
    [Tooltip("Quanti nemici preparare all'avvio del gioco")]
    public int poolSize = 30;

    // La lista che conterrà i nostri nemici pre-caricati
    private List<GameObject> enemyPool;

    void Start()
    {
        if (playerReference == null)
        {
            playerReference = Object.FindFirstObjectByType<Player>();
        }

        InitializePool();
        StartCoroutine(SpawnRoutine());
    }

    // Crea i nemici all'avvio e li disattiva
    void InitializePool()
    {
        enemyPool = new List<GameObject>();
        for (int i = 0; i < poolSize; i++)
        {
            GameObject enemy = Instantiate(enemyPrefab);
            enemy.SetActive(false); // Lo nasconde
            enemyPool.Add(enemy);
        }
    }

    private IEnumerator SpawnRoutine()
    {
        while (true)
        {
            int currentLevel = (playerReference != null) ? playerReference.currentLevel : 1;
            float scaleFactor = Mathf.Pow(timeMultiplierPerLevel, currentLevel - 1);

            float currentMinDelay = Mathf.Max(absoluteMinSpawnDelay, baseMinSpawnDelay * scaleFactor);
            float currentMaxDelay = Mathf.Max(absoluteMinSpawnDelay, baseMaxSpawnDelay * scaleFactor);

            float randomDelay = Random.Range(currentMinDelay, currentMaxDelay);
            yield return new WaitForSeconds(randomDelay);

            SpawnEnemyFromPool();
        }
    }

    void SpawnEnemyFromPool()
    {
        for (int i = 0; i < enemyPool.Count; i++)
        {
            if (!enemyPool[i].activeInHierarchy)
            {
                // Teletrasporto sicuro per la NavMesh
                UnityEngine.AI.NavMeshAgent agent = enemyPool[i].GetComponent<UnityEngine.AI.NavMeshAgent>();
                if (agent != null)
                {
                    agent.Warp(transform.position);
                }
                else
                {
                    enemyPool[i].transform.position = transform.position;
                }

                enemyPool[i].transform.rotation = Quaternion.identity;
                enemyPool[i].SetActive(true);
                return;
            }
        }

        GameObject newEnemy = Instantiate(enemyPrefab, transform.position, Quaternion.identity);
        enemyPool.Add(newEnemy);
    }
}