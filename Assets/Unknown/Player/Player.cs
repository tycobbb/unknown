using UnityEngine;
using UnityEngine.InputSystem;
using I = UnityEngine.InputSystem;

// the player requires an InputSystem.PlayerInput component for a
// PlayerInputManager to spawn it, but we use the generated class to
// actually check input.

/// the player
[RequireComponent(typeof(I.PlayerInput))]
public class Player: MonoBehaviour {
    // -- nodes --
    [Header("nodes")]
    [Tooltip("the player's line")]
    [SerializeField] Shapes.Line mLine;

    [Tooltip("plays music on contact")]
    [SerializeField] Musicker mMusic;

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
        mPattern = new Pattern();
        mInputs = new PlayerInput().Player;
    }

    void Update() {
        // calc position from input
        mPattern.SetPercent(ReadPercent(mInputs.Left));
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

    // -- commands --
    /// init this player on join
    public void Join(PlayerConfig config) {
        name = config.Name;
        mKey = new Key(config.Key);
        mLine.Color = config.Color;
        mMusic.SetInstrument(config.Instrument);
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