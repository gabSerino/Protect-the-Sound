using UnityEngine;
using System;
using System.Collections.Generic;

public class HitboxDamage : MonoBehaviour
{
    private List<EnemyAI> hitEnemies = new List<EnemyAI>();
    private Color[] hitboxColors; // 0 = normal, 1 = good, 2 = perfect
    private Renderer myRenderer;

    private void OnEnable() => hitEnemies.Clear();
    private float damage = 0f;

    void Awake()
    {
        // Cache del renderer per ottimizzare le prestazioni
        myRenderer = GetComponent<Renderer>();

        hitboxColors = new Color[3];
        hitboxColors[0] = Color.red;    // Normal
        hitboxColors[1] = Color.yellow; // Good
        hitboxColors[2] = Color.green;  // Perfect
    }

    private void OnTriggerEnter(Collider other)
    {
        EnemyAI enemy = other.GetComponent<EnemyAI>();
        if (enemy == null) return;
        if (hitEnemies.Contains(enemy)) return;

        enemy.TakeDamage(damage);
        KnockbackEnemy(enemy, 20f);
        hitEnemies.Add(enemy);
    }

    public void SetHitboxDamage(float newDamage, float multiplier)
    {
        damage = newDamage;

        if (RhythmManager.Instance == null || myRenderer == null) return;

        int index = 0;
        if (Mathf.Approximately(multiplier, RhythmManager.Instance.goodMultiplier))
        {
            index = 1;
        }
        else if (Mathf.Approximately(multiplier, RhythmManager.Instance.perfectMultiplier))
        {
            index = 2;
        }
        // Se non è né good né perfect, resta 0 (Normal)

        // Cambia il colore immediatamente
        if (index >= 0 && index < hitboxColors.Length)
        {
            myRenderer.material.color = hitboxColors[index];
        }
    }

    private void KnockbackEnemy(EnemyAI enemy, float force)
    {
        Vector3 direction = (enemy.transform.position - transform.position).normalized;
        enemy.transform.position += direction * force * Time.deltaTime;
    }
}