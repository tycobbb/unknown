using UnityEngine;

public class Shape: MonoBehaviour {
    // -- lifecycle --
    void Update() {
        // draw line
        var pct = Mathf.Repeat(Time.time, 4.0f) / 4.0f;
        var anim = new Animation(pct);

        var p0 = (Vector3)anim.Point0;
        var p1 = (Vector3)anim.Point1;

        p0.x -= 0.5f;
        p1.x -= 0.5f;

        Debug.DrawLine(p0, p1, Color.green);

        // draw box
        // var b0 = new Vector3(-0.5f, -0.5f);
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
    }

    readonly struct Animation {
        // -- props --
        /// the first point
        public readonly Vector2 Point0;

        /// the second point
        public readonly Vector2 Point1;

        // --  lifetime --
        /// create a new shape from a percent
        public Animation(float pct) {
            // math constants
            const float k2Pi = Mathf.PI * 2.0f;
            const float kPi2 = Mathf.PI * 0.5f;

            // calculate anchor point
            Point0.x = Mathf.Round(Mathf.Repeat(pct, 1.0f));
            Point0.y = Mathf.Round(Mathf.Repeat(pct + 0.25f, 1.0f));

            // start from the anchor
            Point1 = Point0;

            // add apply the rotation
            var a0 = k2Pi - Mathf.Floor(pct * 4.0f) * kPi2;
            var at = a0 + Mathf.Repeat(pct, 0.25f) * k2Pi;

            Point0.x += Mathf.Cos(at);
            Point0.y += Mathf.Sin(at);
        }
    }
}
