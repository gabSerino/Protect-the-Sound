using UnityEngine;
using UnityEngine.Events;

public class BeatManager : MonoBehaviour
{
    public float _bpm = 120f;
    [SerializeField] private AudioSource _musicSource;
    [SerializeField] private Intervals[] _intervals;

    [Range(0f, 0.2f)] 
    [SerializeField] private float _marginPercentage = 0.05f;

    public bool IsOnBeat()
    {
        if (_intervals == null || _intervals.Length == 0 || _musicSource.clip == null) return false;

        // Calcoliamo la durata di un beat in campioni audio (samples)
        float samplesPerBeat = (_musicSource.clip.frequency * 60f) / _bpm;
        int currentSample = _musicSource.timeSamples;
        float marginInSamples = samplesPerBeat * _marginPercentage;
        float samplePosInBeat = currentSample % samplesPerBeat;

        bool closeToStart = samplePosInBeat < marginInSamples;
        bool closeToEnd = samplePosInBeat > (samplesPerBeat - marginInSamples);

        return closeToStart || closeToEnd;
    }

    private void Update()
    {
        foreach (var interval in _intervals)
        {
            float sampledTime = _musicSource.timeSamples / (_musicSource.clip.frequency * interval.GetIntervalLength(_bpm));
            interval.CheckForNewInterval(sampledTime);
        }
    }
}

// --- QUESTA È LA PARTE CHE MANCAVA ---
[System.Serializable]
public class Intervals
{
    [SerializeField] private float _stepsPerBeat;
    [SerializeField] private UnityEvent _onBeat;
    private int _lastInterval;

    public float GetIntervalLength(float bpm)
    {
        return 60f / (bpm * _stepsPerBeat);
    }

    public void CheckForNewInterval(float interval)
    {
        if (Mathf.FloorToInt(interval) != _lastInterval)
        {
            _lastInterval = Mathf.FloorToInt(interval);
            _onBeat.Invoke();
        }
    }
}