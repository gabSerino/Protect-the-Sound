using UnityEngine;

public class BeatPulse : MonoBehaviour
{
    public float pulseScale = 1.3f;
    public float pulseSpeed = 8f;

    private Vector3 originalScale;
    private float targetScale = 1f;

    private int lastBeat = -1;

    void Start()
    {
        originalScale = transform.localScale;
    }

    void Update()
    {
        if (IsNewBeat())
        {
            TriggerPulse();
        }

        // animazione fluida
        transform.localScale = Vector3.Lerp(
            transform.localScale,
            originalScale * targetScale,
            Time.deltaTime * pulseSpeed
        );

        // ritorno alla scala normale
        targetScale = Mathf.Lerp(targetScale, 1f, Time.deltaTime * pulseSpeed);
    }

    void TriggerPulse()
    {
        targetScale = pulseScale;
    }

    bool IsNewBeat()
    {
        int currentBeat = RhythmManager.Instance.GetCurrentBeat();

        if (currentBeat != lastBeat)
        {
            lastBeat = currentBeat;
            return true;
        }

        return false;
    }
}