using UnityEngine;
using UnityEngine.InputSystem;

/// a player
public class Player: MonoBehaviour {
    // -- tuning --
    [Header("tuning")]
    [Tooltip("the maximum length of a flick")]
    [SerializeField] float mFlickLength = 0.1f;

    // -- nodes --
    [Header("nodes")]
    [Tooltip("the player's shape")]
    [SerializeField] Shapes.ShapeGroup mShape;

    [Tooltip("the player's line")]
    [SerializeField] Shapes.Line mLine;

    [Tooltip("the player's endpoint")]
    [SerializeField] Shapes.Disc mEnd;

    [Tooltip("the player's offset line")]
    [SerializeField] Shapes.Line mOffset;

    [Tooltip("plays music on contact")]
    [SerializeField] Musicker mMusic;

    [Tooltip("the input system input")]
    [SerializeField] PlayerInput mInput;

    // -- props --
    /// the musical key
    Key mKey;

    /// the line pattern
    Pattern mPattern;

    /// the flick offset
    Vector2 mFlickOffset;

    /// the flick speed
    Vector2 mFlickSpeed;

    /// the player's inputs
    PlayerActions mActions;

    // -- lifecycle --
    void Awake() {
        // set props
        mPattern = new Pattern();
        mActions = new PlayerActions(mInput);
    }

    void Update() {
        // update state from input
        ReadMove();
        ReadFlick();

        // move line
        Move();
    }

    // -- commands --
    /// init this player on join
    public void Join(PlayerConfig cfg) {
        Debug.Log($"{cfg.Name} joined");

        // set props
        name = cfg.Name;
        mKey = new Key(cfg.Key);

        // set line props
        mShape.Color = cfg.Color;

        // set music props
        mMusic.Instrument = cfg.Instrument;
    }

    /// read move pattern
    void ReadMove() {
        mPattern.SetPercent(ReadPercent(mActions.Move));
    }

    /// read flick offset and velocity
    void ReadFlick() {
        // capture prev and next offset
        var prev = mFlickOffset;
        var next = mActions.Flick;

        // update state
        mFlickOffset = next;
    }

    /// move line into position
    void Move() {
        // the pattern pos
        var p0 = mPattern.Point0;
        var p1 = mPattern.Point1;

        // the flick-adjusted endpoint
        var pe = p1 + mFlickOffset * mFlickLength;

        // render shapes
        mLine.Start = p0;
        mLine.End = pe;

        mOffset.Start = p1;
        mOffset.End = pe;

        mEnd.transform.localPosition = p1;
    }

    // -- queries --
    /// read percent complete from stick dir (oriented down, clockwise)
    float ReadPercent(Vector2 dir) {
        var a = -Vector2.SignedAngle(Vector2.down, dir);
        a = Mathf.Repeat(a + 360.0f, 360.0f);
        return a / 360.0f;
    }
}