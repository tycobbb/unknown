using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;

/// a player
public class Player: MonoBehaviour {
    // -- types --
    struct Release {
        /// the flick offset on release
        Vector2 Offset;
        /// the time on release
        float Time;
    }
    // -- tuning --
    [FormerlySerializedAs("mFlickLength")]
    [Header("tuning")]
    [Tooltip("the max length of the flick windup")]
    [SerializeField] float mWindupLength = 0.1f;

    [Tooltip("the curve for the flick release")]
    [SerializeField] AnimationCurve mReleaseCurve;

    [Tooltip("the duration for the flick release")]
    [SerializeField] float mReleaseDuration = 0.1f;

    [Tooltip("the min alignment to detect a flick release")]
    [SerializeField] float mReleaseAlignment = 0.1f;

    // -- nodes --
    [Header("nodes")]
    [Tooltip("the player's shape")]
    [SerializeField] Shapes.ShapeGroup mShape;

    [Tooltip("the player's line")]
    [SerializeField] Shapes.Line mLine;

    [Tooltip("the player's endpoint")]
    [SerializeField] Shapes.Disc mEnd;

    [Tooltip("the player's offset line")]
    [SerializeField] Shapes.Line mOffset;

    [Tooltip("plays music on contact")]
    [SerializeField] Musicker mMusic;

    [Tooltip("the input system input")]
    [SerializeField] PlayerInput mInput;

    // -- props --
    /// the musical key
    Key mKey;

    /// the line pattern
    Pattern mPattern;

    /// the flick windup offset
    Vector2 mWindupOffset;

    /// the flick offset when released
    Vector2 mReleaseOffset;

    float? mReleaseTime;
    float mReleaseMax;

    /// the player's inputs
    PlayerActions mActions;

    // -- lifecycle --
    void Awake() {
        // set props
        mPattern = new Pattern();
        mActions = new PlayerActions(mInput);
        mReleaseMax = mReleaseCurve.keys[1].value - 1.0f;
    }

    void Update() {
        // update state from input
        ReadMove();
        ReadFlick();
    }

    void FixedUpdate() {
        // move line
        Move();
    }

    // -- commands --
    /// init this player on join
    public void Join(PlayerConfig cfg) {
        Debug.Log($"{cfg.Name} joined");

        // set props
        name = cfg.Name;
        mKey = new Key(cfg.Key);

        // set line props
        mShape.Color = cfg.Color;

        // set music props
        mMusic.Instrument = cfg.Instrument;
    }

    /// read move pattern
    void ReadMove() {
        mPattern.SetPercent(ReadPercent(mActions.Move));
    }

    /// read flick offset and velocity
    void ReadFlick() {
        // capture prev and next offset
        var prev = mWindupOffset;
        var next = mActions.Flick;

        // update windup state
        mWindupOffset = next;

        // given the stick delta
        var delta = next - prev;

        // get alignment with previous frame
        var pDir = prev.normalized;
        var rDir = delta.normalized;
        var align = Vector2.Dot(pDir, rDir);

        // if the stick was released
        if (mReleaseTime == null && Mathf.Abs(align + 1.0f) < mReleaseAlignment) {
            mReleaseTime = Time.time;
            mReleaseOffset = prev;
            mReleaseCurve.keys[1].value = 1.0f + mReleaseMax * delta.magnitude;
        }
    }

    /// move line into position
    void Move() {
        // the pattern pos
        var p0 = mPattern.Point0;
        var p1 = mPattern.Point1;

        // the flick offset
        var offset = Vector2.zero;
        if (mReleaseTime != null) {
            var elapsed = Time.time - mReleaseTime.Value;

            if (elapsed > mReleaseDuration) {
                mReleaseTime = null;
            } else {
                var curved = mReleaseCurve.Evaluate(elapsed / mReleaseDuration);
                offset = Vector2.LerpUnclamped(mReleaseOffset, Vector2.zero, curved);
            }
        }

        if (mReleaseTime == null) {
            offset = mWindupOffset;
        }

        // the flick-adjusted endpoint
        var pe = p1 + offset * mWindupLength;

        // render shapes
        mLine.Start = p0;
        mLine.End = pe;

        mOffset.Start = p1;
        mOffset.End = pe;

        mEnd.transform.localPosition = p1;
    }

    // -- queries --
    /// read percent complete from stick dir (oriented down, clockwise)
    float ReadPercent(Vector2 dir) {
        var a = -Vector2.SignedAngle(Vector2.down, dir);
        a = Mathf.Repeat(a + 360.0f, 360.0f);
        return a / 360.0f;
    }
}