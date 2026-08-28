using UnityEngine;
using UnityEngine.UI;

public class SharedHealth : MonoBehaviour
{
    public float maxPoints = 100f;
    private float currentPoints;

    [Header("UI")]
    public Slider healthSlider;

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
    public Image iconaCassa; // Trascina qui l'oggetto "Image" che fa da icona
    public Sprite spriteCassaSpaccata; // Trascina qui la grafica della cassa rotta
    private bool spriteCambiato = false;

    private RectTransform sliderRectTransform;
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

    void Start()
    {
        if (maxPoints < 100f) maxPoints = 100f;
        currentPoints = maxPoints;

        activeCasseCount++;

        if (healthSlider != null)
        {
            healthSlider.maxValue = maxPoints;
            healthSlider.value = maxPoints;
            sliderRectTransform = healthSlider.GetComponent<RectTransform>();
            originalPosition = sliderRectTransform.anchoredPosition;
        }

        if (casseDaDistruggerePrimaDiApparire != null && casseDaDistruggerePrimaDiApparire.Length > 0)
        {
            isRivelata = false;
            SetElementiVisibili(false);
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
        if (healthSlider != null)
        {
            healthSlider.gameObject.SetActive(visibile);
        }

        if (colliderDaAbilitare != null)
        {
            foreach (Collider col in colliderDaAbilitare)
            {
                if (col != null) col.enabled = visibile;
            }
        }
    }

    private void HandleShake()
    {
        if (healthSlider == null || isDestroyed || !isRivelata) return;

        if (currentPoints <= shakeThreshold && currentPoints > 0)
        {
            float offsetX = Random.Range(-1f, 1f) * shakeIntensity;
            float offsetY = Random.Range(-1f, 1f) * shakeIntensity;
            sliderRectTransform.anchoredPosition = originalPosition + new Vector2(offsetX, offsetY);
        }
        else
        {
            if (sliderRectTransform.anchoredPosition != originalPosition)
            {
                sliderRectTransform.anchoredPosition = originalPosition;
            }
        }
    }

    public void TakeDamage(float amount)
    {
        if (isDestroyed || !isRivelata) return;

        currentPoints -= amount;
        currentPoints = Mathf.Max(currentPoints, 0);

        if (healthSlider != null)
        {
            healthSlider.value = currentPoints;
        }

        // --- CONTROLLO CAMBIO SPRITE ---
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