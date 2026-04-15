using UnityEngine;

public class BeatTracker : MonoBehaviour
{
    [SerializeField] private float bpm = 120f; // Impostalo a 120 come il manager
    private float lastBeatTime;
    private float nextBeatTime;
    private float intervalLength;

    void Awake()
    {
        intervalLength = 60f / bpm;
    }

    public void OnBeatDetected()
    {
        lastBeatTime = Time.time;
        nextBeatTime = Time.time + intervalLength;
        // Debug.Log("Beat ricevuto dal Tracker!"); // Decommenta per testare
    }

    public bool IsInWindow(float margin)
    {
        if (lastBeatTime == 0) return false;

        float timeSinceLast = Time.time - lastBeatTime;
        float timeToNext = nextBeatTime - Time.time;

        // Ritorna true se siamo vicini al battito passato o a quello imminente
        return (timeSinceLast <= margin || timeToNext <= margin);
    }
}