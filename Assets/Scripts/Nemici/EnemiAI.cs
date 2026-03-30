using UnityEngine;
using UnityEngine.AI;
using System.Collections;

[RequireComponent(typeof(NavMeshAgent))] // Garantisce che l'oggetto abbia l'agente
public class EnemyAI : MonoBehaviour
{
    private NavMeshAgent agent;
    private GameObject currentTarget;

    [Header("Movimento")]
    public float moveSpeed = 3.5f;
    public float acceleration = 8f;
    public float angularSpeed = 120f;

    [Header("Configurazioni Attacco")]
    public float detectionRadius = 10f;
    public float damageToTarget = 10f;
    public float stopDistance = 1.5f;
    public float targetUpdateInterval = 0.3f;

    private float stopDistanceSqr;
    private float detectionRadiusSqr;

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();

        // Applichiamo i parametri di movimento all'agente
        agent.speed = moveSpeed;
        agent.acceleration = acceleration;
        agent.angularSpeed = angularSpeed;
        agent.stoppingDistance = stopDistance * 0.9f;

        stopDistanceSqr = stopDistance * stopDistance;
        detectionRadiusSqr = detectionRadius * detectionRadius;
    }

    void Start()
    {
        StartCoroutine(UpdateLogicRoutine());
    }

    IEnumerator UpdateLogicRoutine()
    {
        while (true)
        {
            if (agent != null && agent.enabled && agent.isOnNavMesh)
            {
                DecideTarget();

                // OTTIMIZZAZIONE: Impostiamo la destinazione solo qui, non ogni frame
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
        // Nell'Update lasciamo solo il controllo della distanza (molto leggero)
        if (currentTarget == null || !agent.isOnNavMesh) return;

        float distSqr = (currentTarget.transform.position - transform.position).sqrMagnitude;

        if (distSqr < stopDistanceSqr)
        {
            Attack();
        }
    }

    void DecideTarget()
    {
        // Ricerca Player (Priorità)
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

        // Ricerca Casse (Secondaria)
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
        // Logica danno
        DamageReceiver sharedTarget = currentTarget.GetComponent<DamageReceiver>();
        if (sharedTarget != null)
        {
            sharedTarget.TakeDamage(damageToTarget);
        }
        else
        {
            Health targetHealth = currentTarget.GetComponent<Health>();
            if (targetHealth != null) targetHealth.TakeDamage(damageToTarget);
        }

        // Sparisce dopo l'attacco
        Debug.Log($"{gameObject.name} ha colpito {currentTarget.name} e si è sacrificato.");
        Destroy(gameObject);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, stopDistance);
    }
}