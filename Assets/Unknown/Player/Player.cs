using UnityEngine;

public class Player: MonoBehaviour {
    // -- tuning --
    [Header("config")]
    [Tooltip("the player's musical key")]
    [SerializeField] Root mKeyOf = Root.C;

    // -- nodes --
    [Header("nodes")]
    [Tooltip("the music player")]
    [SerializeField] Musicker mMusic;

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

    void OnEnable() {
        mInputs.Enable();
    }

    void OnDisable() {
        mInputs.Disable();
    }
}