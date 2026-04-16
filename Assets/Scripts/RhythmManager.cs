using UnityEngine;
using FMODUnity;
using FMOD.Studio;

public class RhythmManager : MonoBehaviour
{
    public static RhythmManager Instance;

    [Header("FMOD")]
    public string eventPath = "event:/theme";
    private EventInstance musicInstance;

    [Header("Rhythm Settings")]
    public float bpm = 120f;
    public float startOffset = 0f; // compensazione latenza (tipo 0.05)

    [Header("Window Settings")]
    public float perfectInputWindow = 0.1f;
    public float goodInputWindow = 0.2f;
    public float normalMultiplier = 0.5f;
    public float goodMultiplier = 0.75f;
    public float perfectMultiplier = 1.0f;

    private float beatInterval;
    private float songStartTime;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    void Start()
    {
        beatInterval = 60f / bpm;

        musicInstance = RuntimeManager.CreateInstance(eventPath);
        musicInstance.start();

        // segniamo quando parte la musica
        songStartTime = Time.time + startOffset;
    }

    // 🔥 tempo attuale della "musica"
    public float GetSongTime()
    {
        return Time.time - songStartTime;
    }

    // 🔥 beat corrente (numero intero)
    public int GetCurrentBeat()
    {
        return Mathf.FloorToInt(GetSongTime() / beatInterval);
    }

    // 🔥 tempo del beat corrente
    public float GetBeatTime(int beatIndex)
    {
        return beatIndex * beatInterval;
    }

    // 🔥 quanto sei fuori tempo dal beat più vicino
    public float GetBeatError()
    {
        float songTime = GetSongTime();

        float nearestBeat = Mathf.Round(songTime / beatInterval) * beatInterval;

        return Mathf.Abs(songTime - nearestBeat);
    }

    // 🔥 valutazione timing (per gameplay)
    public bool IsOnBeat(float perfectWindow, float goodWindow, out float multiplier)
    {
        float error = GetBeatError();

        if (error <= perfectWindow)
        {
            multiplier = perfectMultiplier;
            return true;
        }
        else if (error <= goodWindow)
        {
            multiplier = goodMultiplier;
            return true;
        }
        else
        {
            multiplier = normalMultiplier;
            return false;
        }
    }

    void OnDestroy()
    {
        musicInstance.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
        musicInstance.release();
    }
}