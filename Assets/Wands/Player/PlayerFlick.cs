using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

namespace Wands {

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

    [Tooltip("the min alignment to detect a release")]
    [SerializeField] float m_ReleaseAlignment = 0.1f;

    [Tooltip("the spring on the release")]
    [SerializeField] float m_ReleaseSpring = 0.1f;

    [Tooltip("the drag on the release")]
    [SerializeField] float m_ReleaseDrag = 0.95f;

    [Tooltip("the square speed to end the release")]
    [SerializeField] float m_ReleaseEndSpeed = 0.05f;

    // -- nodes --
    [Header("nodes")]
    [Tooltip("the input system input")]
    [SerializeField] PlayerInput m_Input;

    // -- events --
    [Header("events")]
    [Tooltip("the release event")]
    [SerializeField] UnityEvent m_OnReleaseEnd;

    // -- props --
    /// the current position
    Vector2 m_Pos;

    /// the current offset
    Vector2 m_Offset;

    /// the destination offset
    Vector2 m_DstOffset;

    /// the input action
    InputAction m_Action;

    // -- p/release
    /// the current frame for the release
    int m_Frame = c_ReleaseNone;

    /// the release velocity
    Vector2 m_Velocity;

    /// the target offset of the release
    Vector2 m_Target;

    // -- lifecycle --
    void Awake() {
        // set props
        m_Action = m_Input.currentActionMap["Flick"];
    }

    void OnDrawGizmos() {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(m_Pos + m_Target, 0.05f);
    }

    // -- commands --
    public void Init(PlayerConfig cfg, Vector2 p0, float len) {
        // set initial position
        m_Pos = p0 + cfg.Direction * len;
    }

    /// read flick
    public void Read() {
        // capture prev and next offset
        var curr = m_DstOffset;
        var next = m_Action.ReadValue<Vector2>();

        if (!IsReleasing) {
            ReadPull(next);
        }

        if (!IsReleaseLocked) {
            ReadRelease(curr, next);
        }
    }

    /// read pull from offset
    void ReadPull(Vector2 next) {
        m_DstOffset = next;
    }

    /// read release from curr and next offset
    void ReadRelease(Vector2 curr, Vector2 next) {
        m_DstOffset = next;

        // check alignment between previous dir and movement dir
        var delta = next - curr;
        var align = Vector2.Dot(curr.normalized, delta.normalized);

        // if direction changed, this is a release
        if (Mathf.Abs(align + 1.0f) > m_ReleaseAlignment) {
            return;
        }

        // init the release if this is the first frame
        if (!IsReleasing) {
            m_Frame = 0;
            m_Velocity = Vector2.zero;
        }

        // buffer target
        if (delta.magnitude > m_Velocity.magnitude) {
            m_Target = m_DstOffset + delta;
        }
    }

    /// move the flick into its new position
    public void Play(Vector2 p0) {
        // move the flick
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

        var v = m_Velocity;

        // apply forces to velocity
        v += m_ReleaseSpring * (m_Target - m_Offset);
        v *= m_ReleaseDrag;

        // finish release if under the threshold
        if (v.sqrMagnitude <= m_ReleaseEndSpeed) {
            Finish();
            return;
        }

        // update state
        m_Offset += v * Time.deltaTime;
        m_Velocity = v;
    }

    /// bounce off a surface
    public void Bounce(Contact contact) {
        if (!IsReleasing) {
            return;
        }

        // reflect velocity off surface
        var v0 = m_Velocity;
        var v1 = Vector2.Reflect(v0, contact.Normal);

        // project the remaining dist to the target onto the new direction
        var pc = contact.Pos;
        var p0 = m_Pos + m_Target;
        var p1 = pc + v1.normalized * Vector2.Distance(pc, p0) - m_Pos;

        // update state
        m_Target = p1;
        m_Velocity = v1;
    }

    /// cancel any active release
    public void Cancel() {
        Finish();
    }

    /// finish any active release
    void Finish() {
        m_Pos = OffsetPos;
        m_Offset = Vector2.zero;
        m_Velocity = Vector2.zero;
        m_Frame = c_ReleaseNone;
        m_OnReleaseEnd.Invoke();
    }

    // -- props/hot --
    /// the current position
    public Vector2 Pos {
        get => m_Pos;
        set => m_Pos = value;
    }

    // -- queries --
    /// the offset position
    public Vector2 OffsetPos {
        get => m_Pos + Offset;
    }

    /// the current pitch shift
    public float PitchShift {
        get => m_Offset.magnitude * m_PitchScale;
    }

    /// the current release speed
    public float Speed {
        get => m_Velocity.magnitude;
    }

    /// if the flick is active
    public bool IsActive {
        get => m_Offset != Vector2.zero;
    }

    /// if the release gesture is active
    public bool IsReleasing {
        get => m_Frame != c_ReleaseNone;
    }

    /// the scaled offset
    Vector2 Offset {
        get => m_Offset * m_Scale;
    }

    /// if the release gesture is frame locked
    bool IsReleaseLocked {
        get => m_Frame > 1;
    }
}

}