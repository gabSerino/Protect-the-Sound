using UnityEngine;
using System.Collections.Generic;

public class HitboxDamage : MonoBehaviour
{
    [SerializeField] private BeatManager beatManager; // Ora puntiamo al Manager
    private List<EnemyAI> hitEnemies = new List<EnemyAI>();

    private void OnEnable() => hitEnemies.Clear();

    private void OnTriggerEnter(Collider other)
    {
        EnemyAI enemy = other.GetComponent<EnemyAI>();
        if (enemy != null && !hitEnemies.Contains(enemy))
        {
            // Chiediamo al Manager usando la nuova logica dei samples
            bool aTempo = beatManager.IsOnBeat();
            float damage = aTempo ? 1.0f : 0.5f;
            
            enemy.TakeDamage(damage);
            hitEnemies.Add(enemy);

            Debug.Log(aTempo ? "<color=green>♪ A TEMPO! 1.0 ♪</color>" : "<color=yellow>× FUORI TEMPO! 0.5 ×</color>");
        }
    }
}