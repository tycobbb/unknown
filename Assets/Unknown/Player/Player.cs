using UnityEngine;
using UnityEngine.InputSystem;

/// the player
public class Player: MonoBehaviour {
    // -- config --
    [Header("config")]
    [Tooltip("the player's musical key")]
    [SerializeField] Root mKeyOf = Root.C;

    [Tooltip("the player's line color")]
    [SerializeField] Color mColor;

    // -- nodes --
    [Header("nodes")]
    [Tooltip("the music player")]
    [SerializeField] Musicker mMusic;

    [Tooltip("the line renderer")]
    [SerializeField] Shapes.Line mLine;

    // -- props --
    /// the musical key
    Key mKey;

    /// the line pattern
    Pattern mPattern;

    /// the player's inputs
    PlayerInput.PlayerActions mInputs;

    // -- lifecycle --
    void Awake() {
        // set props
        mKey = new Key(mKeyOf);
        mPattern = new Pattern();
        mInputs = new PlayerInput().Player;
    }

    void Update() {
        // update style
        mLine.Color = mColor;

        // calc position from input
        if (name == "Player1") {
            mPattern.SetPercent(ReadPercent(mInputs.Left));
        } else {
            mPattern.SetPercent(ReadPercent(mInputs.Right));
        }
    }

    void FixedUpdate() {
        // render line
        mLine.Start = mPattern.Point0;
        mLine.End = mPattern.Point1;
    }

    void OnEnable() {
        mInputs.Enable();
    }

    void OnDisable() {
        mInputs.Disable();
    }

    // -- queries --
    /// read percent complete from stick dir (oriented down, clockwise)
    float ReadPercent(InputAction stick) {
        var d = stick.ReadValue<Vector2>();
        var a = -Vector2.SignedAngle(Vector2.down, d);
        a = Mathf.Repeat(a + 360.0f, 360.0f);
        return a / 360.0f;
    }
}