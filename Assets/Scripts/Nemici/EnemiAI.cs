using UnityEngine;
using UnityEngine.AI;
using System.Collections;

[RequireComponent(typeof(NavMeshAgent))]
public class EnemyAI : MonoBehaviour
{
    private NavMeshAgent agent;
    private GameObject currentTarget;

    [Header("Salute")]
    [SerializeField] private float health = 4f;

    [Header("Grafica e Morte")]
    [SerializeField] private SpriteRenderer spriteRenderer; 
    [SerializeField] private Sprite aliveSprite;           
    [SerializeField] private Sprite deadSprite;            
    [SerializeField] private float deathDelay = 0.5f;      

    [Header("Movimento")]
    public float moveSpeed = 3.5f;
    public float acceleration = 8f;
    public float angularSpeed = 120f;

    [Header("Configurazioni Attacco")]
    public float detectionRadius = 10f;
    public float damageToTarget = 10f;
    public float stopDistance = 1.5f;
    public float targetUpdateInterval = 0.3f;
    public float attackCooldown = 1.0f; 

    private float stopDistanceSqr;
    private float detectionRadiusSqr;
    private float lastAttackTime;
    private bool isDead = false;

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();

        // Configurazione NavMeshAgent
        agent.speed = moveSpeed;
        agent.acceleration = acceleration;
        agent.angularSpeed = angularSpeed;
        agent.stoppingDistance = stopDistance * 0.9f;

        stopDistanceSqr = stopDistance * stopDistance;
        detectionRadiusSqr = detectionRadius * detectionRadius;

        // Imposta la sprite iniziale
        if (spriteRenderer != null && aliveSprite != null)
        {
            spriteRenderer.sprite = aliveSprite;
        }
    }

    void Start()
    {
        StartCoroutine(UpdateLogicRoutine());
    }

    public void TakeDamage(float amount)
    {
        if (isDead) return;

        health -= amount;
        Debug.Log($"{gameObject.name} ha ricevuto {amount} danni. Vita: {health}");

        if (health <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        if (isDead) return;
        isDead = true;

        Debug.Log($"{gameObject.name} è stato eliminato.");

        // 1. Ferma l'intelligenza e il movimento
        if (agent != null) agent.enabled = false; 

        Collider col = GetComponent<Collider>();
        if (col != null) col.enabled = false;

        // 2. Avvia la sparizione visiva ritardata
        StartCoroutine(DeathSequence());
    }

    private IEnumerator DeathSequence()
    {
        // Cambia la sprite in quella di morte
        if (spriteRenderer != null && deadSprite != null)
        {
            spriteRenderer.sprite = deadSprite;
        }

        // Aspetta il tempo stabilito
        yield return new WaitForSeconds(deathDelay);

        // Rimuove definitivamente l'oggetto
        Destroy(gameObject);
    }

    IEnumerator UpdateLogicRoutine()
    {
        while (!isDead)
        {
            if (agent != null && agent.enabled && agent.isOnNavMesh)
            {
                DecideTarget();

                if (currentTarget != null)
                {
                    agent.SetDestination(currentTarget.transform.position);
                }
                else
                {
                    agent.ResetPath();
                }
            }
            yield return new WaitForSeconds(targetUpdateInterval);
        }
    }

    void Update()
    {
        if (isDead || currentTarget == null || !agent.isOnNavMesh) return;

        float distSqr = (currentTarget.transform.position - transform.position).sqrMagnitude;

        if (distSqr < stopDistanceSqr && Time.time >= lastAttackTime + attackCooldown)
        {
            Attack();
        }
    }

    void DecideTarget()
    {
        // Priorità 1: Player
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        GameObject bestTarget = null;
        float closestPlayerDistSqr = detectionRadiusSqr;

        foreach (GameObject p in players)
        {
            float distSqr = (p.transform.position - transform.position).sqrMagnitude;
            if (distSqr < closestPlayerDistSqr)
            {
                closestPlayerDistSqr = distSqr;
                bestTarget = p;
            }
        }

        if (bestTarget != null)
        {
            currentTarget = bestTarget;
            return;
        }

        // Priorità 2: Casse
        GameObject[] casse = GameObject.FindGameObjectsWithTag("provacassa");
        float closestCassaDistSqr = Mathf.Infinity;

        foreach (GameObject c in casse)
        {
            float distSqr = (c.transform.position - transform.position).sqrMagnitude;
            if (distSqr < closestCassaDistSqr)
            {
                closestCassaDistSqr = distSqr;
                bestTarget = c;
            }
        }

        currentTarget = bestTarget;
    }

    void Attack()
    {
        lastAttackTime = Time.time;
        Debug.Log($"{gameObject.name} sta attaccando {currentTarget.name}!");

        // Applica danno preferendo lo script Health del Player
        Health targetHealth = currentTarget.GetComponent<Health>();
        if (targetHealth != null)
        {
            targetHealth.TakeDamage(damageToTarget);
        }
        else
        {
            // Fallback per altri tipi di bersagli (es. casse)
            DamageReceiver sharedTarget = currentTarget.GetComponent<DamageReceiver>();
            if (sharedTarget != null) sharedTarget.TakeDamage(damageToTarget);
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, stopDistance);
    }
}