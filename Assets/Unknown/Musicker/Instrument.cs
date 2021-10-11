using UnityEngine;

/// produces any note in the chromatic scale
public class Instrument: MonoBehaviour {
    // -- config --
    [Header("config")]
    [Tooltip("the chromatic scale (usually C3-based); must have a multiple of 12 notes")]
    [SerializeField] AudioClip[] mScale;

    // -- lifecycle --
    void Awake() {
        if (mScale.Length % 12 != 0) {
            Debug.LogError($"{this} has an incomplete octave");
        }
    }

    // -- queries --
    /// the duration of any clip
    public float Duration {
        get => mScale[0].length;
    }

    /// find a random audio clip
    public AudioClip RandClip() {
        return mScale[Random.Range(0, Length)];
    }

    /// find the clip for a tone
    public AudioClip FindClip(Tone tone) {
        return mScale[tone.Steps % Length];
    }

    /// the length of the scale
    int Length {
        get => mScale.Length;
    }
}