using UnityEngine;
using System;
using System.Collections.Generic;

public class HitboxDamage : MonoBehaviour
{
    private List<EnemyAI> hitEnemies = new List<EnemyAI>();
    private Color[] hitboxColors; // 0 = normal, 1 = good, 2 = perfect
    private void OnEnable() => hitEnemies.Clear();
    private float damage = 0f;
    private float currentDamage;

    void Start()
    {
        hitboxColors = new Color[3];
        hitboxColors[0] = Color.red; // Normal
        hitboxColors[1] = Color.yellow; // Good
        hitboxColors[2] = Color.green; // Perfect
        currentDamage = damage;
    }

    void Update()
    {
        if (currentDamage != damage)
        {
            currentDamage = damage;
            int index = Array.IndexOf(new float[] { RhythmManager.Instance.normalMultiplier, RhythmManager.Instance.goodMultiplier, RhythmManager.Instance.perfectMultiplier }, damage);
            if (index >= 0 && index < hitboxColors.Length)
            {
                GetComponent<Renderer>().material.color = hitboxColors[index];
            }
        }
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
    public void SetHitboxDamage(float newDamage)
    {
        damage = newDamage;
    }

    private void KnockbackEnemy(EnemyAI enemy, float force)
    {
        Vector3 direction = (enemy.transform.position - transform.position).normalized;
        enemy.transform.position += direction * force * Time.deltaTime;
    }
}