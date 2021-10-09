using UnityEngine;

public class Shape: MonoBehaviour {
    // -- lifecycle --
    void Update() {
        var pct = Mathf.Repeat(Time.time, 4.0f) / 4.0f;

        var p0 = Vector3.zero;
        p0.x = GetAnimValue(pct + 0.75f);
        p0.y = GetAnimValue(pct + 0.75f);

        var p1 = Vector3.zero;
        p1.x = GetAnimValue(pct + 0.50f);
        p1.y = GetAnimValue(pct + 0.00f);

        p0.x -= 0.5f;
        p1.x -= 0.5f;

        Debug.DrawLine(p0, p1, Color.green);
    }

    /// gets the value ([0,1]) to run the line animation at a given pct offset
    float GetAnimValue(float pct) {
        return Mathf.Clamp01((Mathf.Abs(Mathf.Repeat(pct, 1.0f) - 0.375f) - 0.125f) * 4.0f);
    }
}
