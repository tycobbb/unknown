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

    // -- lifecycle --
    void Awake() {
        // create any necessary audio sources
        var go = gameObject;
        for (var i = mSources.Count; i < mNumSources; i++) {
            mSources.Add(go.AddComponent<AudioSource>());
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

    /// play the clips in the chord. pass an interval to arpeggiate.
    public void PlayChord(Chord chord, float interval = 0.0f, Key? key = null) {
        StartCoroutine(PlayChordAsync(chord, interval, key));
    }

    /// play the clips in the chord. pass an interval to arpeggiate.
    public IEnumerator PlayChordAsync(Chord chord, float interval = 0.0f, Key? key = null) {
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

    /// play a clip on the next source
    void PlayClip(AudioClip clip) {
        var i = mNextSource;

        // find the audio source
        var source = mSources[i];

        // play the clip
        source.clip = clip;
        source.Play();

        // advance the source
        mNextSource = (i + 1) % mNumSources;
    }

    // -- c/config
    /// set the instrument
    public void SetInstrument(Instrument instrument) {
        mInstrument = instrument;
    }

    // -- queries --
    /// if the musicker has any sources available
    public bool IsAvailable() {
        foreach (var source in mSources) {
            if (!source.isPlaying) {
                return true;
            }
        }

        return false;
    }
}