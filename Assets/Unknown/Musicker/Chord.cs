using System.Linq;

/// a chord w/ a key and quality
public readonly struct Chord {
    // -- props --
    /// the chord tones
    readonly Tone[] mTones;

    // -- lifetime --
    /// create a chord from a list of tones
    public Chord(params Tone[] tones) {
        mTones = tones;
    }

    /// create a chord from a root note and a chord quality, building its tones
    public Chord(Tone root, Quality quality) {
        // create the right number of tones
        var n = quality.Length;
        var tones = new Tone[n];

        // transpose each tone from the root
        for (var i = 0; i < n; i++) {
            var tone = quality[i];
            tones[i] = tone.Transpose(root);
        }

        // build a chord
        mTones = tones;
    }

    // -- queries --
    /// the number of notes in this chord
    public int Length {
        get => mTones.Length;
    }

    /// the tone at the position
    public Tone this[int i] {
        get => mTones[i];
    }

    // -- debugging --
    public override string ToString() {
        return string.Join(" ", mTones.Select((n) => n.ToString()));
    }
}