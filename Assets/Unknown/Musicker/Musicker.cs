using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

/// a thing that plays music
public class Musicker: MonoBehaviour {
    // -- config --
    [Header("config")]
    [Tooltip("the number of audio sources to create or keep")]
    [SerializeField] int m_NumSources = 4;

    [Tooltip("the audio source to realize sound")]
    [SerializeField] List<AudioSource> m_Sources;

    [Tooltip("their current instrument")]
    [SerializeField] Instrument m_Instrument;

    // -- props --
    /// the index of the next available audio source
    int m_NextSource = 0;

    /// the master volume for all audio sources
    /// TODO: this only works w/ loops right now
    float m_Volume = 1.0f;

    /// the per-source volumes
    /// TODO: hmm
    float[] m_SourceVolumes;

    /// the id of the current loop
    int m_LoopId;

    // -- lifecycle --
    void Awake() {
        var go = gameObject;

        // init volumes
        m_SourceVolumes = new float[m_NumSources];

        // create audio sources
        for (var i = m_Sources.Count; i < m_NumSources; i++) {
            m_Sources.Add(go.AddComponent<AudioSource>());
            m_SourceVolumes[i] = 1.0f;
        }
    }

    // -- commands --
    /// play the current tone in the line and advance it
    public void PlayLine(Line line, Key? key = null) {
        PlayTone(line.Curr(), key);
        line.Advance();
    }

    /// play the current chord in a progression and advance it
    public void PlayProgression(Progression prog, float interval = 0.0f, Key? key = null) {
        PlayArp(prog.Curr(), interval, key);
        prog.Advance();
    }

    /// toggle the loop
    public void ToggleLoop(Loop loop, bool isPlaying, Key? key = null) {
        if (isPlaying) {
            PlayLoop(loop, key);
        } else {
            StopLoop(loop);
        }
    }

    /// play the loop
    public void PlayLoop(Loop loop, Key? key = null) {
        Reset();
        StartCoroutine(PlayLoopAsync(loop, key));
    }

    /// play the loop
    IEnumerator PlayLoopAsync(Loop loop, Key? key = null) {
        m_LoopId++;
        var id = m_LoopId;

        // fade in the loop
        StartCoroutine(FadeIn(MasterVolume(), loop.Fade));

        // the time between loop plays
        var blend = loop.Blend;
        var interval = m_Instrument.Duration - blend;

        // play the tones in sequence until stopped
        while (true) {
            var source = NextSourceVolume();

            // blend in the tone
            StartCoroutine(FadeIn(source, blend));

            // play the tone
            PlayTone(loop.Curr(), key);
            loop.Advance();
            yield return new WaitForSeconds(interval);

            // stop if this loop was cancelled
            if (id != m_LoopId) {
                break;
            }

            // blend out the tone
            StartCoroutine(FadeOut(source, blend));
        }
    }

    /// stops the active loop, if any
    public void StopLoop(Loop loop) {
        if (IsPlayingLoop) {
            m_LoopId++;
            StartCoroutine(StopLoopAsync(loop));
        }
    }

    /// stops the active loop, if any
    IEnumerator StopLoopAsync(Loop loop) {
        yield return FadeOut(MasterVolume(), loop.Fade);
        Reset();
    }

    /// play the clips in the chord
    public void PlayChord(Chord chord, Key? key = null) {
        PlayArp(chord, 0.0f, key);
    }

    /// play the clips in the chord, pass an interval to arpeggiate
    public void PlayArp(Chord chord, float interval, Key? key = null) {
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
    /// set the pitch of this player
    public void SetPitch(float pitch) {
        foreach (var source in m_Sources) {
            source.pitch = pitch;
        }
    }

    // -- c/helpers
    /// play a clip on the next source
    void PlayClip(AudioClip clip) {
        // play the clip
        var source = NextSource();
        source.clip = clip;
        source.Play();

        // advance the source
        m_NextSource = (m_NextSource + 1) % m_NumSources;
    }

    /// fade in the volume to its current volume over the duration
    IEnumerator FadeIn(Lens<float> vol, float duration) {
        yield return Fade(vol, duration, 0.0f, 1.0f);
    }

    /// fade out the volume from its current volume over the duration
    IEnumerator FadeOut(Lens<float> vol, float duration) {
        yield return Fade(vol, duration, 1.0f, 0.0f);
    }

    /// fade the volume from v0 to v1 over the duration
    IEnumerator Fade(Lens<float> vol, float duration, float v0, float v1) {
        // get initial state
        var t0 = Time.time;

        // fade in until the duration elapses
        while (true) {
            var pct = (Time.time - t0) / duration;
            if (pct >= 1.0f) {
                break;
            }

            vol.Val = Mathf.Lerp(v0, v1, pct);
            yield return null;
        }

        // restore original volume
        Reset(vol);
    }

    /// HACK: cancel all operations and reset all the volumes
    void Reset() {
        StopAllCoroutines();

        m_Volume = 1.0f;

        for (var i = 0; i < m_NextSource; i++) {
            m_Sources[i].volume = 1.0f;
            m_SourceVolumes[i] = 1.0f;
        }
    }

    /// HACK: reset the volume
    void Reset(Lens<float> vol) {
        vol.Val = 1.0f;
    }

    // -- props/hot
    /// the current instrument
    public Instrument Instrument {
        get => m_Instrument;
        set => m_Instrument = value;
    }

    // -- queries --
    /// if there is a loop playing
    public bool IsPlayingLoop {
        get => m_LoopId % 2 == 0;
    }

    /// if the musicker has any sources available
    public bool IsAvailable() {
        foreach (var source in m_Sources) {
            if (!source.isPlaying) {
                return true;
            }
        }

        return false;
    }

    /// get the next audio source
    AudioSource NextSource() {
        return m_Sources[m_NextSource];
    }

    /// get a lens for the master volume
    Lens<float> MasterVolume() {
        return new Lens<float>(
            ( ) => m_Volume,
            (v) => m_Volume = v
        );
    }

    /// gets a lens for the next source's volume
    Lens<float> NextSourceVolume() {
        var i = m_NextSource;
        var s = m_Sources[i];

        return new Lens<float>(
            ( ) => m_SourceVolumes[i],
            (v) => {
                m_SourceVolumes[i] = v;
                s.volume = v * m_Volume;
            }
        );
    }
}