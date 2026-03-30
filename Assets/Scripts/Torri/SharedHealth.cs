using UnityEngine;
using UnityEngine.UI;

public class SharedHealth : MonoBehaviour
{
    public float maxPoints = 100f;
    private float currentPoints;

    [Header("UI")]
    public Slider healthSlider;

    [Header("Settings Shaking")]
    public float shakeThreshold = 20f; // Soglia sotto la quale trema
    public float shakeIntensity = 5f;  // Quanto forte deve tremare

    private RectTransform sliderRectTransform;
    private Vector2 originalPosition;

    void Start()
    {
        // Forziamo il minimo a 100 come richiesto
        if (maxPoints < 100f) maxPoints = 100f;

        currentPoints = maxPoints;

        if (healthSlider != null)
        {
            healthSlider.maxValue = maxPoints;
            healthSlider.value = maxPoints;

            // Memorizziamo la posizione originale per il tremolio
            sliderRectTransform = healthSlider.GetComponent<RectTransform>();
            originalPosition = sliderRectTransform.anchoredPosition;
        }
    }

    void Update()
    {
        HandleShake();
    }

    private void HandleShake()
    {
        if (healthSlider == null) return;

        // Trema se la vita è tra 1 e 20. Si ferma a 0.
        if (currentPoints <= shakeThreshold && currentPoints > 0)
        {
            float offsetX = Random.Range(-1f, 1f) * shakeIntensity;
            float offsetY = Random.Range(-1f, 1f) * shakeIntensity;

            sliderRectTransform.anchoredPosition = originalPosition + new Vector2(offsetX, offsetY);
        }
        else
        {
            // Torna alla posizione originale se siamo sopra la soglia o a zero
            if (sliderRectTransform.anchoredPosition != originalPosition)
            {
                sliderRectTransform.anchoredPosition = originalPosition;
            }
        }
    }

    public void TakeDamageGlobal(float amount)
    {
        currentPoints -= amount;
        // Impedisce che la vita scenda sotto lo zero
        currentPoints = Mathf.Max(currentPoints, 0);

        if (healthSlider != null)
        {
            healthSlider.value = currentPoints;
        }

        if (currentPoints <= 0)
        {
            GameOver();
        }
    }

    void GameOver()
    {
        Debug.Log("Tutti i punti sono distrutti!");
        // Logica di fine gioco
    }
}