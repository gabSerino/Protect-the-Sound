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
    public GameOverManager gameOverManager; // <--- Riferimento al nuovo script

    private RectTransform sliderRectTransform;
    private Vector2 originalPosition;

    void Start()
    {
        if (maxPoints < 100f) maxPoints = 100f;
        currentPoints = maxPoints;

        if (healthSlider != null)
        {
            healthSlider.maxValue = maxPoints;
            healthSlider.value = maxPoints;

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

    public void TakeDamageGlobal(float amount)
    {
        currentPoints -= amount;
        currentPoints = Mathf.Max(currentPoints, 0);

        if (healthSlider != null)
        {
            healthSlider.value = currentPoints;
        }

        if (currentPoints <= 0)
        {
            // La vita è a 0! Diciamo al manager di attivare la schermata
            if (gameOverManager != null)
            {
                gameOverManager.AttivaGameOver();
            }
        }
    }
}