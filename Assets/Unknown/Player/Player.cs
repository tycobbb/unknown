using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;

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

    // -- nodes --
    [Header("nodes")]
    [Tooltip("the player's line")]
    [SerializeField] Shapes.Line m_Line;

    [Tooltip("the player's hand")]
    [SerializeField] Transform m_Hand;

    [Tooltip("the player's ghost trail")]
    [SerializeField] Shapes.Line m_Trail;

    [Tooltip("the player's ghost endpoint")]
    [SerializeField] Transform m_Ghost;

    [Tooltip("plays voice music")]
    [SerializeField] Musicker m_Voice;

    [Tooltip("plays footstep music")]
    [SerializeField] Musicker m_Footsteps;

    [Tooltip("the input system input")]
    [SerializeField] PlayerFlick m_Flick;

    [Tooltip("the input system input")]
    [SerializeField] PlayerInput m_Input;

    // -- props --
    /// the player's config
    PlayerConfig m_Config;

    /// if this is the first frame
    bool m_IsFirstFrame = true;

    /// the musical key
    Key m_Key;

    /// the line pattern
    Pattern m_Pattern;

    /// the destination move angle [0...1]
    float m_PercentDest;

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

    void Update() {
        // read input
        ReadMove();
    }

    void FixedUpdate() {
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

        // grab shapes
        var hand = m_Hand.GetComponent<Shapes.Disc>();
        var ghost = m_Ghost.GetComponent<Shapes.Disc>();

        // decompose color
        var c = cfg.Color;
        Color.RGBToHSV(c, out var h, out var s, out var v);

        // set colors
        m_Line.Color = c;
        m_Trail.Color = FromHsv(h + (h > 0.5f ? -0.3f : 0.3f), s, v, a: 0.5f);
        ghost.Color = FromHsv(h, s - 0.3f, v + 0.5f, a: 0.5f);
        hand.Color = ghost.Color;

        // size hand
        hand.Radius = m_Hitbox.Radius;

        // set music props
        m_Voice.Instrument = cfg.VoiceInstrument;
        m_Footsteps.Instrument = cfg.FootstepsInstrument;

        // set initial position
        m_Pattern.MoveTo(cfg.Percent);

        // show score
        m_Score.AddPlayer(cfg);
    }

    /// read move pattern
    void ReadMove() {
        var mDir = m_Actions.Move;

        // map the analog stick position to the pattern [0,1]
        if (mDir != Vector2.zero) {
            var angle = Vector2.SignedAngle(Vector2.down, mDir);
            if (angle < 0.0) {
                angle = Mathf.Abs(angle);
            } else {
                angle = 360.0f - angle;
            }

            m_PercentDest = angle / 360.0f;
        }

        // play footsteps when moving
        var isMoving = mDir != Vector2.zero;
        if (isMoving != m_Footsteps.IsPlayingLoop) {
            m_Footsteps.ToggleLoop(m_FootstepsLoop, isMoving, m_Key);
        }
    }

    /// move line into position
    void Move() {
        // move the pattern
        MovePattern();

        // try to release the flick
        m_Flick.TryRelease();

        // get pattern pos
        var p0 = m_Pattern.Point0;
        var p1 = m_Pattern.Point1;
        var pe = p1 + m_Flick.Offset;

        // move to the new positions
        SyncPosition(p0, p1, pe);
        m_IsFirstFrame = false;

        // raise move pitch based on offset
        m_Footsteps.SetPitch(1.0f + m_Flick.PitchShift);
    }

    void MovePattern() {
        // check the distance to our destination
        var pct0 = m_Pattern.Percent;
        var pct1 = m_PercentDest;
        var dist = pct1 - pct0;

        // if there is anywhere to move
        if (dist == 0.0f) {
            return;
        }

        // find the dir to destination
        var dDir = Mathf.Sign(dist);

        // but move in the shortest path
        var mDir = dDir;
        if (Mathf.Abs(dist) > 0.5f) {
            mDir = -mDir;
        }

        // lerp the angle into the pattern
        var pcti = pct0 + m_MoveSpeed * Time.deltaTime * mDir;

        // if we overshot, snap to target
        if (Mathf.Sign(pct1 - pcti) != dDir) {
            pcti = pct1;
        }

        m_Pattern.MoveTo(pcti);

        // play voice when anchor changes
        var i = m_Pattern.AnchorIndex;
        m_AnchorIndex.Val = i;
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
        m_Hand.localPosition = pe;

        // move ghost trail
        m_Trail.Start = p1;
        m_Trail.End = pe;
        m_Ghost.localPosition = p1;
    }

    // -- queries --
    /// check if the player's overlap
    public bool Overlaps(Player other) {
        return m_Hitbox.Overlaps(other.m_Hitbox);
    }

    /// if this player is releasing
    bool IsReleasing {
        get => m_Flick.IsReleasing;
    }

    // -- events --
    public void OnCollision(Player other) {
        // play the chord
        if (IsReleasing) {
            m_Voice.PlayChord(m_HitChord, m_Key);
        } else if (other.IsReleasing) {
            m_Voice.PlayChord(m_HurtChord, m_Key);
        }

        // record the hit
        if (IsReleasing) {
            m_Score.RecordHit(m_Config, m_Flick.Speed);
        }
    }

    // -- utils --
    /// get rgb color from hsv
    Color FromHsv(float h, float s, float v, float a = 1.0f) {
        var c = Color.HSVToRGB(Mathf.Repeat(h, 1.0f), s, v);
        c.a = a;
        return c;
    }
}