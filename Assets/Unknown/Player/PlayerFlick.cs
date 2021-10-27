using UnityEngine;
using UnityEngine.InputSystem;

/// the player's flick gesture
public class PlayerFlick: MonoBehaviour {
    // -- tuning --
    [Header("tuning")]
    [Tooltip("the max length of the flick windup")]
    [SerializeField] float m_Scale = 0.1f;

    [Tooltip("the speed the flick moves towards its pos in units / s")]
    [SerializeField] float m_Speed = 0.1f;

    [Tooltip("the max pitch shift of the flick windup")]
    [SerializeField] float m_PitchScale = 0.3f;

    [Tooltip("the curve for the flick release")]
    [SerializeField] AnimationCurve m_ReleaseCurve;

    [Tooltip("the duration for the flick release")]
    [SerializeField] float m_ReleaseDuration = 0.1f;

    [Tooltip("the min alignment to detect a flick release")]
    [SerializeField] float m_ReleaseAlignment = 0.1f;

    // -- nodes --
    [Header("nodes")]
    [Tooltip("the hitstop")]
    [SerializeField] PlayerHitStop m_HitStop;

    [Tooltip("the input system input")]
    [SerializeField] PlayerInput m_Input;

    // -- props --
    /// the current offset
    Vector2 m_Offset;

    /// the target offset
    Vector2 m_DestOffset;

    /// the player's inputs
    PlayerActions m_Actions;

    // -- p/release
    /// the current frame
    int? m_ReleaseFrame;

    /// the time on release
    float m_ReleaseTime;

    /// the initial release offset
    Vector2 m_ReleaseInitial;

    /// the release offset
    Vector2 m_ReleaseOffset;

    /// the strength of the release
    float m_ReleaseStrength;

    /// the current release speed
    float m_ReleaseSpeed;

    // -- lifecycle --
    void Awake() {
        // set props
        m_Actions = new PlayerActions(m_Input);
    }

    void FixedUpdate() {
        // read input
        Read();

        // move flick
        Move();
        TryRelease();
    }

    // -- commands --
    /// read flick
    void Read() {
        // capture prev and next offset
        var prev = m_DestOffset;
        var next = m_Actions.Flick;

        // update state
        m_DestOffset = next;

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

        // if direction changed, this is a release
        if (Mathf.Abs(align + 1.0f) > m_ReleaseAlignment) {
            return;
        }

        // init the release if this is the first frame
        if (!IsReleasing) {
            m_ReleaseFrame = 0;
            m_ReleaseTime = Time.time;
            m_ReleaseInitial = prev;
            m_ReleaseStrength = 0.0f;
        }

        // buffer strength
        m_ReleaseStrength = Mathf.Max(m_ReleaseStrength, delta.magnitude);
    }

    /// move the flick towards its target position
    void Move() {
        // if not in hitstop
        if (m_HitStop.IsActive) {
            return;
        }

        // given direction to target
        var dest = m_DestOffset;
        var prev = m_Offset;
        var pdir = Vec2.Normalize(dest - prev);

        // move offset towards it
        var next = prev + pdir * m_Speed * Time.time;

        // snap if we overshot
        var ndir = dest - next;
        if (Vector2.Dot(pdir, ndir) < 0.0f) {
            next = dest;
        }

        // update state
        m_Offset = next;
    }

    /// release the flick, if necessary, modifying the offset
    void TryRelease() {
        // if not in hitstop
        if (m_HitStop.IsActive) {
            return;
        }

        // if not releasing
        if (!IsReleasing) {
            return;
        }

        // check elapsed time
        var pct = (Time.time - m_ReleaseTime) / m_ReleaseDuration;

        // if the release finished, clear it
        if (pct >= 1.0f) {
            Cancel();
            return;
        }

        // otherwise, figure out where we are on the release curve
        pct = m_ReleaseCurve.Evaluate(pct);
        if (pct > 1.0f) {
            pct = 1.0f + (pct - 1.0f) * m_ReleaseStrength;
        }

        // calculate the next offset
        var prev = m_ReleaseOffset;
        var next = Vector2.LerpUnclamped(m_ReleaseInitial, Vector2.zero, pct);

        // track the offset and speed
        m_ReleaseOffset = next;
        m_ReleaseSpeed = Vector2.Distance(prev, next) / Time.deltaTime;
    }

    /// cancels an active release, if any
    public void Cancel() {
        m_ReleaseFrame = null;
    }

    // -- queries --
    /// find current scaled offset
    public Vector2 Offset {
        get => RawOffset * m_Scale;
    }

    /// the current pitch shift
    public float PitchShift {
        get => RawOffset.magnitude * m_PitchScale;
    }

    /// the current release speed
    public float Speed {
        get => m_ReleaseSpeed;
    }

    /// if the release gesture is active
    public bool IsReleasing {
        get => m_ReleaseFrame > -1;
    }

    /// if the release gesture is frame locked
    bool IsReleaseLocked {
        get => m_ReleaseFrame > 1;
    }

    /// the current unscaled offset, depending on state
    Vector2 RawOffset {
        get => IsReleasing ? m_ReleaseOffset : m_Offset;
    }
}