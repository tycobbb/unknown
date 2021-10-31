using DG.Tweening;
using UnityEngine;
using UnityEngine.InputSystem;

/// the line pattern
public class PlayerMove: MonoBehaviour {
    // -- statics --
    /// the direction when pct is zero
    public static Vector2 DirZero = new Vector2(-1.0f, -1.0f);

    // -- tuning --
    [Header("tuning")]
    [Tooltip("the move speed in percent per second")]
    [SerializeField] float m_Speed = 0.5f;

    [Tooltip("the dash speed as a multiplier over time")]
    [SerializeField] AnimationCurve m_DashCurve;

    // -- nodes --
    [Header("nodes")]
    [Tooltip("the input system input")]
    [SerializeField] PlayerInput m_Input;

    // -- props --
    /// the current angle [0...1]
    float m_Percent;

    /// the destination angle
    float m_DstPercent;

    /// the current position
    Vector2 m_Pos;

    /// the move direction
    Buffer<Vector2> m_Dir;

    /// the dash speed multiplier
    float m_DashAccel;

    /// the pattern's corner index
    Draft<int> m_Corner = new Draft<int>(-1);

    /// the player's inputs
    InputAction m_Action;

    // -- lifecycle --
    void Awake() {
        // set props
        m_Action = m_Input.currentActionMap["Move"];
        m_Dir = new Buffer<Vector2>(1);
    }

    // -- commands --
    /// apply the configuration
    public void Init(PlayerConfig cfg) {
        Move(cfg.Percent);
    }

    /// read move input
    public void Read() {
        var next = m_Action.ReadValue<Vector2>();

        // process input
        ReadDash(next);
        ReadDest(next);

        // buffer dir
        m_Dir.Add(next);
    }

    /// read a dash input
    void ReadDash(Vector2 next) {
        // unless were stopped
        if (next == Vector2.zero) {
            return;
        }

        // if we were idle
        var isDash = !IsActive;

        // or if direction changes
        if (!isDash) {
            foreach (var prev in m_Dir) {
                var dot = Vector2.Dot(prev, next);
                if (dot < 0.0f) {
                    isDash = true;
                    break;
                }
            }
        }

        // start the dash
        // TODO: move this into a method called from Play
        if (isDash) {
            // get duration from curve
            var duration = m_DashCurve.keys[m_DashCurve.length - 1].time;

            // tween the dash acceleration
            var accel = new Lens<float>(
                ( ) => m_DashAccel,
                (v) => m_DashAccel = v
            );

            var _ = accel
                .TweenTo(1.0f, 0.0f, duration)
                .SetEase(m_DashCurve);
        }
    }

    /// read the destination input
    void ReadDest(Vector2 next) {
        // if inactive, stop moving
        if (next == Vector2.zero) {
            m_DstPercent = m_Percent;
        }
        // or, map the analog stick position to the pattern [0,1]
        else {
            // calculate destination angle
            var angle = Vector2.SignedAngle(PlayerMove.DirZero, next);
            if (angle < 0.0) {
                angle = Mathf.Abs(angle);
            } else {
                angle = 360.0f - angle;
            }

            m_DstPercent = angle / 360.0f;
        }
    }

    /// move into position
    public void Play() {
        // check the distance to our destination
        var pct0 = m_Percent;
        var pct1 = m_DstPercent;
        var dist = pct1 - pct0;

        // if there is anywhere to move
        if (dist == 0.0f || dist == 1.0f) {
            return;
        }

        // find the dir to destination
        var dDir = Mathf.Sign(dist);

        // but move in the shortest path
        var mDir = dDir;
        if (Mathf.Abs(dist) > 0.5f) {
            mDir = -mDir;
        }

        // adjust speed by dash
        var spd = m_Speed * (1.0f + m_DashAccel);

        // move by speed to get new pct
        var pcti = pct0 + spd * Time.deltaTime * mDir;

        // if we overshot, snap to target
        if (Mathf.Sign(pct1 - pcti) != dDir) {
            pcti = pct1;
        }

        // move to the percent
        Move(pcti);
    }

    /// move to the percent
    void Move(float pct) {
        // loop pct in [0,1]
        pct = Mathf.Repeat(pct, 1.0f);

        // move point
        Vector2 p;
        p.x = CalcDimension(pct + 1.375f);
        p.y = CalcDimension(pct + 1.625f);

        // update state
        m_Pos = p;
        m_Percent = pct;
        m_Corner.Val = Mathf.Min(Mathf.FloorToInt(pct / 0.25f), 3);
    }

    // -- c/helpers
    /// calc a dimension of the point
    float CalcDimension(float pct) {
        return Mathf.Clamp01(Mathf.Abs(4.0f * (Mathf.Repeat(pct, 2.0f) - 1.0f)) - 2.5f);
    }

    // -- queries --
    /// the current position
    public Vector2 Pos {
        get => m_Pos;
    }

    /// the current corner index
    public Draft<int> Corner {
        get => m_Corner;
    }

    /// if the move is active
    public bool IsActive {
        get => m_Dir.Val != Vector2.zero;
    }
}
