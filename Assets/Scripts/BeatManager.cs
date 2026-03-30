using UnityEngine;
using UnityEngine.Events;

public class BeatManager : MonoBehaviour
{
    public float _bpm = 120f;
    [SerializeField] private AudioSource _musicSource;
    [SerializeField] private Intervals[] _intervals;
    private void Update()
    {
        foreach (var interval in _intervals)
        {
            float sampledTime = _musicSource.timeSamples /(_musicSource.clip.frequency * interval.GetIntervalLength(_bpm));
            interval.CheckForNewInterval(sampledTime);
        }
    }
}

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

    public void CheckForNewInterval(float inteval)
    {
        if(Mathf.FloorToInt(inteval) != _lastInterval)
        {
            _lastInterval = Mathf.FloorToInt(inteval);
            _onBeat.Invoke();
        }
    }
}
