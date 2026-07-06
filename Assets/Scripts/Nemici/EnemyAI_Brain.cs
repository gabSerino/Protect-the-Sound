using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent), typeof(EnemyBase))]
public class EnemyAI_Brain : MonoBehaviour
{
    private enum EnemyState { Idle, MovingToCassa, AttackingCassa, PreparingCharge, ChasingPlayer, AttackingPlayer }

    [Header("Stato Corrente (Per Debug)")]
    [SerializeField] private EnemyState currentState;

    [Header("Riferimenti")]
    public LayerMask playerLayer;
    public LayerMask cassaLayer;

    private NavMeshAgent agent;
    private EnemyBase enemyBase;
    private EnemyStats stats;

    private Transform currentTarget;
    private float nextAttackTime;
    private float windupTimer = 0f;


    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        enemyBase = GetComponent<EnemyBase>();
        stats = enemyBase.stats;

        if (stats != null)
        {
            agent.speed = stats.moveSpeed;
            agent.acceleration = stats.acceleration;
            agent.angularSpeed = stats.angularSpeed;
            agent.stoppingDistance = stats.attackRange * 0.8f;
        }

        // Impedisce allo sprite di stortarsi ruotando fisicamente in 3D
        agent.updateRotation = false;
    }

    void Update()
    {
        if (enemyBase.IsDead) return;

        CheckAggro();
        ProcessState();
        UpdateSpriteFacing();
    }

    private void CheckAggro()
    {
        Collider[] hitPlayers = Physics.OverlapSphere(transform.position, stats.aggroRadius, playerLayer);

        if (hitPlayers.Length > 0)
        {
            Transform player = hitPlayers[0].transform;

            if (currentTarget != player)
            {
                currentTarget = player;

                // NUOVO: Invece di correre subito, entra nello stato di preparazione
                currentState = EnemyState.PreparingCharge;
                windupTimer = 0f;

                if (agent != null)
                {
                    agent.isStopped = true; // Tira il freno a mano: si ferma sul posto!
                }
            }
        }
        //  Aggiunto il controllo per interrompere la preparazione se il player scappa troppo in fretta
        else if (currentState == EnemyState.ChasingPlayer || currentState == EnemyState.AttackingPlayer || currentState == EnemyState.PreparingCharge)
        {
            currentTarget = null;
            currentState = EnemyState.Idle;

            if (stats != null && agent != null)
            {
                agent.isStopped = false; // Toglie il freno a mano
                agent.speed = stats.moveSpeed; // Torna calmo
            }
        }
    }

    private void ProcessState()
    {
        switch (currentState)
        {
            case EnemyState.Idle:
                FindNearestCassa();
                break;

            case EnemyState.MovingToCassa:
                if (currentTarget == null) { currentState = EnemyState.Idle; break; }
                agent.SetDestination(currentTarget.position);

                if (Vector3.Distance(transform.position, currentTarget.position) <= stats.attackRange)
                {
                    currentState = EnemyState.AttackingCassa;
                    nextAttackTime = Time.time + stats.firstAttackDelay;
                }
                break;

            case EnemyState.AttackingCassa:
                if (currentTarget == null) { currentState = EnemyState.Idle; break; }
                PerformAttack();

                if (Vector3.Distance(transform.position, currentTarget.position) > stats.attackRange)
                    currentState = EnemyState.MovingToCassa;
                break;

            case EnemyState.ChasingPlayer:
                if (currentTarget == null) { currentState = EnemyState.Idle; break; }
                agent.SetDestination(currentTarget.position);

                if (Vector3.Distance(transform.position, currentTarget.position) <= stats.attackRange)
                {
                    currentState = EnemyState.AttackingPlayer;
                    nextAttackTime = Time.time + stats.firstAttackDelay;
                }
                break;

            case EnemyState.AttackingPlayer:
                if (currentTarget == null) { currentState = EnemyState.Idle; break; }
                PerformAttack();

                if (Vector3.Distance(transform.position, currentTarget.position) > stats.attackRange)
                    currentState = EnemyState.ChasingPlayer;
                break;

            case EnemyState.PreparingCharge:
                if (currentTarget == null) { currentState = EnemyState.Idle; break; }

                windupTimer += Time.deltaTime; // Il tempo scorre...

                // È scaduto il tempo? Parte la vera carica!
                if (windupTimer >= stats.chargeWindupTime)
                {
                    currentState = EnemyState.ChasingPlayer;
                    if (agent != null && stats != null)
                    {
                        agent.isStopped = false; // Toglie il freno a mano
                        agent.speed = stats.chargeSpeed; // Inserisce la marcia alta!
                    }
                }
                break;
        }
    }

    private void FindNearestCassa()
    {
        Collider[] hitCassas = Physics.OverlapSphere(transform.position, 50f, cassaLayer);

        if (hitCassas.Length > 0)
        {
            float closestDistance = Mathf.Infinity;
            Transform bestCassa = null;

            foreach (Collider cassa in hitCassas)
            {
                float dist = Vector3.Distance(transform.position, cassa.transform.position);
                if (dist < closestDistance)
                {
                    closestDistance = dist;
                    bestCassa= cassa.transform;
                }
            }

            currentTarget = bestCassa;
            currentState = EnemyState.MovingToCassa;
        }
    }

    private void PerformAttack()
    {
        if (Time.time >= nextAttackTime)
        {
            nextAttackTime = Time.time + stats.attackCooldown;

            Player playerScript = currentTarget.GetComponentInParent<Player>();
            Health healthScript = currentTarget.GetComponentInParent<Health>();

            if (playerScript != null)
            {
                playerScript.TakeDamage(stats.damage, transform.position, stats.knockbackForce);
            }
            else if (healthScript != null)
            {
                healthScript.TakeDamage(stats.damage);
            }
            else
            {
                DamageReceiver CassaHealth = currentTarget.GetComponentInParent<DamageReceiver>();
                if (CassaHealth != null) CassaHealth.TakeDamage(stats.damage);
            }
        }
    }

    private void UpdateSpriteFacing()
    {
        if (enemyBase.spriteRenderer == null) return;

        if (currentTarget != null)
        {
            enemyBase.spriteRenderer.flipX = currentTarget.position.x < transform.position.x;
        }
        else if (agent.velocity.sqrMagnitude > 0.1f)
        {
            enemyBase.spriteRenderer.flipX = agent.velocity.x < 0;
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (stats == null) return;
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, stats.aggroRadius);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, stats.attackRange);
    }
}