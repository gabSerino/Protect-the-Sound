using UnityEngine;
using System.Collections.Generic;

public class HitboxDamage : MonoBehaviour
{
    private List<EnemyAI> hitEnemies = new List<EnemyAI>();

    [Header("Timing")]
    public float perfectWindow = 0.1f;
    public float goodWindow = 0.25f;

    private void OnEnable() => hitEnemies.Clear();

    private void OnTriggerEnter(Collider other)
    {
        EnemyAI enemy = other.GetComponent<EnemyAI>();

        if (enemy != null && !hitEnemies.Contains(enemy))
        {
            bool aTempo = IsOnBeat(out float multiplier);

            enemy.TakeDamage(multiplier);
            hitEnemies.Add(enemy);

            Debug.Log(aTempo 
                ? $"<color=green>♪ A TEMPO! x{multiplier} ♪</color>" 
                : $"<color=yellow>× FUORI TEMPO! x{multiplier} ×</color>");
        }
    }

    bool IsOnBeat(out float damageMultiplier)
    {
        if (RhythmManager.Instance == null)
        {
            damageMultiplier = 0.5f;
            return false;
        }

        return RhythmManager.Instance.IsOnBeat(perfectWindow, goodWindow, out damageMultiplier);
    }
}