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
    float mPercent;

    // -- commands --
    public void MoveTo(float pct) {
        // find anchor point
        Vector2 p0;
        p0.x = Mathf.Round(Mathf.Repeat(pct, 1.0f));
        p0.y = Mathf.Round(Mathf.Repeat(pct + 0.25f, 1.0f));

        // math constants
        const float k2Pi = Mathf.PI * 2.0f;
        const float kPi2 = Mathf.PI * 0.5f;

        // get the rotation
        var a0 = k2Pi - Mathf.Floor(pct * 4.0f) * kPi2;
        var at = a0 + Mathf.Repeat(pct, 0.25f) * k2Pi;

        // find end point
        var p1 = p0;
        p1.x += Mathf.Cos(at);
        p1.y += Mathf.Sin(at);

        // update props
        Point0 = p0;
        Point1 = p1;
        mPercent = pct;
    }

    // -- queries --
    /// the current percent
    public float Percent {
        get => mPercent;
    }

    /// the current anchor index
    public int AnchorIndex {
        get => Mathf.Min(Mathf.FloorToInt(mPercent / 0.25f), 3);
    }
}
