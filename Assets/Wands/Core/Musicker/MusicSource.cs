using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Musicker {

/// a thing that plays music
public sealed class MusicSource: MonoBehaviour {
    // -- tuning --
    [Header("tuning")]
    [Tooltip("the max volume")]
    [SerializeField] float m_MaxVolume = 1.0f;

    [Tooltip("the template audio source")]
    [SerializeField] AudioSource m_Template;

    // -- config --
    [Header("config")]
    [Tooltip("the number of audio sources to create or keep")]
    [SerializeField] int m_NumSources = 4;

    [Tooltip("the audio source to realize sound")]
    [SerializeField] List<AudioSource> m_Sources;

    [Tooltip("the current instrument")]
    [SerializeField] Instrument m_Instrument;

    // -- props --
    /// the index of the next available audio source
    int m_NextSource = 0;

    // -- lifecycle --
    void Awake() {
        // make sure the template is one of the sources
        if (m_Template != null && !m_Sources.Contains(m_Template)) {
            m_Sources.Add(m_Template);
        }

        // create audio sources
        for (var i = m_Sources.Count; i < m_NumSources; i++) {
            m_Sources.Add(InitAudioSource());
        }

        // init loop module
        PlayLoop_Init();
    }

    void Update() {
        // update loop module
        PlayLoop_Update();
    }

    // -- commands --
    /// play the current tone in the line and advance it
    public void PlayLine(Line line, Key? key = null) {
        PlayTone(line.Curr(), key);
        line.Advance();
    }

    /// play the current chord in a progression and advance it
    public void PlayProgression(Progression prog, float interval = 0.0f, Key? key = null) {
        PlayChord(prog.Curr(), interval, key);
        prog.Advance();
    }

    /// play the clips in the chord
    public void PlayChord(Chord chord, Key? key = null) {
        PlayChord(chord, 0.0f, key);
    }

    /// play the clips in the chord, pass an interval to arpeggiate
    public void PlayChord(Chord chord, float interval, Key? key = null) {
        StartCoroutine(PlayChordAsync(chord, interval, key));
    }

    /// play the clips in the chord. pass an interval to arpeggiate.
    IEnumerator PlayChordAsync(Chord chord, float interval = 0.0f, Key? key = null) {
        for (var i = 0; i < chord.Length; i++) {
            PlayTone(chord[i], key);

            if (interval != 0.0) {
                yield return new WaitForSeconds(interval);
            }
        }
    }

    /// play the clip for a tone
    public void PlayTone(Tone tone, Key? key = null) {
        // transpose if necessary
        if (key != null) {
            tone = key.Value.Transpose(tone);
        }

        // play the clip
        PlayClip(m_Instrument.FindClip(tone));
    }

    /// play a random audio clip
    public void PlayRand() {
        PlayClip(m_Instrument.RandClip());
    }

    // -- c/config
    /// set the pitch
    public void SetPitch(float pitch) {
        foreach (var source in m_Sources) {
            source.pitch = pitch;
        }
    }

    /// set the maximum distance
    public void SetMaxDistance(float distance) {
        foreach (var source in m_Sources) {
            source.maxDistance = distance;
        }
    }

    /// the the max volume of the loop
    public void SetMaxVolume(float max) {
        m_MaxVolume = max;

        foreach (var source in m_Sources) {
            source.volume = Mathf.Min(source.volume, max);
        }
    }

    // -- c/helpers
    /// play a clip on the next source
    void PlayClip(AudioClip clip) {
        // play the clip
        var source = m_Sources[m_NextSource];
        source.clip = clip;
        source.Play();

        // advance the source
        m_NextSource = (m_NextSource + 1) % m_NumSources;
    }

    // -- props/hot
    /// the current instrument
    public Instrument Instrument {
        get => m_Instrument;
        set => m_Instrument = value;
    }

    // -- queries --
    /// if the musicker has any sources available
    public bool IsAvailable() {
        foreach (var source in m_Sources) {
            if (!source.isPlaying) {
                return true;
            }
        }

        return false;
    }

    // -- factories --
    /// create a new audio source
    AudioSource InitAudioSource() {
        // add the audio source
        var src = gameObject.AddComponent<AudioSource>();

        // copy templated props
        var tmp = m_Template;
        if (tmp != null) {
            src.minDistance = tmp.minDistance;
            src.maxDistance = tmp.maxDistance;

            var t0 = AudioSourceCurveType.CustomRolloff;
            var t1 = AudioSourceCurveType.Spread;
            for (var t = t0; t <= t1; t++) {
                src.SetCustomCurve(t, tmp.GetCustomCurve(t));
            }
        }

        return src;
    }

    //
    // -- PlayLoop --
    //

    // -- props --
    /// the current loop, if any
    Loop m_Loop;

    /// the current coroutine, if any
    Coroutine m_Routine;

    /// the master volume during a loop
    float m_Volume = 1.0f;

    /// the per-source volumes during a looop
    float[] m_VolumeBySource;

    // -- lifetime --
    /// init loop support
    void PlayLoop_Init() {
        // set initial volumes
        m_Volume = m_MaxVolume;
        m_VolumeBySource = new float[m_NumSources].Fill(m_MaxVolume);
    }

    void PlayLoop_Update() {
        // if there is a loop
        if (m_Loop == null) {
            return;
        }

        // update the source volumes
        var v0 = m_Volume;
        for (var i = 0; i < m_NumSources; i++) {
            var v1 = m_VolumeBySource[i];
            m_Sources[i].volume = v0 * v1;
        }
    }

    // -- commands --
    /// toggle the loop
    public void ToggleLoop(Loop loop, bool isPlaying, Key? key = null) {
        if (m_Loop != loop) {
            StopLoop();
        }

        if (isPlaying) {
            PlayLoop(loop, key);
        } else {
            StopLoop();
        }
    }

    /// play the loop
    public void PlayLoop(Loop loop, Key? key = null) {
        // if its a different loop
        if (m_Loop == loop) {
            return;
        }

        // play the loop
        m_Loop = loop;
        m_Routine = StartCoroutine(PlayLoopAsync(loop, key));
    }

    /// play the loop
    IEnumerator PlayLoopAsync(Loop loop, Key? key = null) {
        // fade in
        var master = VolumeLens();
        master.Tween(0.0f, m_MaxVolume, dur: loop.Fade);

        // the time between loop plays
        var blend = loop.Blend;
        var interval = m_Instrument.Duration - blend;

        // repeat tones until stopped
        while (true) {
            var i = m_NextSource;
            var source = VolumeLens(i);

            // blend in the tone
            source.Tween(0.0f, m_MaxVolume, dur: blend);

            // play the tone
            PlayTone(loop.Curr(), key);
            loop.Advance();
            yield return new WaitForSeconds(interval);

            // stop if this loop was cancelled
            if (m_Loop != loop) {
                break;
            }

            // blend out the tone
            source.Tween(m_MaxVolume, 0.0f, dur: blend);
        }
    }

    /// stops the active loop, if any
    public void StopLoop() {
        // if there is a loop
        if (m_Loop == null) {
            return;
        }

        // stop the loop
        StopCoroutine(m_Routine);

        // fade out
        var master = VolumeLens();
        master.Tween(m_MaxVolume, 0.0f, dur: m_Loop.Fade);

        // reset state
        m_Loop = null;
        m_Routine = null;
    }

    // -- queries --
    public bool IsPlayingLoop {
        get => m_Loop != null;
    }

    /// get a lens for the master volume
    Lens<float> VolumeLens() {
        return new Lens<float>(
            ( ) => m_Volume,
            (v) => m_Volume = Mathf.Min(v, m_MaxVolume)
        );
    }

    /// gets a lens for the a source's volume
    Lens<float> VolumeLens(int i) {
        var s = m_Sources[i];

        return new Lens<float>(
            ( ) => m_VolumeBySource[i],
            (v) => m_VolumeBySource[i] = Mathf.Min(v, m_MaxVolume)
        );
    }
}

}