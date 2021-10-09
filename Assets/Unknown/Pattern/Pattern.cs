using UnityEngine;

/// the box and line pattern
public class Pattern: MonoBehaviour {
    // -- props --
    /// the current pattern state
    State mState = State.Zero;

    // -- lifecycle --
    void Update() {
        // draw line
        var p0 = (Vector3)mState.Point0;
        var p1 = (Vector3)mState.Point1;

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

    // -- commands --
    public void SetPercent(float pct) {
        mState = State.At(pct);
    }

    // -- state --
    readonly struct State {
        // -- props --
        /// the first point
        public readonly Vector2 Point0;

        /// the second point
        public readonly Vector2 Point1;

        // --  lifetime --
        /// create a new state
        State(Vector2 p0, Vector2 p1) {
            Point0 = p0;
            Point1 = p1;
        }

        // -- factories --
        /// the zero state
        public readonly static State Zero = At(0.0f);

        /// get the state at this percent
        public static State At(float pct) {
            // math constants
            const float k2Pi = Mathf.PI * 2.0f;
            const float kPi2 = Mathf.PI * 0.5f;

            // calculate anchor point
            Vector2 p0;
            p0.x = Mathf.Round(Mathf.Repeat(pct, 1.0f));
            p0.y = Mathf.Round(Mathf.Repeat(pct + 0.25f, 1.0f));

            // start from the anchor
            Vector2 p1 = p0;

            // add apply the rotation
            var a0 = k2Pi - Mathf.Floor(pct * 4.0f) * kPi2;
            var at = a0 + Mathf.Repeat(pct, 0.25f) * k2Pi;

            p1.x += Mathf.Cos(at);
            p1.y += Mathf.Sin(at);

            return new State(p0, p1);
        }
    }
}
