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
    public float startOffset = 0f; // compensazione latenza output (in secondi, es. 0.05)

    [Header("Window Settings")]
    public float perfectInputWindow = 0.1f;
    public float goodInputWindow = 0.2f;
    public float normalMultiplier = 0.75f;
    public float goodMultiplier = 1f;
    public float perfectMultiplier = 1.5f;

    private float beatInterval;
    private bool musicStarted = false;

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
    }

    // 🔥 tempo attuale della "musica" preso DIRETTAMENTE da FMOD, non da Time.time
    public float GetSongTime()
    {
        // getTimelinePosition ritorna i millisecondi reali di playback dell'evento
        musicInstance.getTimelinePosition(out int posMs);

        // Se la posizione è ancora 0 e non abbiamo confermato l'avvio, l'evento
        // potrebbe non essere ancora effettivamente partito (latenza di start).
        PLAYBACK_STATE state;
        musicInstance.getPlaybackState(out state);
        if (state != PLAYBACK_STATE.PLAYING && !musicStarted)
        {
            return 0f;
        }
        musicStarted = true;

        return (posMs / 1000f) - startOffset;
    }

    public int GetCurrentBeat()
    {
        return Mathf.FloorToInt(GetSongTime() / beatInterval);
    }

    public float GetBeatTime(int beatIndex)
    {
        return beatIndex * beatInterval;
    }

    public float GetBeatError()
    {
        float songTime = GetSongTime();
        float nearestBeat = Mathf.Round(songTime / beatInterval) * beatInterval;
        return Mathf.Abs(songTime - nearestBeat);
    }

    public bool IsOnBeat(float perfectWindow, float goodWindow, out float multiplier)
    {
        float error = GetBeatError();
        float scale = beatInterval / (60f / 120f); // scala finestra in base al bpm reale

        if (error <= perfectWindow * scale)
        {
            multiplier = perfectMultiplier;
            return true;
        }
        else if (error <= goodWindow * scale)
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