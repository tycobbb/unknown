/// a note progression
public sealed class Line {
    // -- props --
    /// the index of the current tone
    int mCurr;

    /// the tones in this progression
    readonly Tone[] mTones;

    // -- lifetime --
    /// create a new line
    public Line(params Tone[] tones) {
        mCurr = 0;
        mTones = tones;
    }

    // -- commands --
    /// move to the next tone
    public void Advance() {
        var next = mCurr + 1;
        mCurr = next % mTones.Length;
    }

    // -- queries --
    /// the current tone
    public Tone Curr() {
        return mTones[mCurr];
    }

    /// the tone at the position
    public Tone this[int i] {
        get => mTones[i];
    }
}