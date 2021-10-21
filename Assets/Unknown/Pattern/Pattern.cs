using UnityEngine;

/// the line pattern
public struct Pattern {
    // -- props --
    /// the anchor point
    public Vector2 Point0 { get; private set; }

    /// the floating point
    public Vector2 Point1 { get; private set; }

    // -- p/private
    /// the current percent
    float m_Percent;

    // -- commands --
    public void MoveTo(float pct) {
        // loop pct in [0,1]
        pct = Mathf.Repeat(pct, 1.0f);

        // work around values that are a multiple of 0.25f
        var pcti = Mathf.Clamp01(pct - 0.0001f);

        // find anchor point
        Vector2 p0;
        p0.x = Mathf.Round(pcti);
        p0.y = Mathf.Round(Mathf.Repeat(pcti + 0.25f, 1.0f));

        // math constants
        const float k2Pi = Mathf.PI * 2.0f;
        const float kPi2 = Mathf.PI * 0.5f;

        // get the rotation
        var a0 = k2Pi - Mathf.Floor(pcti * 4.0f) * kPi2;
        var at = a0 + Mathf.Repeat(pcti, 0.25f) * k2Pi;

        // find end point
        var p1 = p0;
        p1.x += Mathf.Cos(at);
        p1.y += Mathf.Sin(at);

        // update props
        Point0 = p0;
        Point1 = p1;

        // set internal pct using actual value
        m_Percent = pct;
    }

    // -- queries --
    /// the current percent
    public float Percent {
        get => m_Percent;
    }

    /// the current anchor index
    public int AnchorIndex {
        get => Mathf.Min(Mathf.FloorToInt(m_Percent / 0.25f), 3);
    }
}
