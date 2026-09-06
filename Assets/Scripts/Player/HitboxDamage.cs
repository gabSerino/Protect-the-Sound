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

    [Header("VFX")]
[   SerializeField] private HitStarsVFX starsVFX;

    private List<EnemyBase> hitEnemies = new List<EnemyBase>();
    private Color[] hitboxColors;
    [SerializeField] GameObject normalHitSprite, marijHitSprite, cocaHitSprite, lsdHitSprite, mdmaHitSprite;
    private Renderer myRenderer;
    private float damage = 0f;
    private bool attaccoATempo = false; // Memoria per capire se hai colpito a tempo
    private int timingIndex = 0;        // 0 = Normal, 1 = Good, 2 = Perfect

    private void OnEnable() => hitEnemies.Clear();

    public void ResetHitEnemies() => hitEnemies.Clear();

    void Awake()
    {
        player = GetComponentInParent<Player>();
        myRenderer = GetComponent<Renderer>();
        hitboxColors = new Color[3];
        hitboxColors[0] = Color.red;    // Normal
        hitboxColors[1] = Color.yellow; // Good
        hitboxColors[2] = Color.green;  // Perfect

        DisableHitSprite();
    }

    private void OnTriggerEnter(Collider other)
    {
        EnemyBase enemy = other.GetComponentInParent<EnemyBase>();
        if (enemy == null) return;
        if (hitEnemies.Contains(enemy)) return;

        enemy.TakeDamage(damage);
        KnockbackEnemy(enemy, knockbackForce);
        hitEnemies.Add(enemy);

        Vector3 hitPoint = other.ClosestPoint(transform.position);
        PlayHitSound(hitPoint);
        starsVFX?.PlayHitEffect(timingIndex, hitPoint);   // <-- nuova riga

        if (attaccoATempo && ComboMeterUI.Instance != null && RhythmManager.Instance.musicType == MusicType.DEFAULT)
        {
            player.AddMusicPoints(5f);
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

    public void EnableHitSprite()
    {
        DisableHitSprite();
        switch(player.consumedDrug)
        {
            case DrugType.MARIJUANA:
                PlayHitAnimation(marijHitSprite, "atk_marij");
                break;
            case DrugType.COCAINE:
                PlayHitAnimation(cocaHitSprite, "atk_coca");
                break;
            case DrugType.LSD:
                PlayHitAnimation(lsdHitSprite, "atk_lsd");
                break;
            case DrugType.MDMA:
                PlayHitAnimation(mdmaHitSprite, "atk_mdma");
                break;
            case DrugType.NONE:
                PlayHitAnimation(normalHitSprite, "atk_normal");
                break;
            default:
                break;
        }
    }

    public void DisableHitSprite()
    {
        StopAnimation(marijHitSprite);
        StopAnimation(cocaHitSprite);
        StopAnimation(lsdHitSprite);
        StopAnimation(mdmaHitSprite);
        StopAnimation(normalHitSprite);
    }

    private void StopAnimation(GameObject spriteObject)
    {
        Animator animator = spriteObject.GetComponent<Animator>();
        animator.enabled = false;
        spriteObject.GetComponent<SpriteRenderer>().enabled = false;
        animator.enabled = true;
    }

    private void PlayHitAnimation(GameObject spriteObject, string stateName)
    {
        Animator animator = spriteObject.GetComponent<Animator>();
        int stateHash = Animator.StringToHash(stateName);

        if (!animator.HasState(0, stateHash)) return;

        animator.Play(stateHash, 0, 0f);
        animator.Update(0f);
        spriteObject.GetComponent<SpriteRenderer>().enabled = true;
    }

    public void EnableHitboxRenderer()
    {
        gameObject.GetComponent<MeshRenderer>().enabled = true;
    }

    public void DisableHitboxRenderer()
    {
        gameObject.GetComponent<MeshRenderer>().enabled = false;
    }
}