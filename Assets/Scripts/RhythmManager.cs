using System;
using System.Runtime.InteropServices;
using UnityEngine;
using FMODUnity;
using FMOD.Studio;
using FMOD;

public class RhythmManager : MonoBehaviour
{
    public static RhythmManager Instance;

    // Deve rispecchiare ESATTAMENTE l'ordine dei valori del labeled parameter
    // "Music" definito in FMOD Studio (indice 0 = DEFAULT, 1 = DnB, ecc.)
    [Header("FMOD")]
    public string eventPath = "event:/theme";
    private EventInstance musicInstance;
    private EVENT_CALLBACK beatCallback;
    private GCHandle timelineHandle;

    [Header("Music Style")]
    public MusicType musicType = MusicType.DEFAULT;
    private const string musicParamName = "Music";

    [Header("Rhythm Settings")]
    public float startOffset = 0f; // compensazione latenza output (in secondi, es. 0.05)

    [Header("Window Settings")]
    public float perfectInputWindow = 0.1f;
    public float goodInputWindow = 0.2f;
    public float normalMultiplier = 0.75f;
    public float goodMultiplier = 1f;
    public float perfectMultiplier = 1.5f;

    private bool musicStarted = false;

    // Struct condivisa in modo thread-safe col callback nativo di FMOD.
    // Deve essere una classe (reference type) perché il GCHandle punta
    // alla stessa istanza sia da Unity che dal thread audio.
    private class TimelineInfo
    {
        public int currentBeat = 0;
        public int currentBar = 0;
        public float currentTempo = 120f;
        public int timeSigUpper = 4;
        public int timeSigLower = 4;
        public double lastBeatSongPosition = 0; // in secondi, posizione timeline all'ultimo beat
    }
    private TimelineInfo timelineInfo = new TimelineInfo();

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
        musicInstance = RuntimeManager.CreateInstance(eventPath);

        timelineHandle = GCHandle.Alloc(timelineInfo, GCHandleType.Pinned);
        musicInstance.setUserData(GCHandle.ToIntPtr(timelineHandle));

        beatCallback = new EVENT_CALLBACK(BeatEventCallback);
        musicInstance.setCallback(beatCallback, EVENT_CALLBACK_TYPE.TIMELINE_BEAT | EVENT_CALLBACK_TYPE.TIMELINE_MARKER);

        musicInstance.start();

        // Applica il valore iniziale scelto nell'Inspector
        ApplyMusicStyle();
    }

    private void ApplyMusicStyle()
    {
        // Il cast a float corrisponde all'indice del labeled parameter in FMOD Studio
        FMOD.RESULT res = musicInstance.setParameterByName(musicParamName, (float)musicType);
        if (res != FMOD.RESULT.OK)
        {
            UnityEngine.Debug.LogWarning($"[RhythmManager] Impossibile impostare il parametro '{musicParamName}': {res}");
        }
    }

    /// Cambia lo stile musicale a runtime (es. da altri script o UI)
    public void SetMusicStyle(MusicType newStyle)
    {
        musicType = newStyle;
        ApplyMusicStyle();
    }

    public MusicType GetMusicStyle()
    {
        return musicType;
    }

#if UNITY_EDITOR
    // Permette di testare il cambio parametro direttamente dall'Inspector durante il Play
    void OnValidate()
    {
        if (Application.isPlaying && musicInstance.isValid())
        {
            ApplyMusicStyle();
        }
    }
#endif

    [AOT.MonoPInvokeCallback(typeof(EVENT_CALLBACK))]
    static FMOD.RESULT BeatEventCallback(EVENT_CALLBACK_TYPE type, IntPtr instancePtr, IntPtr parameterPtr)
    {
        EventInstance instance = new EventInstance(instancePtr);

        IntPtr userDataPtr;
        FMOD.RESULT res = instance.getUserData(out userDataPtr);
        if (res != FMOD.RESULT.OK || userDataPtr == IntPtr.Zero)
            return FMOD.RESULT.OK;

        GCHandle handle = GCHandle.FromIntPtr(userDataPtr);
        TimelineInfo info = handle.Target as TimelineInfo;
        if (info == null) return FMOD.RESULT.OK;

        if (type == EVENT_CALLBACK_TYPE.TIMELINE_BEAT)
        {
            var props = (TIMELINE_BEAT_PROPERTIES)Marshal.PtrToStructure(parameterPtr, typeof(TIMELINE_BEAT_PROPERTIES));
            info.currentBeat = props.beat;
            info.currentBar = props.bar;
            info.currentTempo = props.tempo; // 🔥 tempo aggiornato in base al marker attivo
            info.timeSigUpper = props.timesignatureupper;
            info.timeSigLower = props.timesignaturelower;
            info.lastBeatSongPosition = props.position / 1000.0; // ms -> secondi
        }

        return FMOD.RESULT.OK;
    }

    // 🔥 tempo attuale della "musica" preso DIRETTAMENTE da FMOD, non da Time.time
    public float GetSongTime()
    {
        musicInstance.getTimelinePosition(out int posMs);

        PLAYBACK_STATE state;
        musicInstance.getPlaybackState(out state);
        if (state != PLAYBACK_STATE.PLAYING && !musicStarted)
        {
            return 0f;
        }
        musicStarted = true;
        return (posMs / 1000f) - startOffset;
    }

    public float CurrentBpm => timelineInfo.currentTempo;
    public int CurrentBeat => timelineInfo.currentBeat;
    public int CurrentBar => timelineInfo.currentBar;

    private float BeatInterval => 60f / Mathf.Max(timelineInfo.currentTempo, 1f);

    public int GetCurrentBeat()
    {
        return Mathf.FloorToInt(GetSongTime() / BeatInterval);
    }

    public float GetBeatTime(int beatIndex)
    {
        return beatIndex * BeatInterval;
    }

    public float GetBeatError()
    {
        float songTime = GetSongTime();
        float interval = BeatInterval;

        // IMPORTANTE: non si può assumere che la griglia dei beat parta da t=0
        // con l'intervallo corrente, perché se il tempo è cambiato durante il
        // brano quell'assunzione è falsa (i beat precedenti al cambio avevano
        // un intervallo diverso). Si ancora invece il calcolo all'ultimo beat
        // realmente confermato da FMOD (lastBeatSongPosition, aggiornato dal
        // callback TIMELINE_BEAT) e si estrapola solo localmente da lì.
        float anchor = (float)timelineInfo.lastBeatSongPosition;
        float offsetFromAnchor = songTime - anchor;
        float nearestBeat = anchor + Mathf.Round(offsetFromAnchor / interval) * interval;

        return Mathf.Abs(songTime - nearestBeat);
    }

    public bool IsOnBeat(float perfectWindow, float goodWindow, out float multiplier)
    {
        float error = GetBeatError();

        if (error <= perfectWindow*120f/timelineInfo.currentTempo)
        {
            multiplier = perfectMultiplier;
            return true;
        }
        else if (error <= goodWindow*120f/timelineInfo.currentTempo)
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
        musicInstance.setCallback(null, EVENT_CALLBACK_TYPE.ALL);
        musicInstance.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
        musicInstance.release();

        if (timelineHandle.IsAllocated)
            timelineHandle.Free();
    }
}