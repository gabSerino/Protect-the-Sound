using UnityEngine;
using System.Collections.Generic;
using Unity.VisualScripting;

public class HitboxDamage : MonoBehaviour
{
    [Header("Audio")]
    [SerializeField] private FMODUnity.EventReference hitSoundEvent = new FMODUnity.EventReference();
    [SerializeField] private string timingParameter = "Timing";
    [SerializeField] private float knockbackForce = 20f;
    private Player player;

    private List<EnemyBase> hitEnemies = new List<EnemyBase>();
    private Color[] hitboxColors;
    private Renderer myRenderer;
    private float damage = 0f;
    private bool attaccoATempo = false; // Memoria per capire se hai colpito a tempo
    private int timingIndex = 0;        // 0 = Normal, 1 = Good, 2 = Perfect

    private void OnEnable() => hitEnemies.Clear();

    void Awake()
    {
        player = GetComponentInParent<Player>();
        myRenderer = GetComponent<Renderer>();
        hitboxColors = new Color[3];
        hitboxColors[0] = Color.red;    // Normal
        hitboxColors[1] = Color.yellow; // Good
        hitboxColors[2] = Color.green;  // Perfect
    }

    private void OnTriggerEnter(Collider other)
    {
        // Trova il nemico (leggendo lo script sul Padre)
        EnemyBase enemy = other.GetComponentInParent<EnemyBase>();
        if (enemy == null) return;
        if (hitEnemies.Contains(enemy)) return;

        // Infligge danno e spinge indietro
        enemy.TakeDamage(damage);
        KnockbackEnemy(enemy, knockbackForce);
        hitEnemies.Add(enemy);

        PlayHitSound(other.ClosestPoint(transform.position));

        // Se hai colpito a tempo, invia il segnale all'interfaccia UI!
        if (attaccoATempo && ComboMeterUI.Instance != null && RhythmManager.Instance.musicType == MusicType.DEFAULT)
        {
            player.AddMusicPoints(10f);
            ComboMeterUI.Instance.AggiungiCombo(10f); // Riempe la barra di 10 punti
        }
    }

    private void PlayHitSound(Vector3 position)
    {
        if (hitSoundEvent.IsNull) return;
    
        FMOD.Studio.EventInstance hitInstance = FMODUnity.RuntimeManager.CreateInstance(hitSoundEvent);
        hitInstance.setParameterByName(timingParameter, timingIndex);
    
        hitInstance.set3DAttributes(FMODUnity.RuntimeUtils.To3DAttributes(position));
        hitInstance.start();
        hitInstance.release();
    }

    public void SetHitboxDamage(float newDamage, float multiplier)
    {
        damage = newDamage;
        if (RhythmManager.Instance == null || myRenderer == null) return;

        timingIndex = 0;
        attaccoATempo = false; // Di base, l'attacco non è a tempo

        // Controlla se il colpo era a tempo
        if (Mathf.Approximately(multiplier, RhythmManager.Instance.goodMultiplier))
        {
            timingIndex = 1;
        }
        else if (Mathf.Approximately(multiplier, RhythmManager.Instance.perfectMultiplier))
        {
            timingIndex = 2;
            attaccoATempo = true;
        }

        // Cambia il colore della hitbox
        if (timingIndex >= 0 && timingIndex < hitboxColors.Length)
        {
            myRenderer.material.color = hitboxColors[timingIndex];
        }
    }

    private void KnockbackEnemy(EnemyBase enemy, float force)
    {
        Vector3 direction = (enemy.transform.position - transform.position);
        direction.y = 0f;
        direction.Normalize();

        UnityEngine.AI.NavMeshAgent agent = enemy.GetComponent<UnityEngine.AI.NavMeshAgent>();
        if (agent != null && agent.isActiveAndEnabled)
        {
            agent.Move(direction * force * Time.deltaTime);
        }
        else
        {
            enemy.transform.position += direction * force * Time.deltaTime;
        }
    }
}