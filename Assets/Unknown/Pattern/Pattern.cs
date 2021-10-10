using UnityEngine;

/// the box and line pattern
public class Pattern: MonoBehaviour {
    // -- config --
    [Header("config")]
    [Tooltip("the pattern's color")]
    [SerializeField] Color mColor = Color.magenta;

    // -- props --
    Vector2 mPoint0;
    Vector2 mPoint1;

    // -- lifecycle --
    void Update() {
        // draw box
        var b0 = new Vector3(-0.5f, 0.0f);
        var b1 = b0;

        b1.x += 1.0f;
        Debug.DrawLine(b0, b1, Color.magenta);

        b0 = b1;
        b1.y += 1.0f;
        Debug.DrawLine(b0, b1, Color.magenta);

        b0 = b1;
        b1.x -= 1.0f;
        Debug.DrawLine(b0, b1, Color.magenta);

        b0 = b1;
        b1.y -= 1.0f;
        Debug.DrawLine(b0, b1, Color.magenta);

        // draw line
        var p0 = (Vector3)mPoint0;
        var p1 = (Vector3)mPoint1;

        p0.x -= 0.5f;
        p1.x -= 0.5f;

        Debug.Log($"color {mColor}");
        Debug.DrawLine(p0, p1, mColor);
    }

    // -- commands --
    public void SetPercent(float pct) {
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
        mPoint0 = p0;
        mPoint1 = p1;
    }
}
