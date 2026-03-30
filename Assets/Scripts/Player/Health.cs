using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class Health : MonoBehaviour
{
    [Header("Settings")]
    public float maxPoints = 100f;

    [Header("Invulnerability settings")]
    public float invulnerabilityDuration = 2f;
    public float flickerInterval = 0.1f;

    [Header("UI")]
    public Slider healthSlider;
    public UIJuice uiJuice;

    public float CurrentPoints { get; private set; }

    private CharacterController _controller;
    // MODIFICA 1: Ora è un array [], una lista di Renderer
    private Renderer[] _playerRenderers;
    private bool _isInvulnerable = false;

    void Awake()
    {
        _controller = GetComponent<CharacterController>();

        // MODIFICA 2: Usiamo la versione plurale "GetComponentsInChildren"
        // Questo comando troverà i Renderer di 'Face', 'Player Capsule', e qualsiasi altro figlio.
        _playerRenderers = GetComponentsInChildren<Renderer>();

        // Controllo di sicurezza se l'array è vuoto
        if (_playerRenderers == null || _playerRenderers.Length == 0)
        {
            Debug.LogError("Attenzione: Nessun Renderer trovato sul Player o nei suoi figli! Il lampeggio non funzionerà.");
        }
    }

    void Start()
    {
        ResetHealth();
    }

    public void TakeDamage(float amount)
    {
        if (_isInvulnerable) return;

        CurrentPoints = Mathf.Clamp(CurrentPoints - amount, 0, maxPoints);
        UpdateUI();

        if (uiJuice != null) uiJuice.Shake();

        if (CurrentPoints <= 0)
        {
            Respawn();
        }
    }

    private void Respawn()
    {
        ResetHealth();

        if (_controller != null) _controller.enabled = false;
        transform.position = Vector3.zero;
        transform.rotation = Quaternion.identity;
        if (_controller != null) _controller.enabled = true;

        // --- NUOVO: Avvia l'invulnerabilità multi-renderer ---
        // Verifichiamo che l'array non sia vuoto
        if (_playerRenderers != null && _playerRenderers.Length > 0)
        {
            StartCoroutine(MultiRendererInvulnerabilityRoutine());
        }
    }

    // --- MODIFICA 3: La Coroutina aggiornata per gestire più Renderer ---
    private IEnumerator MultiRendererInvulnerabilityRoutine()
    {
        _isInvulnerable = true;

        float timer = 0;
        bool currentlyVisible = true; // Stato di base

        while (timer < invulnerabilityDuration)
        {
            // Invertiamo lo stato
            currentlyVisible = !currentlyVisible;

            // --- CICLO FOR: Applichiamo il nuovo stato a TUTTI i Renderer ---
            // Invece di modificare un solo renderer, usiamo un ciclo for
            // per spegnere o accendere ogni Renderer trovato nell'array.
            foreach (Renderer r in _playerRenderers)
            {
                // Un controllo di sicurezza extra, nel caso un figlio venisse distrutto a runtime
                if (r != null)
                {
                    r.enabled = currentlyVisible;
                }
            }

            yield return new WaitForSeconds(flickerInterval);
            timer += flickerInterval;
        }

        // --- FINE CICLO: Ripristino finale per TUTTI ---
        // Quando il tempo scade, usiamo un altro ciclo foreach per riaccendere tutto.
        foreach (Renderer r in _playerRenderers)
        {
            if (r != null)
            {
                r.enabled = true;
            }
        }

        _isInvulnerable = false;
    }

    private void ResetHealth()
    {
        CurrentPoints = maxPoints;
        UpdateUI();
    }

    private void UpdateUI()
    {
        if (healthSlider != null)
        {
            healthSlider.maxValue = maxPoints;
            healthSlider.value = CurrentPoints;
        }
    }
}