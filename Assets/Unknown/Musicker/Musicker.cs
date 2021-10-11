using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// a thing that plays music
public class Musicker: MonoBehaviour {
    // -- config --
    [Header("config")]
    [Tooltip("the number of audio sources to create or keep")]
    [SerializeField] int mNumSources = 4;

    [Tooltip("the audio source to realize sound")]
    [SerializeField] List<AudioSource> mSources;

    [Tooltip("their current instrument")]
    [SerializeField] Instrument mInstrument;

    // -- props --
    /// the index of the next available audio source
    int mNextSource = 0;

    /// the master volume for all audio sources
    /// TODO: this only works w/ loops right now
    float mVolume = 1.0f;

    /// the per-source volumes
    /// TODO: hmm
    float[] mSourceVolumes;

    /// the id of the current loop
    int mLoopId;

    // -- lifecycle --
    void Awake() {
        var go = gameObject;

        // init volumes
        mSourceVolumes = new float[mNumSources];

        // create audio sources
        for (var i = mSources.Count; i < mNumSources; i++) {
            mSources.Add(go.AddComponent<AudioSource>());
            mSourceVolumes[i] = 1.0f;
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
        PlayChord(prog.Curr(), interval, key);
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
        mLoopId++;
        var id = mLoopId;

        // fade in the loop
        StartCoroutine(FadeIn(MasterVolume(), loop.Fade));

        // the time between loop plays
        var blend = loop.Blend;
        var interval = mInstrument.Duration - blend;

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
            if (id != mLoopId) {
                break;
            }

            // blend out the tone
            StartCoroutine(FadeOut(source, blend));
        }
    }

    /// stops the active loop, if any
    public void StopLoop(Loop loop) {
        if (IsPlayingLoop) {
            mLoopId++;
            StartCoroutine(StopLoopAsync(loop));
        }
    }

    /// stops the active loop, if any
    IEnumerator StopLoopAsync(Loop loop) {
        yield return FadeOut(MasterVolume(), loop.Fade);
        Reset();
    }

    /// play the clips in the chord. pass an interval to arpeggiate.
    public void PlayChord(Chord chord, float interval = 0.0f, Key? key = null) {
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
        PlayClip(mInstrument.FindClip(tone));
    }

    /// play a random audio clip
    public void PlayRand() {
        PlayClip(mInstrument.RandClip());
    }

    // -- c/helpers
    /// play a clip on the next source
    void PlayClip(AudioClip clip) {
        // play the clip
        var source = NextSource();
        source.clip = clip;
        source.Play();

        // advance the source
        mNextSource = (mNextSource + 1) % mNumSources;
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

        mVolume = 1.0f;

        for (var i = 0; i < mNextSource; i++) {
            mSources[i].volume = 1.0f;
            mSourceVolumes[i] = 1.0f;
        }
    }

    /// HACK: reset the volume
    void Reset(Lens<float> vol) {
        vol.Val = 1.0f;
    }

    // -- props/hot
    /// the current instrument
    public Instrument Instrument {
        get => mInstrument;
        set => mInstrument = value;
    }

    // -- queries --
    /// if there is a loop playing
    public bool IsPlayingLoop {
        get => mLoopId % 2 == 0;
    }

    /// if the musicker has any sources available
    public bool IsAvailable() {
        foreach (var source in mSources) {
            if (!source.isPlaying) {
                return true;
            }
        }

        return false;
    }

    /// get the next audio source
    AudioSource NextSource() {
        return mSources[mNextSource];
    }

    /// get a lens for the master volume
    Lens<float> MasterVolume() {
        return new Lens<float>(
            ( ) => mVolume,
            (v) => mVolume = v
        );
    }

    /// gets a lens for the next source's volume
    Lens<float> NextSourceVolume() {
        var i = mNextSource;
        var s = mSources[i];

        return new Lens<float>(
            ( ) => mSourceVolumes[i],
            (v) => {
                mSourceVolumes[i] = v;
                s.volume = v * mVolume;
            }
        );
    }
}