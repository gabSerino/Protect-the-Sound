using UnityEngine;
using UnityEngine.AI;

public class EnemyAnimatorScript : MonoBehaviour
{
    [SerializeField] private Animator animator;
    [SerializeField] private NavMeshAgent enemy;

    void Awake()
    {
        if (animator == null)
        {
            animator = GetComponent<Animator>();
        }
        if (enemy == null)
        {
            enemy = GetComponentInParent<NavMeshAgent>();
        }
    }
    //NOTA!! lo sprite ogni tanto si flippa, non so perche, ma non è un problema di animazione
    //penso si flippi proprio lo sprite
    private Vector3 GetDirection()
    {
        Vector3 direction = Vector3.zero;
        if (enemy == null)
        {
            Debug.LogError("Enemy transform is not assigned.");
            return direction;
        }
        direction = enemy.velocity.normalized;
        return direction;   
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 direction = GetDirection();
        direction.y=0f;
        Debug.DrawRay( enemy.transform.position, direction*2f ,Color.red);
        animator.SetFloat("X", direction.x);
        animator.SetFloat("Y", direction.z);
    }
}
