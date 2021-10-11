using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;

/// a player
public class Player: MonoBehaviour {
    // -- deps --
    /// the score module
    Score mScore;

    // -- tuning --
    [Header("tuning")]
    [Tooltip("the hitbox at the end of the line")]
    [SerializeField] PlayerHitbox mHitbox;

    [Tooltip("the move speed, degrees for a full rotation")]
    [SerializeField] float mMoveSpeed = 720.0f;

    [Tooltip("the max length of the flick windup")]
    [SerializeField] float mFlickWindup = 0.1f;

    [Tooltip("the max pitch shift of the flick windup")]
    [SerializeField] float mFlickPitch = 0.3f;

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

    [FormerlySerializedAs("mMusic")]
    [Tooltip("plays voice music")]
    [SerializeField] Musicker mVoice;

    [Tooltip("plays footstep music")]
    [SerializeField] Musicker mFootsteps;

    [Tooltip("the input system input")]
    [SerializeField] PlayerInput mInput;

    // -- props --
    /// the player's config
    PlayerConfig mConfig;

    /// the musical key
    Key mKey;

    /// the line pattern
    Pattern mPattern;

    /// the current move dir
    Vector2 mMoveDir;

    /// the accumulated move angle
    float mMoveAngle;

    /// the flick windup offset
    Vector2 mFlickOffset;

    /// the release gesture
    FlickRelease? mFlickRelease;

    /// the line to play as the anchor changes
    Line mVoiceLine;

    /// the loop to play as the player moves
    Loop mFootstepsLoop;

    /// the pattern's previous anchor index
    int mPrevAnchorIndex = -1;

    /// the player's inputs
    PlayerActions mActions;

    // -- lifecycle --
    void Awake() {
        // set deps
        mScore = Score.Get;

        // set props
        mPattern = new Pattern();
        mActions = new PlayerActions(mInput);

        mVoiceLine = new Line(
            Tone.III,
            Tone.V,
            Tone.VII,
            Tone.I.Octave()
        );

        mFootstepsLoop = new Loop(
            fade: 1.5f,
            blend: 0.6f,
            Tone.I
        );

        // apply config
        var cfg = mConfig;
        name = cfg.Name;
        mKey = new Key(cfg.Key);

        // set line props
        mShape.Color = cfg.Color;

        // set music props
        mVoice.Instrument = cfg.VoiceInstrument;
        mFootsteps.Instrument = cfg.FootstepsInstrument;

        // show score
        mScore.AddPlayer(cfg);
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
        mConfig = cfg;
    }

    /// read move pattern
    void ReadMove() {
        var nDir = mActions.Move;
        var pDir = mMoveDir;

        // update move dir
        mMoveDir = nDir;

        // add the angle between prev and next to the move angle
        if (pDir != Vector2.zero) {
            mMoveAngle = Mathf.Repeat(
                mMoveAngle + Vector2.SignedAngle(nDir, pDir),
                mMoveSpeed
            );
        }

        // apply as a percentage to pattern, down is 0%/100%
        mPattern.MoveTo(mMoveAngle / mMoveSpeed);

        // play footsteps when moving
        var isMoving = nDir != Vector2.zero;
        if (isMoving != mFootsteps.IsPlayingLoop) {
            mFootsteps.ToggleLoop(mFootstepsLoop, isMoving, mKey);
        }

        // play voice when anchor changes
        var i = mPattern.AnchorIndex;
        if (mPrevAnchorIndex != i) {
            mPrevAnchorIndex = i;
            mVoice.PlayTone(mVoiceLine[i], mKey);
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

        // update move pitch based on offset
        mFootsteps.SetPitch(1.0f + offset.magnitude * mFlickPitch);

        // get the flick-adjusted endpoint
        var pe = p1 + offset * mFlickWindup;

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