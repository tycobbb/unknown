using System.Collections;
using DG.Tweening;
using UnityEngine;
using UnityEngine.InputSystem;

/// a player
public class Player: MonoBehaviour {
    // -- deps --
    /// the score module
    Score m_Score;

    // -- tuning --
    [Header("tuning")]
    [Tooltip("the hitbox at the end of the line")]
    [SerializeField] PlayerHitbox m_Hitbox;

    [Tooltip("the move speed in percent per second")]
    [SerializeField] float m_MoveSpeed = 0.5f;

    [Tooltip("the dash speed as a multiplier over time")]
    [SerializeField] AnimationCurve m_DashCurve;

    // -- nodes --
    [Header("nodes")]
    [Tooltip("the player's line")]
    [SerializeField] Shapes.Line m_Line;

    [Tooltip("the player's hand")]
    [SerializeField] Shapes.Disc m_Hand;

    [Tooltip("the player's ghost trail")]
    [SerializeField] Shapes.Line m_Trail;

    [Tooltip("the player's ghost endpoint")]
    [SerializeField] Shapes.Disc m_Ghost;

    [Tooltip("plays voice music")]
    [SerializeField] Musicker m_Voice;

    [Tooltip("plays footstep music")]
    [SerializeField] Musicker m_Steps;

    [Tooltip("the flick action")]
    [SerializeField] PlayerFlick m_Flick;

    [Tooltip("the hitstop")]
    [SerializeField] PlayerHitStop m_HitStop;

    [Tooltip("the input system input")]
    [SerializeField] PlayerInput m_Input;

    // -- props --
    /// the player's config
    PlayerConfig m_Config;

    /// the musical key
    Key m_Key;

    /// the line pattern
    Pattern m_Pattern;

    /// the destination move angle [0...1]
    float m_PercentDest;

    /// the move direction
    Buffer<Vector2> m_MoveDir;

    /// the dash speed multiplier
    float m_DashAccel;

    /// the pattern's anchor index
    Draft<int> m_AnchorIndex = new Draft<int>(-1);

    /// a line as the player's anchor changes
    Line m_VoiceLine;

    /// a loop to when moving
    Loop m_FootstepsLoop;

    /// a chord on hit
    Chord m_HitChord;

    /// a chord on getting hit
    Chord m_HurtChord;

    /// the player's inputs
    PlayerActions m_Actions;

    // -- lifecycle --
    void Awake() {
        // set deps
        m_Score = Score.Get;

        // set props
        m_Pattern = new Pattern();
        m_Actions = new PlayerActions(m_Input);
        m_MoveDir = new Buffer<Vector2>(1);

        // set music
        m_VoiceLine = new Line(
            Tone.III,
            Tone.V,
            Tone.VII,
            Tone.I.Octave()
        );

        m_FootstepsLoop = new Loop(
            fade: 1.5f,
            blend: 0.6f,
            Tone.I
        );

        m_HitChord = new Chord(
            Tone.V,
            Quality.P5
        );

        m_HurtChord = new Chord(
            Tone.II,
            Quality.Min3
        );

        // apply config
        Configure();
    }

    void FixedUpdate() {
        // read input
        ReadMove();

        // move line
        Move();
    }

    // -- commands --
    /// init this player on join
    public void Join(PlayerConfig cfg) {
        name = $"Player{cfg.Index}";
        m_Config = cfg;

        Debug.Log($"{name} joined");
    }

    /// apply player config
    void Configure() {
        var cfg = m_Config;

        // apply config
        m_Key = new Key(cfg.Key);

        // decompose color
        var rgb = cfg.Color;
        var hsv = rgb.ToHsv();

        // build palette
        var fg = rgb;
        var accent = hsv.Add(h: hsv.H > 0.5f ? -0.3f : 0.3f).ToRgb(a: 0.5f);
        var bg = hsv.Add(s: -0.3f, v: 0.5f).ToRgb(a: 0.5f);

        // set colors
        m_Line.Color = fg;
        m_Hand.Color = bg;
        m_Trail.Color = accent;
        m_Ghost.Color = bg;

        // size hand
        m_Hand.Radius = m_Hitbox.Radius;

        // set music props
        m_Voice.Instrument = cfg.VoiceInstrument;
        m_Steps.Instrument = cfg.FootstepsInstrument;

        // set initial position
        m_Pattern.MoveTo(cfg.Percent);

        // show score
        m_Score.AddPlayer(cfg);
    }

