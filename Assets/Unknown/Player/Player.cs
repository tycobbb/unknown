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
    [UnityEngine.Serialization.FormerlySerializedAs("m_Hitbox")]
    [Tooltip("the hitbox at the player's hand")]
    [SerializeField] HitBox m_HandHitBox;

    [Tooltip("the hitbox at the player's foot")]
    [SerializeField] HitBox m_FootHitBox;

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

    [Tooltip("the player's foot")]
    [SerializeField] Shapes.Disc m_Foot;

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

    /// the current length
    float m_Length = 0.5f;

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

        // m_Line.Thickness = Mathf.LerpUnclamped(2.0f, 0.5f, (m_Line.End - m_Line.Start).magnitude);
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
        m_Foot.Color = bg;
        m_Trail.Color = accent;
        m_Ghost.Color = bg;

        // size hand and foot
        m_Hand.Radius = m_HandHitBox.Radius;
        m_Foot.Radius = m_FootHitBox.Radius;

        // set music props
        m_Voice.Instrument = cfg.VoiceInstrument;
        m_Steps.Instrument = cfg.FootstepsInstrument;

        // set initial position
        m_Move.Init(cfg);
        m_Flick.Init(cfg, m_Move.Pos, m_Length);

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
        m_Flick.Play(m_Move.Pos);
        Constrain();

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

    /// constrain the length of the wand
    void Constrain() {
        var p0 = m_Move.Pos;
        var p1 = m_Flick.Pos;

        // get hand to foot dist
        var dist = p1 - p0;

        // given the max and actual length
        var max = m_Length;
        var len = Vec2.Magnitude(dist);

        // pull the hand into place
        if (max < len) {
            m_Flick.Pos = p0 + dist.normalized * max;
        }
    }

    /// sync length w/ present position
    void SyncLength() {
        m_Length = Vec2.Magnitude(m_Flick.Pos - m_Move.Pos);
    }

    /// sync player position
    void SyncPos() {
        // get state
        var p0 = m_Move.Pos;
        var p1 = m_Flick.Pos;
        var pe = m_Flick.OffsetPos;

        // move hitboxes
        m_HandHitBox.Pos = pe;
        m_FootHitBox.Pos = p0;

        // move actual line
        m_Line.Start = p0;
        m_Line.End = pe;
        m_Hand.transform.localPosition = pe;
        m_Foot.transform.localPosition = p0;

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

        // cancel any active release
        m_Flick.Cancel();

        // play hitstop effect
        var scale = Mathf.Max(m_Flick.Speed, other.m_Flick.Speed);
        PlayHitStop(scale);

        // play hit effects
        if (isAttacker) {
            Hit.Play(m_Config, m_HandHitBox);
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
    /// the player's index
    public int Index {
        get => m_Config.Index;
    }

    /// the player's hand position
    public Vector2 Pos {
        get => m_Flick.Pos;
    }

    /// if this player is releasing
    bool IsAttacking {
        get => m_Flick.IsReleasing;
    }

    // -- collision --
    /// if the players overlap
    public bool Collide(Player other) {
        return (
            m_HandHitBox.Overlaps(other.m_HandHitBox) ||
            m_HandHitBox.Overlaps(other.m_FootHitBox)
        );
    }

    /// if the player overlaps the wall
    public Contact? Collide(Wall wall) {
        return wall.Collide(m_Flick.OffsetPos);
    }

    // -- events --
    /// when two players collide
    public void OnPlayerContact(Player other) {
        HitPlayer(other);
    }

    /// when a player collides w/ something else
    public void OnWallContact(Contact contact) {
        m_Flick.Bounce(contact);
    }

    /// when a flick release finishes
    public void OnReleaseEnd() {
        SyncLength();
    }
}