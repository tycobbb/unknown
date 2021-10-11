using UnityEngine;
using UnityEngine.InputSystem;

/// a player
public class Player: MonoBehaviour {
    // -- nodes --
    [Header("nodes")]
    [Tooltip("the player's line")]
    [SerializeField] Shapes.Line mLine;

    [Tooltip("plays music on contact")]
    [SerializeField] Musicker mMusic;

    [Tooltip("the input system input")]
    [SerializeField] PlayerInput mInput;

    // -- props --
    /// the musical key
    Key mKey;

    /// the line pattern
    Pattern mPattern;

    /// the player's inputs
    PlayerActions mActions;

    // -- lifecycle --
    void Awake() {
        // set props
        mPattern = new Pattern();
        mActions = new PlayerActions(mInput);
    }

    void Update() {
        // calc position from input
        mPattern.SetPercent(ReadPercent(mActions.Left));
    }

    void FixedUpdate() {
        // render line
        mLine.Start = mPattern.Point0;
        mLine.End = mPattern.Point1;
    }

    // -- commands --
    /// init this player on join
    public void Join(PlayerConfig cfg) {
        Debug.Log($"{cfg.Name} joined");

        // set props
        name = cfg.Name;
        mKey = new Key(cfg.Key);

        // set line props
        mLine.Color = cfg.Color;

        // set music props
        mMusic.Instrument = cfg.Instrument;
    }

    // -- queries --
    /// read percent complete from stick dir (oriented down, clockwise)
    float ReadPercent(Vector2 dir) {
        var a = -Vector2.SignedAngle(Vector2.down, dir);
        a = Mathf.Repeat(a + 360.0f, 360.0f);
        return a / 360.0f;
    }
}