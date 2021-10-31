using UnityEngine;
using UnityEngine.InputSystem;

/// the player's flick gesture
public class PlayerFlick: MonoBehaviour {
    // -- constants --
    /// when there is no release
    const int c_ReleaseNone = -1;

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
    [Tooltip("the input system input")]
    [SerializeField] PlayerInput m_Input;

    // -- props --
    /// the current position
    Vector2 m_Pos;

    /// the current offset
    Vector2 m_Offset;

    /// the target offset
    Vector2 m_DstOffset;

    /// the input action
    InputAction m_Action;

    // -- p/release
    /// the current frame
    int m_ReleaseFrame;

    /// the time on release
    float m_ReleaseTime;

    /// the initial release offset
    Vector2 m_ReleaseSrc;

    /// the strength of the release
    float m_ReleaseStrength;

    /// the current release speed
    float m_ReleaseSpeed;

    // -- lifecycle --
    void Awake() {
        // set props
        m_Action = m_Input.currentActionMap["Flick"];
    }

    // -- commands --
    public void Init(Vector2 pos) {
        // set initial position
        m_Pos = pos;
    }

    /// read flick
    public void Read() {
        // capture prev and next offset
        var prev = m_DstOffset;
        var next = m_Action.ReadValue<Vector2>();

        // update state
        m_DstOffset = next;

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
            m_ReleaseSrc = prev;
            m_ReleaseStrength = 0.0f;
        }

        // buffer strength
        m_ReleaseStrength = Mathf.Max(m_ReleaseStrength, delta.magnitude);
    }

    /// move the flick into its new position
    public void Play() {
        if (IsReleasing) {
            Release();
        } else {
            Move();
        }
    }

    /// move the flick towards its target position
    void Move() {
        // if there is any move to make
        if (m_Offset == m_DstOffset) {
            return;
        }

        // given direction to target
        var curr = m_Offset;
        var dest = m_DstOffset;
        var cDir = Vec2.Normalize(dest - curr);

        // move offset towards it
        var next = curr + cDir * m_Speed * Time.deltaTime;

        // snap if we overshot
        var nDir = dest - next;
        if (Vector2.Dot(cDir, nDir) < 0.0f) {
            next = dest;
        }

        // update state
        m_Offset = next;
    }

    /// release the flick, if necessary, modifying the offset
    void Release() {
        // if not releasing
        if (!IsReleasing) {
            return;
        }

        // check elapsed time
        var pct = (Time.time - m_ReleaseTime) / m_ReleaseDuration;

        // if the release finished, clear it
        if (pct >= 1.0f) {
            Finish();
            return;
        }

        // otherwise, figure out where we are on the release curve
        pct = m_ReleaseCurve.Evaluate(pct);
        if (pct > 1.0f) {
            pct = 1.0f + (pct - 1.0f) * m_ReleaseStrength;
        }

        // calculate the next offset
        var curr = m_Offset;
        var next = Vector2.LerpUnclamped(m_ReleaseSrc, Vector2.zero, pct);

        // track the offset and speed
        m_Offset = next;
        m_ReleaseSpeed = Vector2.Distance(curr, next) / Time.deltaTime;
    }

    /// finish the active release, if any
    public void Finish() {
        m_Pos = Offset;
        m_Offset = Vector2.zero;
        m_ReleaseFrame = c_ReleaseNone;
    }

    // -- queries --
    /// find current position
    public Vector2 Pos {
        get => m_Pos;
    }

    /// the offset position
    public Vector2 Offset {
        get => m_Pos + m_Offset * m_Scale;
    }

    /// the current pitch shift
    public float PitchShift {
        get => m_Offset.magnitude * m_PitchScale;
    }

    /// the current release speed
    public float Speed {
        get => m_ReleaseSpeed;
    }

    /// if the release gesture is active
    public bool IsReleasing {
        get => m_ReleaseFrame != c_ReleaseNone;
    }

    /// if the release gesture is frame locked
    bool IsReleaseLocked {
        get => m_ReleaseFrame > 1;
    }
}