using UnityEngine;
using UnityEngine.AI;

public class EnemyAI : MonoBehaviour
{
    private NavMeshAgent agent;

    private GameObject cassaTarget;
    private GameObject playerTarget;

    [Header("Configurazioni")]
    public float detectionRadius = 10f;
    public float damageToTarget = 10f; // Quanto danno fa il nemico
    private float stopDistanceSqr = 2.25f; // Distanza di contatto (1.5m al quadrato)

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();

        // Riferimenti ai bersagli
        cassaTarget = GameObject.Find("provacassa");
        playerTarget = GameObject.FindGameObjectWithTag("Player");

        if (cassaTarget == null)
            Debug.LogWarning("Attenzione: 'provacassa' non trovata in scena!");

        // Impostazione iniziale verso la cassa
        if (cassaTarget != null)
            agent.SetDestination(cassaTarget.transform.position);
    }

    void Update()
    {
        // Se non c'è l'agente o non ci sono più bersagli, non fare nulla
        if (agent == null || !agent.enabled || !agent.isOnNavMesh) return;
        if (cassaTarget == null && playerTarget == null) return;

        // Determina chi inseguire
        GameObject currentTarget = DecideTarget();

        if (currentTarget != null)
        {
            // Aggiorna la destinazione verso il target attuale
            agent.SetDestination(currentTarget.transform.position);

            // Controllo distanza per il contatto
            float distSqr = (currentTarget.transform.position - transform.position).sqrMagnitude;

            if (distSqr < stopDistanceSqr)
            {
                // Applica il danno se il target ha lo script Health
                Health targetHealth = currentTarget.GetComponent<Health>();
                if (targetHealth != null)
                {
                    targetHealth.TakeDamage(damageToTarget);
                }

                // Il nemico sparisce dopo l'attacco
                Destroy(gameObject);
            }
        }
    }

    GameObject DecideTarget()
    {
        // Se uno dei due non esiste più, punta all'altro
        if (playerTarget == null) return cassaTarget;
        if (cassaTarget == null) return playerTarget;

        float distanceToPlayer = Vector3.Distance(transform.position, playerTarget.transform.position);
        float distanceToCassa = Vector3.Distance(transform.position, cassaTarget.transform.position);

        // PRIORITÀ ALLA CASSA: 
        // Insegue il Player solo se è nel raggio E se è più vicino della cassa
        if (distanceToPlayer < detectionRadius && distanceToPlayer < distanceToCassa)
        {
            return playerTarget;
        }

        // Altrimenti l'obiettivo resta la cassa
        return cassaTarget;
    }

    private void OnDrawGizmosSelected()
    {
        // Disegna il raggio di rilevamento nell'Editor (cerchio giallo)
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
    }
}