    /// read move input
    void ReadMove() {
        var next = m_Actions.Move;

        // process input
        ReadDash(next);
        ReadDest(next);

        // buffer dir
        m_MoveDir.Add(next);
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
            foreach (var prev in m_MoveDir) {
                var dot = Vector2.Dot(prev, next);
                if (dot < 0.0f) {
                    isDash = true;
                    break;
                }
            }
        }

        // start the dash
        if (isDash) {
            m_DashAccel = 1.0f;

            DOTween
                .To(
                    ( ) => m_DashAccel,
                    (v) => m_DashAccel = v,
                    0.0f,
                    DashDuration
                )
                .SetEase(m_DashCurve);
        }
    }

    /// read the destination input
    void ReadDest(Vector2 next) {
        // if inactive, stop moving
        if (next == Vector2.zero) {
            m_PercentDest = m_Pattern.Percent;
        }
        // or, map the analog stick position to the pattern [0,1]
        else {
            // calculate destination angle
            var angle = Vector2.SignedAngle(Vector2.down, next);
            if (angle < 0.0) {
                angle = Mathf.Abs(angle);
            } else {
                angle = 360.0f - angle;
            }

            m_PercentDest = angle / 360.0f;
        }
    }

    /// move line into position
    void Move() {
        // if not in hitstop
        if (m_HitStop.IsActive) {
            return;
        }

        // move the pattern
        MovePattern();

        // get pattern pos
        var p0 = m_Pattern.Point0;
        var p1 = m_Pattern.Point1;
        var pe = p1 + m_Flick.Offset;

        // move to the new positions
        SyncPosition(p0, p1, pe);

        // raise move pitch based on offset
        m_Steps.SetPitch(1.0f + m_Flick.PitchShift);

        // play footsteps when moving
        if (IsActive != m_Steps.IsPlayingLoop) {
            m_Steps.ToggleLoop(m_FootstepsLoop, IsActive, m_Key);
        }
    }

    /// move the pattern to new percent
    void MovePattern() {
        // check the distance to our destination
        var pct0 = m_Pattern.Percent;
        var pct1 = m_PercentDest;
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
        var spd = m_MoveSpeed * (1.0f + m_DashAccel);

        // move by speed to get new pct
        var pcti = pct0 + spd * Time.deltaTime * mDir;

        // if we overshot, snap to target
        if (Mathf.Sign(pct1 - pcti) != dDir) {
            pcti = pct1;
        }

        m_Pattern.MoveTo(pcti);

        // play voice when anchor changes
        var i = m_AnchorIndex.Val = m_Pattern.AnchorIndex;
        if (m_AnchorIndex.IsDirty) {
            m_Voice.PlayTone(m_VoiceLine[i], m_Key);
        }
    }

    /// sync player position
    void SyncPosition(Vector2 p0, Vector2 p1, Vector2 pe) {
        // move hitbox
        m_Hitbox.Position = pe;

        // move actual line
        m_Line.Start = p0;
        m_Line.End = pe;
        m_Hand.transform.localPosition = pe;

        // move ghost trail
        m_Trail.Start = p1;
        m_Trail.End = pe;
        m_Ghost.transform.localPosition = p1;
    }

    /// trigger a collision
    void HitPlayer(Player other) {
        // if at least one player is attacking
        var isAttacker = IsReleasing;
        var isAttacked = other.IsReleasing;
        if (!isAttacker && !isAttacked) {
            return;
        }

        // cancel any active release
        m_Flick.Cancel();

        // play hitstop effect
        var mag = m_Flick.Speed;
        m_HitStop.Play(mag);

        // play hit effect
        if (isAttacker) {
            Hit.Play(
                m_Config,
                m_Hand.transform.position,
                m_Hitbox.Radius
            );
        }

        // play the chord
        if (isAttacker) {
            m_Voice.PlayChord(m_HitChord, m_Key);
        } else {
            m_Voice.PlayChord(m_HurtChord, m_Key);
        }

        // record the hit
        if (isAttacker) {
            m_Score.RecordHit(m_Config, mag);
        }
    }

    // -- queries --
    /// check if the player's overlap
    public bool Overlaps(Player other) {
        return m_Hitbox.Overlaps(other.m_Hitbox);
    }

    /// if the player is active
    bool IsActive {
        get => m_MoveDir.Val != Vector2.zero;
    }

    /// if this player is releasing
    bool IsReleasing {
        get => m_Flick.IsReleasing;
    }

    /// if this player is releasing
    float DashDuration {
        get => m_DashCurve.keys[m_DashCurve.length - 1].time;
    }

    // -- events --
    /// when two players collide
    public void OnCollision(Player other) {
        HitPlayer(other);
    }
}