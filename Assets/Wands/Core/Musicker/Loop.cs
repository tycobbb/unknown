/// a loop
public sealed class Loop {
    // -- props --
    /// the index of the current tone
    int m_Curr;

    /// the tones in this progression
    readonly Tone[] m_Tones;

    /// the fade time in seconds
    float m_Fade;

    /// the blend time in seconds
    float m_Blend;

    // -- lifetime --
    /// create a new loop
    public Loop(
        float fade = 0.0f,
        float blend = 0.0f,
        params Tone[] tones
    ) {
        m_Curr = 0;
        m_Fade = fade;
        m_Blend = blend;
        m_Tones = tones;
    }

    // -- commands --
    /// move to the next tone
    public void Advance() {
        var next = m_Curr + 1;
        m_Curr = next % m_Tones.Length;
    }

    // -- queries --
    /// the current tone
    public Tone Curr() {
        return m_Tones[m_Curr];
    }

    /// the fade time in seconds
    public float Fade {
        get => m_Fade;
    }

    /// the blend time in seconds
    public float Blend {
        get => m_Blend;
    }
}