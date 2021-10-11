/// a loop
public sealed class Loop {
    // -- props --
    /// the index of the current tone
    int mCurr;

    /// the tones in this progression
    readonly Tone[] mTones;

    /// the fade time in seconds
    float mFade;

    /// the blend time in seconds
    float mBlend;

    // -- lifetime --
    /// create a new loop
    public Loop(
        float fade = 0.0f,
        float blend = 0.0f,
        params Tone[] tones
    ) {
        mCurr = 0;
        mFade = fade;
        mBlend = blend;
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

    /// the fade time in seconds
    public float Fade {
        get => mFade;
    }

    /// the blend time in seconds
    public float Blend {
        get => mBlend;
    }
}