using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent), typeof(EnemyBase))]
public class EnemyAI_Brain : MonoBehaviour
{
    private enum EnemyState { Idle, MovingToCrate, AttackingCrate, ChasingPlayer, AttackingPlayer }

    [Header("Stato Corrente (Per Debug)")]
    [SerializeField] private EnemyState currentState;

    [Header("Riferimenti")]
    public LayerMask playerLayer;
    public LayerMask crateLayer;

    private NavMeshAgent agent;
    private EnemyBase enemyBase;
    private EnemyStats stats;

    private Transform currentTarget;
    private float lastAttackTime;

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
            // Il player è nel raggio!
            Transform player = hitPlayers[0].transform;

            if (currentTarget != player)
            {
                currentTarget = player;
                currentState = EnemyState.ChasingPlayer;

                // NUOVO: Inserisci la marcia alta (Carica)
                if (stats != null && agent != null)
                {
                    agent.speed = stats.chargeSpeed;
                }
            }
        }
        else if (currentState == EnemyState.ChasingPlayer || currentState == EnemyState.AttackingPlayer)
        {
            // Il player è scappato fuori dal raggio
            currentTarget = null;
            currentState = EnemyState.Idle;

            // NUOVO: Torna a camminare normale
            if (stats != null && agent != null)
            {
                agent.speed = stats.moveSpeed;
            }
        }
    }

    private void ProcessState()
    {
        switch (currentState)
        {
            case EnemyState.Idle:
                FindNearestCrate();
                break;

            case EnemyState.MovingToCrate:
                if (currentTarget == null) { currentState = EnemyState.Idle; break; }
                agent.SetDestination(currentTarget.position);

                if (Vector3.Distance(transform.position, currentTarget.position) <= stats.attackRange)
                    currentState = EnemyState.AttackingCrate;
                break;

            case EnemyState.AttackingCrate:
                if (currentTarget == null) { currentState = EnemyState.Idle; break; }
                PerformAttack();

                if (Vector3.Distance(transform.position, currentTarget.position) > stats.attackRange)
                    currentState = EnemyState.MovingToCrate;
                break;

            case EnemyState.ChasingPlayer:
                if (currentTarget == null) { currentState = EnemyState.Idle; break; }
                agent.SetDestination(currentTarget.position);

                if (Vector3.Distance(transform.position, currentTarget.position) <= stats.attackRange)
                    currentState = EnemyState.AttackingPlayer;
                break;

            case EnemyState.AttackingPlayer:
                if (currentTarget == null) { currentState = EnemyState.Idle; break; }
                PerformAttack();

                if (Vector3.Distance(transform.position, currentTarget.position) > stats.attackRange)
                    currentState = EnemyState.ChasingPlayer;
                break;
        }
    }

    private void FindNearestCrate()
    {
        Collider[] hitCrates = Physics.OverlapSphere(transform.position, 50f, crateLayer);

        if (hitCrates.Length > 0)
        {
            float closestDistance = Mathf.Infinity;
            Transform bestCrate = null;

            foreach (Collider crate in hitCrates)
            {
                float dist = Vector3.Distance(transform.position, crate.transform.position);
                if (dist < closestDistance)
                {
                    closestDistance = dist;
                    bestCrate = crate.transform;
                }
            }

            currentTarget = bestCrate;
            currentState = EnemyState.MovingToCrate;
        }
    }

    private void PerformAttack()
    {
        if (Time.time >= lastAttackTime + stats.attackCooldown)
        {
            lastAttackTime = Time.time;

            // CERCA LO SCRIPT SUL PADRE DEL BERSAGLIO
            Player playerScript = currentTarget.GetComponentInParent<Player>();
            Health healthScript = currentTarget.GetComponentInParent<Health>();

            if (playerScript != null)
            {
                playerScript.TakeDamage(stats.damage);
            }
            else if (healthScript != null)
            {
                healthScript.TakeDamage(stats.damage);
            }
            else
            {
                DamageReceiver crateHealth = currentTarget.GetComponentInParent<DamageReceiver>();
                if (crateHealth != null) crateHealth.TakeDamage(stats.damage);
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