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

    [Tooltip("the move speed, degrees for a full rotation")]
    [SerializeField] float mMoveSpeed = 720.0f;

    [Tooltip("the duration to smooth movements")]
    [SerializeField] float mAnimDuration = 0.1f;

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

    /// the current move dir
    Vector2 mMoveDir;

    /// the accumulated move angle
    float mMoveAngle;

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
        mMoveAngle = mMoveSpeed * cfg.Percent;

        // show score
        mScore.AddPlayer(cfg);
    }

    /// read move pattern
    void ReadMove() {
        var nDir = mActions.Move;
        var pDir = mMoveDir;

        // update move dir
        mMoveDir = nDir;

        // add the angle between prev and next to the move angle
        if (pDir != Vector2.zero) {
            mMoveAngle = Mathf.Repeat(
                mMoveAngle + Vector2.SignedAngle(pDir, nDir),
                mMoveSpeed
            );
        }

        // apply angle to pattern (down is 0%/100%)
        mPattern.MoveTo(mMoveAngle / mMoveSpeed);

        // play footsteps when moving
        var isMoving = nDir != Vector2.zero;
        if (isMoving != mFootsteps.IsPlayingLoop) {
            mFootsteps.ToggleLoop(mFootstepsLoop, isMoving, mKey);
        }

        // play voice when anchor changes
        var i = mPattern.AnchorIndex;
        mAnchorIndex.Val = i;
        if (mAnchorIndex.IsDirty) {
            mVoice.PlayTone(mVoiceLine[i], mKey);
        }
    }
    /// move line into position
    void Move() {
        // try to release the flick
        mFlick.TryRelease();

        // get pattern pos
        var p0 = mPattern.Point0;
        var p1 = mPattern.Point1;
        var pe = p1 + mFlick.Offset;

        // move to the new positions
        MoveTo(p0, p1, pe, animated: !mIsFirstFrame);
        mIsFirstFrame = false;

        // raise move pitch based on offset
        mFootsteps.SetPitch(1.0f + mFlick.PitchShift);
    }

    /// move player to the position, animated if necessary
    void MoveTo(Vector2 p0, Vector2 p1, Vector2 pe, bool animated) {
        if (animated) {
            StartCoroutine(MoveToAnimated(p0, p1, pe));
        } else {
            MoveTo(p0, p1, pe);
        }
    }

    /// move player to the position
    void MoveTo(Vector2 p0, Vector2 p1, Vector2 pe) {
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

    /// animate player to the position
    IEnumerator MoveToAnimated(Vector2 p0, Vector2 p1, Vector2 pe) {
        // acc time each frame
        var time = 0.0f;
        var duration = mAnimDuration;

        // given the initial position
        var p10 = mTrail.Start;
        var pe0 = mLine.End;

        // until the animation finishes
        while (time < duration) {
            // lerp the moving points
            var pct = time / duration;
            var p11 = Vector2.Lerp(p10, p1, pct);
            var pe1 = Vector2.Lerp(pe0, pe, pct);

            // and update player's position
            MoveTo(p0, p11, pe1);

            // wait a frame
            time += Time.deltaTime;
            yield return null;
        }

        // end in the final position
        MoveTo(p0, p1, pe);
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