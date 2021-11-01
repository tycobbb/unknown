using System.Collections;
using DG.Tweening;
using UnityEngine;
using UnityEngine.InputSystem;

/// the line pattern
public class PlayerMove: MonoBehaviour {
    // -- statics --
    /// the direction when pct is zero
    static Vector2 s_DirZero = new Vector2(-1.0f, -1.0f);

    // -- tuning --
    [Header("tuning")]
    [Tooltip("the move speed in percent per second")]
    [SerializeField] float m_Speed = 0.5f;

    [Tooltip("the move position fn; [0,1] -> pos.x")]
    [SerializeField] AnimationCurve m_MovePos;

    [Tooltip("the dash speed as a multiplier over time")]
    [SerializeField] AnimationCurve m_DashAccel;

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
    float m_DashScale;

    /// the pattern's corner index
    int? m_Corner;

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
        // set initial pos
        SetPos(cfg.Percent);

        // set initial corner
        m_Corner = Mathf.FloorToInt(cfg.Percent * 4.0f);
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
            // tween the dash acceleration
            var accel = new Lens<float>(
                ( ) => m_DashScale,
                (v) => m_DashScale = v
            );

            var _ = accel
                .TweenTo(1.0f, 0.0f, m_DashAccel.Duration())
                .SetEase(m_DashAccel);
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
            var angle = Vector2.SignedAngle(s_DirZero, next);
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
        // check the distance to the destination
        var pct0 = m_Percent;
        var pctD = m_DstPercent;
        var dist = pctD - pct0;

        // if there is anywhere to move
        if (dist != 0.0f && dist != 1.0f) {
            Move();
        }

        // set the new corner
        SyncCorner(pct0);
    }

    /// move towards the destination percent
    void Move() {
        // check the distance to our destination
        var pct0 = m_Percent;
        var pctD = m_DstPercent;
        var dist = pctD - pct0;

        // find the dir to destination
        var dirD = Mathf.Sign(dist);

        // but move in the shortest path
        var dirM = dirD;
        if (Mathf.Abs(dist) > 0.5f) {
            dirM = -dirM;
        }

        // adjust speed by dash
        var spd = m_Speed * (1.0f + m_DashScale);

        // move by speed to get new pct
        var pct1 = pct0 + spd * Time.deltaTime * dirM;

        // if we overshot, snap to target
        if (Mathf.Sign(pctD - pct1) != dirD) {
            pct1 = pctD;
        }

        // set the new position
        SetPos(pct1);
    }

    /// set the current percent
    void SetPos(float pct) {
        // loop pct in [0,1]
        var pct1 = Mathf.Repeat(pct, 1.0f);

        // move point
        Vector2 p;
        p.x = m_MovePos.Evaluate(pct1);// Calc(pct1 + 1.375f);
        p.y = m_MovePos.Evaluate(pct1 + 0.25f);

        // update state
        m_Pos = p;
        m_Percent = pct1;
    }

    /// sync our current corner
    void SyncCorner(float pct0) {
        var pct1 = m_Percent;

        // default to no corner
        var ci = null as int?;

        // if we moved
        var dir = pct1 - pct0;
        if (dir != 0.0f && Mathf.Repeat(pct0, 0.25f) != 0.0f) {
            int c0;
            int c1;

            // find corner indices based on direction
            // TODO: there must be better math here right
            if (dir >= 0) {
                c0 = Mathf.FloorToInt(pct0 * 4.0f);
                c1 = Mathf.FloorToInt(pct1 * 4.0f);
            } else {
                c0 = Mathf.CeilToInt(pct0 * 4.0f);
                c1 = Mathf.CeilToInt(pct1 * 4.0f);
            }

            // and the corner changed
            if (c0 != c1) {
                ci = c1;
            }
        }

        // update state
        m_Corner = ci;
    }

    // -- queries --
    /// the current position
    public Vector2 Pos {
        get => m_Pos;
    }

    /// the current corner index
    public int? Corner {
        get => m_Corner;
    }

    /// if the move is active
    public bool IsActive {
        get => m_Dir.Val != Vector2.zero;
    }
}
