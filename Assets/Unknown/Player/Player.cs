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

    [Tooltip("the scale of the hand on hit")]
    [SerializeField] Linear<float> m_HitHandScale;

    [Tooltip("the hitstop duration curve")]
    [SerializeField] AnimationCurve m_HitStopDuration;

    // -- parts --
    [Header("parts")]
    [Tooltip("the move action")]
    [SerializeField] PlayerMove m_Move;

    [Tooltip("the flick action")]
    [SerializeField] PlayerFlick m_Flick;

    // -- nodes --
    [Header("nodes")]
    [Tooltip("the player's linGe")]
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

    [Tooltip("the input system input")]
    [SerializeField] PlayerInput m_Input;

    // -- props --
    /// the player's config
    PlayerConfig m_Config;

    /// the musical key
    Key m_Key;

    /// if hitstop is active
    bool m_IsHitStop;

    /// the hand tween on hit
    Tweener m_HitHandTween;

    /// a line as the player's anchor changes
    Line m_VoiceLine;

    /// a loop to when moving
    Loop m_FootstepsLoop;

    /// a chord on hit
    Chord m_HitChord;

    /// a chord on getting hit
    Chord m_HurtChord;

    // -- lifecycle --
    void Awake() {
        // set deps
        m_Score = Score.Get;

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
        Init();
    }

    void FixedUpdate() {
        // read input
        Read();

        // apply movement
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
    void Init() {
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
        m_Move.Init(cfg);
        m_Flick.Init(cfg, m_Move.Pos);

        // show score
        m_Score.AddPlayer(cfg);
    }

    /// read input
    void Read() {
        m_Move.Read();
        m_Flick.Read();
    }

    /// move line into position
    void Move() {
        // if not in hitstop
        if (m_IsHitStop) {
            return;
        }

        // play actions
        m_Move.Play();
        m_Flick.Play();

        // move to new positions
        SyncPos();

        // raise move pitch based on offset
        m_Steps.SetPitch(1.0f + m_Flick.PitchShift);

        // play footsteps when moving
        var isActive = m_Move.IsActive || m_Flick.IsActive;
        if (isActive != m_Steps.IsPlayingLoop) {
            m_Steps.ToggleLoop(m_FootstepsLoop, isActive, m_Key);
        }

        // play voice when corner changes
        var corner = m_Move.Corner;
        if (corner != null) {
            m_Voice.PlayTone(m_VoiceLine[corner.Value], m_Key);
        }
    }

    /// sync player position
    void SyncPos() {
        // get state
        var p0 = m_Move.Pos;
        var p1 = m_Flick.Pos;
        var pe = m_Flick.Offset;

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
        var isAttacker = IsAttacking;
        var isAttacked = other.IsAttacking;
        if (!isAttacker && !isAttacked) {
            return;
        }

        // finish any active release
        m_Flick.Finish();

        // play hitstop effect
        var scale = Mathf.Max(m_Flick.Speed, other.m_Flick.Speed);
        PlayHitStop(scale);

        // play hit effects
        if (isAttacker) {
            Hit.Play(
                m_Config,
                m_Hand.transform.position,
                m_Hitbox.Radius
            );

            PlayHitTweens();
        }

        // play the chord
        if (isAttacker) {
            m_Voice.PlayChord(m_HitChord, m_Key);
        } else {
            m_Voice.PlayChord(m_HurtChord, m_Key);
        }

        // record the hit
        if (isAttacker) {
            m_Score.RecordHit(m_Config, m_Flick.Speed);
        }
    }

    /// play hitstop effect
    void PlayHitStop(float mag) {
        IEnumerator PlayAsync(float mag) {
            var duration = m_HitStopDuration.Evaluate(mag);
            m_IsHitStop = true;
            yield return new WaitForSeconds(duration);
            m_IsHitStop = false;
        }

        StartCoroutine(PlayAsync(mag));
    }

    /// play a new hand scale tween to play on hit
    void PlayHitTweens() {
        // get start and end radius
        var r0 = m_Hand.Radius;
        var r1 = m_HitHandScale.Mul(r0, 0.5f);

        // get lens
        var radius = new Lens<float>(
            ( ) => m_Hand.Radius,
            (v) => m_Hand.Radius = v
        );

        // create tween
        radius
            .TweenTo(r0, r1)
            .SetLoops(2, LoopType.Yoyo)
            .SetEase(Ease.InOutCubic);
    }

    // -- queries --
    /// if this player is releasing
    bool IsAttacking {
        get => m_Flick.IsReleasing;
    }

    // -- collision --
    /// check if the player's overlap
    public bool Overlaps(Player other) {
        return m_Hitbox.Overlaps(other.m_Hitbox);
    }

    /// when two players collide
    public void OnCollision(Player other) {
        HitPlayer(other);
    }
}