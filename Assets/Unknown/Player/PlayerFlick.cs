using UnityEngine;
using UnityEngine.InputSystem;

/// the player's flick gesture
public class PlayerFlick: MonoBehaviour {
    // -- tuning --
    [Header("tuning")]
    [Tooltip("the max length of the flick windup")]
    [SerializeField] float mScale = 0.1f;

    [Tooltip("the max pitch shift of the flick windup")]
    [SerializeField] float mPitchScale = 0.3f;

    [Tooltip("the curve for the flick release")]
    [SerializeField] AnimationCurve mReleaseCurve;

    [Tooltip("the duration for the flick release")]
    [SerializeField] float mReleaseDuration = 0.1f;

    [Tooltip("the min alignment to detect a flick release")]
    [SerializeField] float mReleaseAlignment = 0.1f;

    // -- nodes --
    [Header("nodes")]
    [Tooltip("the input system input")]
    [SerializeField] PlayerInput mInput;

    // -- props --
    /// the control offset
    Vector2 mOffset;

    /// the player's inputs
    PlayerActions mActions;

    // -- p/release
    /// the current frame
    int? mReleaseFrame;

    /// the time on release
    float mReleaseTime;

    /// the initial release offset
    Vector2 mReleaseInitial;

    /// the release offset
    Vector2 mReleaseOffset;

    /// the strength of the release
    float mReleaseStrength;

    /// the current release speed
    float mReleaseSpeed;

    // -- lifecycle --
    void Awake() {
        // set props
        mActions = new PlayerActions(mInput);
    }

    void Update() {
        // read input
        Read();
    }

    // -- commands --
    /// read flick
    void Read() {
        // capture prev and next offset
        var prev = mOffset;
        var next = mActions.Flick;

        // update state
        mOffset = next;

        // read a release gesture
        ReadRelease(prev, next);
    }

    /// read release given prev and next offset
    void ReadRelease(Vector2 prev, Vector2 next) {
        // unless the release is frame locked
        if (IsReleaseLocked) {
            return;
        }

        // check alignment between previous dir and movement dir
        var delta = next - prev;
        var align = Vector2.Dot(prev.normalized, delta.normalized);

        // if align flipped (dot is ~ -1.0f), this is a release
        if (Mathf.Abs(align + 1.0f) > mReleaseAlignment) {
            return;
        }

        // init the release if this is the first frame
        if (!IsReleasing) {
            mReleaseFrame = 0;
            mReleaseTime = Time.time;
            mReleaseInitial = prev;
            mReleaseStrength = 0.0f;
        }

        // buffer strength
        mReleaseStrength = Mathf.Max(mReleaseStrength, delta.magnitude);
    }

    /// release the flick, if necessary, modifying the offset
    public void TryRelease() {
        if (!IsReleasing) {
            return;
        }

        // check elapsed time
        var pct = (Time.time - mReleaseTime) / mReleaseDuration;

        // if the release finished, clear it
        if (pct >= 1.0f) {
            mReleaseFrame = null;
            return;
        }

        // otherwise, figure out where we are on the release curve
        pct = mReleaseCurve.Evaluate(pct);
        if (pct > 1.0f) {
            pct = 1.0f + (pct - 1.0f) * mReleaseStrength;
        }

        // calculate the next offset
        var prev = mReleaseOffset;
        var next = Vector2.LerpUnclamped(mReleaseInitial, Vector2.zero, pct);

        // track the offset and speed
        mReleaseOffset = next;
        mReleaseSpeed = Vector2.Distance(prev, next) / Time.deltaTime;
    }

    // -- queries --
    /// find current scaled offset
    public Vector2 Offset {
        get => UnscaledOffset * mScale;
    }

    /// the current pitch shift
    public float PitchShift {
        get => UnscaledOffset.magnitude * mPitchScale;
    }

    /// the current release speed
    public float Speed {
        get => mReleaseSpeed;
    }

    /// if the release gesture is active
    public bool IsReleasing {
        get => mReleaseFrame > -1;
    }

    /// if the release gesture is frame locked
    bool IsReleaseLocked {
        get => mReleaseFrame > 1;
    }

    /// the current unscaled offset, depending on state
    Vector2 UnscaledOffset {
        get => IsReleasing ? mReleaseOffset : mOffset;
    }
}