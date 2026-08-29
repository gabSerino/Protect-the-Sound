using UnityEngine;
using UnityEngine.UI;

public class SharedHealth : MonoBehaviour
{
    public float maxPoints = 100f;
    private float currentPoints;

    [Header("UI Barra della Vita")]
    [Tooltip("Trascina qui il GameObject 'Fill' (quello con Image Type: Filled)")]
    public Image barraVitaFill;

    [Tooltip("Trascina qui il GameObject padre della barra per lo shake e la visibilità")]
    public RectTransform healthBarContainer;

    [Header("Settings Shaking")]
    public float shakeThreshold = 20f;
    public float shakeIntensity = 5f;

    [Header("Gestore Game Over")]
    public GameOverManager gameOverManager;

    [Header("Rivelazione Ritardata")]
    [Tooltip("Lascia vuoto se questa cassa è visibile e attaccabile fin dall'inizio")]
    public SharedHealth[] casseDaDistruggerePrimaDiApparire;
    [Tooltip("Collider da abilitare alla rivelazione, così i nemici non rilevano questa cassa prima del tempo")]
    public Collider[] colliderDaAbilitare;

    [Header("Gestione Sprite Danno")]
    public Image iconaCassa;
    public Sprite spriteCassaSpaccata;
    private bool spriteCambiato = false;

    private Vector2 originalPosition;
    private bool isDestroyed = false;
    private bool isRivelata = true;

    public bool IsDestroyed => isDestroyed;

    private static int activeCasseCount = 0;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void ResetStaticState()
    {
        activeCasseCount = 0;
    }

    void Awake()
    {
        // Inizializza i punti vita in Awake così sono pronti prima di qualsiasi raggio/collisione
        if (maxPoints < 100f) maxPoints = 100f;
        currentPoints = maxPoints;
    }

    void Start()
    {
        activeCasseCount++;

        if (healthBarContainer != null)
        {
            originalPosition = healthBarContainer.anchoredPosition;
        }

        if (casseDaDistruggerePrimaDiApparire != null && casseDaDistruggerePrimaDiApparire.Length > 0)
        {
            isRivelata = false;
            SetElementiVisibili(false);
        }
        else
        {
            AggiornaGraficaUI();
        }
    }

    void Update()
    {
        if (!isRivelata) CheckRivelazione();
        HandleShake();
    }

    private void CheckRivelazione()
    {
        foreach (SharedHealth cassa in casseDaDistruggerePrimaDiApparire)
        {
            if (cassa != null && !cassa.IsDestroyed) return;
        }

        isRivelata = true;
        SetElementiVisibili(true);
    }

    private void SetElementiVisibili(bool visibile)
    {
        if (healthBarContainer != null)
        {
            healthBarContainer.gameObject.SetActive(visibile);
        }

        if (colliderDaAbilitare != null)
        {
            foreach (Collider col in colliderDaAbilitare)
            {
                if (col != null) col.enabled = visibile;
            }
        }

        // Quando l'oggetto diventa visibile, forza il ripristino della barra piena
        if (visibile)
        {
            AggiornaGraficaUI();
        }
    }

    private void AggiornaGraficaUI()
    {
        if (barraVitaFill != null && maxPoints > 0)
        {
            barraVitaFill.fillAmount = currentPoints / maxPoints;
        }
    }

    private void HandleShake()
    {
        if (healthBarContainer == null || isDestroyed || !isRivelata) return;

        if (currentPoints <= shakeThreshold && currentPoints > 0)
        {
            float offsetX = Random.Range(-1f, 1f) * shakeIntensity;
            float offsetY = Random.Range(-1f, 1f) * shakeIntensity;
            healthBarContainer.anchoredPosition = originalPosition + new Vector2(offsetX, offsetY);
        }
        else
        {
            if (healthBarContainer.anchoredPosition != originalPosition)
            {
                healthBarContainer.anchoredPosition = originalPosition;
            }
        }
    }

    public void TakeDamage(float amount)
    {
        if (isDestroyed || !isRivelata) return;

        currentPoints -= amount;
        currentPoints = Mathf.Max(currentPoints, 0);

        AggiornaGraficaUI();

        if (!spriteCambiato && currentPoints <= (maxPoints / 2f))
        {
            if (iconaCassa != null && spriteCassaSpaccata != null)
            {
                iconaCassa.sprite = spriteCassaSpaccata;
                spriteCambiato = true;
            }
        }

        if (currentPoints <= 0)
        {
            isDestroyed = true;
            activeCasseCount--;

            SetElementiVisibili(false);

            if (activeCasseCount <= 0 && gameOverManager != null)
            {
                gameOverManager.AttivaGameOver();
            }

            gameObject.SetActive(false);
        }
    }

    void OnDestroy()
    {
        if (!isDestroyed)
        {
            isDestroyed = true;
            activeCasseCount--;
        }
    }
}