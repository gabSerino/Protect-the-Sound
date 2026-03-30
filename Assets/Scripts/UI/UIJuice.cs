using UnityEngine;
using System.Collections;

public class UIJuice : MonoBehaviour
{
    private Vector3 originalPosition;
    private Coroutine shakeCoroutine;

    [Header("Impostazioni Tremolio")]
    public float duration = 0.2f;  // Quanto dura il tremolio
    public float magnitude = 5.0f; // Quanto è forte la vibrazione

    void Start()
    {
        // Salviamo la posizione iniziale dello slider per tornarci dopo il tremolio
        originalPosition = transform.localPosition;
    }

    public void Shake()
    {
        // Se c'è già un tremolio in corso, lo fermiamo per farne partire uno nuovo
        if (shakeCoroutine != null) StopCoroutine(shakeCoroutine);
        shakeCoroutine = StartCoroutine(DoShake());
    }

    IEnumerator DoShake()
    {
        float elapsed = 0.0f;

        while (elapsed < duration)
        {
            // Genera uno spostamento casuale
            float x = Random.Range(-1f, 1f) * magnitude;
            float y = Random.Range(-1f, 1f) * magnitude;

            transform.localPosition = new Vector3(originalPosition.x + x, originalPosition.y + y, originalPosition.z);

            elapsed += Time.deltaTime;
            yield return null; // Aspetta il prossimo frame
        }

        // Torna alla posizione originale alla fine
        transform.localPosition = originalPosition;
    }
}