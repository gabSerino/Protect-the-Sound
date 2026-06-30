using UnityEngine;

public class CanvasPulse : MonoBehaviour
{
    [Header("Target")]
    public RectTransform[] targetRects;

    [Header("Pulse Settings")]
    public float pulseScale = 1.15f;   // quanto si ingrandisce al beat
    public float pulseSpeed = 8f;      // velocità di rientro alla scala normale

    private Vector3[] baseScales;
    private int lastBeat = -1;

    void Awake()
    {
        baseScales = new Vector3[targetRects.Length];
        for (int i = 0; i < targetRects.Length; i++)
        {
            if (targetRects[i] != null)
                baseScales[i] = targetRects[i].localScale;
        }
    }

    void Update()
    {
        bool newBeat = IsNewBeat();

        for (int i = 0; i < targetRects.Length; i++)
        {
            if (targetRects[i] == null) continue;

            if (newBeat)
            {
                targetRects[i].localScale = baseScales[i] * pulseScale;
            }

            // ogni frame torniamo morbidamente verso la scala base
            targetRects[i].localScale = Vector3.Lerp(
                targetRects[i].localScale,
                baseScales[i],
                Time.deltaTime * pulseSpeed
            );
        }
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