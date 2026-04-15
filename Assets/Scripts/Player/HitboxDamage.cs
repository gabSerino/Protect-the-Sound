using UnityEngine;
using System.Collections.Generic;

public class HitboxDamage : MonoBehaviour
{
    [SerializeField] private BeatTracker beatTracker;
    [SerializeField] private float marginOfError = 0.12f; 
    private List<EnemyAI> hitEnemies = new List<EnemyAI>();

    private void OnEnable() => hitEnemies.Clear();

    private void OnTriggerEnter(Collider other)
    {
        EnemyAI enemy = other.GetComponent<EnemyAI>();
        if (enemy != null && !hitEnemies.Contains(enemy))
        {
            // Verifichiamo se siamo nel margine di tolleranza
            float damage = beatTracker.IsInWindow(marginOfError) ? 1.0f : 0.5f;
            
            enemy.TakeDamage(damage);
            hitEnemies.Add(enemy);

            if(damage > 0.6f) Debug.Log("COLPO RITMICO!");
        }
    }
}