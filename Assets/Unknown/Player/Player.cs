using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;

/// a player
public class Player: MonoBehaviour {
    // -- tuning --
    [Header("tuning")]
    [Tooltip("the hitbox at the end of the line")]
    [SerializeField] PlayerHitbox mHitbox;

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

    [Tooltip("the player's ghost endpoint")]
    [SerializeField] Transform mGhost;

    [Tooltip("the player's ghost trail")]
    [SerializeField] Shapes.Line mTrail;

    [Tooltip("plays music on contact")]
    [SerializeField] Musicker mMusic;

    [Tooltip("the input system input")]
    [SerializeField] PlayerInput mInput;

    // -- props --
    /// the musical key
    Key mKey;

    /// the line to play as the anchor changes
    Line mAnchorLine;

    /// the line pattern
    Pattern mPattern;

    /// the pattern's previous anchor index
    int mPrevAnchorIndex;

    /// the flick windup offset
    Vector2 mFlickOffset;

    /// the release gesture
    FlickRelease? mFlickRelease;

    /// the player's inputs
    PlayerActions mActions;

    // -- lifecycle --
    void Awake() {
        // set props
        mPattern = new Pattern();
        mActions = new PlayerActions(mInput);
        mAnchorLine = new Line(
            Tone.I,
            Tone.III,
            Tone.V,
            Tone.VII
        );
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

    void OnDrawGizmos() {
        Handles.color = Color.magenta;
        Handles.DrawSolidDisc(mHitbox.Position, Vector3.forward, mHitbox.Radius);
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
        // get angle relative to down vector
        var a = -Vector2.SignedAngle(Vector2.down, mActions.Move);
        a = Mathf.Repeat(a + 360.0f, 360.0f);

        // apply as a percentage to pattern, down is 0%/100%
        mPattern.MoveTo(a / 360.0f);

        // see if the anchor index changed
        var i = mPattern.AnchorIndex;
        if (mPrevAnchorIndex != i) {
            mPrevAnchorIndex = i;
            mMusic.PlayTone(mAnchorLine[i], mKey);
        }
    }

    /// read flick offset
    void ReadFlick() {
        // capture prev and next offset
        var prev = mFlickOffset;
        var next = mActions.Flick;

        // update state
        mFlickOffset = next;

        // if there is no active release, check for one
        if (mFlickRelease == null) {
            ReadFlickRelease(prev, next);
        }
    }

    /// read release gesture given prev and next offset
    void ReadFlickRelease(Vector2 prev, Vector2 next) {
        // check alignment between previous direction and movement direction
        var delta = next - prev;
        var align = Vector2.Dot(prev.normalized, delta.normalized);

        // if it flipped (dot is ~ -1.0f), this is a release
        if (Mathf.Abs(align + 1.0f) < mReleaseAlignment) {
            mFlickRelease = new FlickRelease(
                time: Time.time,
                strength: delta.magnitude,
                offset: prev
            );
        }
    }

    /// move line into position
    void Move() {
        // get pattern pos
        var p0 = mPattern.Point0;
        var p1 = mPattern.Point1;

        // get flick offset
        var offset = mFlickOffset;

        // apply the release if necessary
        TryReleaseFlick(ref offset);

        // get the flick-adjusted endpoint
        var pe = p1 + offset * mWindupLength;

        // move the hitbox
        mHitbox.Position = pe;

        // render shapes
        mLine.Start = p0;
        mLine.End = pe;

        mTrail.Start = p1;
        mTrail.End = pe;

        mGhost.localPosition = p1;
    }

    /// release the flick, if necessary, modifying the offset
    void TryReleaseFlick(ref Vector2 offset) {
        if (mFlickRelease == null) {
            return;
        }

        // check elapsed time
        var release = mFlickRelease.Value;
        var percent = (Time.time - release.Time) / mReleaseDuration;

        // if the release finished, clear it
        if (percent >= 1.0f) {
            mFlickRelease = null;
        }
        // otherwise, use the release offset instead
        else {
            var curved = mReleaseCurve.Evaluate(percent);
            if (curved > 1.0f) {
                curved = 1.0f + (curved - 1.0f) * release.Strength;
            }

            offset = Vector2.LerpUnclamped(release.Offset, Vector2.zero, curved);
        }
    }

    // -- queries --
    /// check if the player's overlap
    public bool Overlaps(Player other) {
        return mHitbox.Overlaps(other.mHitbox);
    }

    // -- events --
    public void OnCollision(Player other) {
        Debug.Log($"{name} colliding w/ {other.name}!");
    }

    // -- gestures --
    /// the flick release gesture initial state
    readonly struct FlickRelease {
        /// the time on release
        public readonly float Time;

        /// the strength of the flick
        public readonly float Strength;

        /// the flick offset on release
        public readonly Vector2 Offset;

        // -- lifetime --
        /// create a new gesture
        public FlickRelease(float time, float strength, Vector2 offset) {
            Time = time;
            Strength = strength;
            Offset = offset;
        }
    }
}