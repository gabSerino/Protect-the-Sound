using UnityEngine;

/// <summary>
/// Gestisce l'effetto "stelline" che compare quando si colpisce un nemico a ritmo.
/// Va aggiunto sullo stesso GameObject di un componente ParticleSystem (o come
/// riferimento a uno figlio) configurato per NON emettere automaticamente
/// (modulo Emission -> Rate over Time = 0): è questo script a far partire
/// i burst via codice, chiamando PlayHitEffect() da HitboxDamage.
/// </summary>
[RequireComponent(typeof(ParticleSystem))]
public class HitStarsVFX : MonoBehaviour
{
    [Header("Riferimenti")]
    [SerializeField] private ParticleSystem starsParticles;

    [Header("Colori delle stelline (coerenti con HitboxDamage.hitboxColors)")]
    [SerializeField] private Color goodColor = Color.yellow;
    [SerializeField] private Color perfectColor = Color.green;

    [Header("Quantità di stelline per livello di precisione")]
    [Tooltip("Fuori tempo: nessuna stellina")]
    [SerializeField] private int normalStarCount = 0;
    [Tooltip("Quasi a tempo (Good): poche stelline gialle")]
    [SerializeField] private int goodStarCount = 6;
    [Tooltip("A tempo perfetto (Perfect): tante stelline verdi")]
    [SerializeField] private int perfectStarCount = 20;

    private void Awake()
    {
        if (starsParticles == null)
            starsParticles = GetComponent<ParticleSystem>();

        // Disattiva l'emissione automatica: le stelline devono uscire
        // SOLO quando chiamiamo Emit() da PlayHitEffect().
        var emission = starsParticles.emission;
        emission.rateOverTime = 0;
        starsParticles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
    }

    /// <summary>
    /// Fa partire l'effetto stelline in base a quanto è stato preciso il colpo.
    /// </summary>
    /// <param name="timingIndex">0 = Normal (fuori tempo), 1 = Good, 2 = Perfect</param>
    /// <param name="worldPosition">Punto in world space da cui far uscire le stelline
    /// (es. lo stesso hitPoint che usi già per il suono d'impatto)</param>
    public void PlayHitEffect(int timingIndex, Vector3 worldPosition)
    {
        int count;
        Color color;

        switch (timingIndex)
        {
            case 2: // Perfect -> a tempo
                count = perfectStarCount;
                color = perfectColor;
                break;
            case 1: // Good -> quasi a tempo
                count = goodStarCount;
                color = goodColor;
                break;
            default: // Normal -> fuori tempo, niente stelline
                count = normalStarCount;
                color = goodColor; // ininfluente: count sarà 0
                break;
        }

        if (count <= 0 || starsParticles == null) return;

        // Sposta il GameObject dell'effetto sul punto dell'impatto
        transform.position = worldPosition;

        // Applica solo il colore: la direzione di uscita viene calcolata
        // autonomamente dal modulo Shape del Particle System
        var emitParams = new ParticleSystem.EmitParams
        {
            startColor = color
        };

        starsParticles.Emit(emitParams, count);
    }
}