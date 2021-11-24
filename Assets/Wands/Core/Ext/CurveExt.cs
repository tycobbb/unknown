using UnityEngine;

/// unity curve extensions
public static class CurveExt {
    // -- queries --
    /// the duration of the curve
    public static float Duration(this AnimationCurve c) {
        return c.keys[c.length - 1].time;
    }

    /// sample a random value from the curve
    public static float Sample(this AnimationCurve c) {
        return c.Evaluate(Random.value);
    }

    /// sample a random value from the curve
    public static int SampleInt(this AnimationCurve c) {
        return Mathf.FloorToInt(c.Sample());
    }
}