using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;

/// a player
public class Player: MonoBehaviour {
    // -- deps --
    /// the score module
    Score mScore;

    // -- tuning --
    [Header("tuning")]
    [Tooltip("the hitbox at the end of the line")]
    [SerializeField] PlayerHitbox mHitbox;

    [Tooltip("the move speed in percent per second")]
    [SerializeField] float mMoveSpeed = 0.5f;

    // -- nodes --
    [Header("nodes")]
    [Tooltip("the player's line")]
    [SerializeField] Shapes.Line mLine;

    [Tooltip("the player's hand")]
    [SerializeField] Transform mHand;

    [Tooltip("the player's ghost trail")]
    [SerializeField] Shapes.Line mTrail;

    [Tooltip("the player's ghost endpoint")]
    [SerializeField] Transform mGhost;

    [FormerlySerializedAs("mMusic")]
    [Tooltip("plays voice music")]
    [SerializeField] Musicker mVoice;

    [Tooltip("plays footstep music")]
    [SerializeField] Musicker mFootsteps;

    [Tooltip("the input system input")]
    [SerializeField] PlayerFlick mFlick;

    [Tooltip("the input system input")]
    [SerializeField] PlayerInput mInput;

    // -- props --
    /// the player's config
    PlayerConfig mConfig;

    /// if this is the first frame
    bool mIsFirstFrame = true;

    /// the musical key
    Key mKey;

    /// the line pattern
    Pattern mPattern;

    /// the destination move angle [0...1]
    float mPercentDest;

    /// the pattern's anchor index
    Draft<int> mAnchorIndex = new Draft<int>(-1);

    /// a line as the player's anchor changes
    Line mVoiceLine;

    /// a loop to when moving
    Loop mFootstepsLoop;

    /// a chord on hit
    Chord mHitChord;

    /// a chord on getting hit
    Chord mHurtChord;

    /// the player's inputs
    PlayerActions mActions;

    // -- lifecycle --
    void Awake() {
        // set deps
        mScore = Score.Get;

        // set props
        mPattern = new Pattern();
        mActions = new PlayerActions(mInput);

        // set music
        mVoiceLine = new Line(
            Tone.III,
            Tone.V,
            Tone.VII,
            Tone.I.Octave()
        );

        mFootstepsLoop = new Loop(
            fade: 1.5f,
            blend: 0.6f,
            Tone.I
        );

        mHitChord = new Chord(
            Tone.V,
            Quality.P5
        );

        mHurtChord = new Chord(
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
        mConfig = cfg;

        Debug.Log($"{name} joined");
    }

    /// apply player config
    void Configure() {
        var cfg = mConfig;

        // apply config
        mKey = new Key(cfg.Key);

        // grab shapes
        var hand = mHand.GetComponent<Shapes.Disc>();
        var ghost = mGhost.GetComponent<Shapes.Disc>();

        // decompose color
        var c = cfg.Color;
        Color.RGBToHSV(c, out var h, out var s, out var v);

        // set colors
        mLine.Color = c;
        mTrail.Color = FromHsv(h + (h > 0.5f ? -0.3f : 0.3f), s, v, a: 0.5f);
        ghost.Color = FromHsv(h, s - 0.3f, v + 0.5f, a: 0.5f);
        hand.Color = ghost.Color;

        // size hand
        hand.Radius = mHitbox.Radius;

        // set music props
        mVoice.Instrument = cfg.VoiceInstrument;
        mFootsteps.Instrument = cfg.FootstepsInstrument;

        // set initial position
        mPattern.MoveTo(cfg.Percent);

        // show score
        mScore.AddPlayer(cfg);
    }

    /// read move pattern
    void ReadMove() {
        var mDir = mActions.Move;

        // map the analog stick position to the pattern [0,1]
        if (mDir != Vector2.zero) {
            var angle = Vector2.SignedAngle(Vector2.down, mDir);
            if (angle < 0.0) {
                angle = Mathf.Abs(angle);
            } else {
                angle = 360.0f - angle;
            }

            mPercentDest = angle / 360.0f;
        }

        // play footsteps when moving
        var isMoving = mDir != Vector2.zero;
        if (isMoving != mFootsteps.IsPlayingLoop) {
            mFootsteps.ToggleLoop(mFootstepsLoop, isMoving, mKey);
        }
    }

    /// move line into position
    void Move() {
        // move the pattern
        MovePattern();

        // try to release the flick
        mFlick.TryRelease();

        // get pattern pos
        var p0 = mPattern.Point0;
        var p1 = mPattern.Point1;
        var pe = p1 + mFlick.Offset;

        // move to the new positions
        SyncPosition(p0, p1, pe);
        mIsFirstFrame = false;

        // raise move pitch based on offset
        mFootsteps.SetPitch(1.0f + mFlick.PitchShift);
    }

    void MovePattern() {
        // check the distance to our destination
        var pct0 = mPattern.Percent;
        var pct1 = mPercentDest;
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
        var pcti = pct0 + mMoveSpeed * Time.deltaTime * mDir;

        // if we overshot, snap to target
        if (Mathf.Sign(pct1 - pcti) != dDir) {
            pcti = pct1;
        }

        mPattern.MoveTo(pcti);

        // play voice when anchor changes
        var i = mPattern.AnchorIndex;
        mAnchorIndex.Val = i;
        if (mAnchorIndex.IsDirty) {
            mVoice.PlayTone(mVoiceLine[i], mKey);
        }
    }

    /// sync player position
    void SyncPosition(Vector2 p0, Vector2 p1, Vector2 pe) {
        // move hitbox
        mHitbox.Position = pe;

        // move actual line
        mLine.Start = p0;
        mLine.End = pe;
        mHand.localPosition = pe;

        // move ghost trail
        mTrail.Start = p1;
        mTrail.End = pe;
        mGhost.localPosition = p1;
    }

    // -- queries --
    /// check if the player's overlap
    public bool Overlaps(Player other) {
        return mHitbox.Overlaps(other.mHitbox);
    }

    /// if this player is releasing
    bool IsReleasing {
        get => mFlick.IsReleasing;
    }

    // -- events --
    public void OnCollision(Player other) {
        // play the chord
        if (IsReleasing) {
            mVoice.PlayChord(mHitChord, mKey);
        } else if (other.IsReleasing) {
            mVoice.PlayChord(mHurtChord, mKey);
        }

        // record the hit
        if (IsReleasing) {
            mScore.RecordHit(mConfig, mFlick.Speed);
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