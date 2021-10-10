using UnityEngine;
using UnityEngine.InputSystem;

/// the player
public class Player: MonoBehaviour {
    // -- config --
    [Header("config")]
    [Tooltip("the player's musical key")]
    [SerializeField] Root mKeyOf = Root.C;

    // -- nodes --
    [Header("nodes")]
    [Tooltip("the music player")]
    [SerializeField] Musicker mMusic;

    [Tooltip("the pattern")]
    [SerializeField] Pattern mPattern;

    [Tooltip("the pattern")]
    [SerializeField] Pattern mPattern2;

    // -- props --
    /// the musical key
    Key mKey;

    /// the player's inputs
    PlayerInput.PlayerActions mInputs;

    // -- lifecycle --
    void Awake() {
        // set props
        mKey = new Key(mKeyOf);
        mInputs = new PlayerInput().Player;
    }

    void Update() {
        mPattern.SetPercent(ReadPercent(mInputs.Left));
        mPattern2.SetPercent(ReadPercent(mInputs.Right));
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