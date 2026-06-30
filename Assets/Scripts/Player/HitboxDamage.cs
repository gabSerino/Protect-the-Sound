using UnityEngine;
using System.Collections.Generic;

public class HitboxDamage : MonoBehaviour
{
    private List<EnemyBase> hitEnemies = new List<EnemyBase>();
    private Color[] hitboxColors;
    private Renderer myRenderer;

    private float damage = 0f;
    private bool attaccoATempo = false; // Memoria per capire se hai colpito a tempo

    private void OnEnable() => hitEnemies.Clear();

    void Awake()
    {
        myRenderer = GetComponent<Renderer>();

        hitboxColors = new Color[3];
        hitboxColors[0] = Color.red;    // Normal
        hitboxColors[1] = Color.yellow; // Good
        hitboxColors[2] = Color.green;  // Perfect
    }

    // ECCO LA FUNZIONE ONTRIGGERENTER:
    private void OnTriggerEnter(Collider other)
    {
        // Trova il nemico (leggendo lo script sul Padre)
        EnemyBase enemy = other.GetComponentInParent<EnemyBase>();

        if (enemy == null) return;
        if (hitEnemies.Contains(enemy)) return;

        // Infligge danno e spinge indietro
        enemy.TakeDamage(damage);
        KnockbackEnemy(enemy, 20f);
        hitEnemies.Add(enemy);

        // Se hai colpito a tempo, invia il segnale all'interfaccia UI!
        if (attaccoATempo && ComboMeterUI.Instance != null)
        {
            ComboMeterUI.Instance.AggiungiCombo(10f); // Riempe la barra di 10 punti
        }
    }

    public void SetHitboxDamage(float newDamage, float multiplier)
    {
        damage = newDamage;

        if (RhythmManager.Instance == null || myRenderer == null) return;

        int index = 0;
        attaccoATempo = false; // Di base, l'attacco non è a tempo

        // Controlla se il colpo era a tempo
        if (Mathf.Approximately(multiplier, RhythmManager.Instance.goodMultiplier))
        {
            index = 1;

        }
        else if (Mathf.Approximately(multiplier, RhythmManager.Instance.perfectMultiplier))
        {
            index = 2;
            attaccoATempo = true;
        }

        // Cambia il colore della hitbox
        if (index >= 0 && index < hitboxColors.Length)
        {
            myRenderer.material.color = hitboxColors[index];
        }
    }

    private void KnockbackEnemy(EnemyBase enemy, float force)
    {
        // 1. Calcola la direzione ignorando totalmente l'asse Y (l'altezza)
        Vector3 direction = (enemy.transform.position - transform.position);
        direction.y = 0f; // IL SEGRETO È QUI: lo blocca al suolo!
        direction.Normalize();

        // 2. Se il nemico usa il NavMesh, usa il comando ufficiale Move per evitare bug di compenetrazione
        UnityEngine.AI.NavMeshAgent agent = enemy.GetComponent<UnityEngine.AI.NavMeshAgent>();

        if (agent != null && agent.isActiveAndEnabled)
        {
            // Il NavMesh assicura che il nemico indietreggi strisciando sul pavimento
            agent.Move(direction * force * Time.deltaTime);
        }
        else
        {
            // Fallback nel caso il NavMesh fosse spento
            enemy.transform.position += direction * force * Time.deltaTime;
        }
    }
}