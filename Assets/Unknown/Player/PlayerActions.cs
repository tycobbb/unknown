using UnityEngine;
using UnityEngine.InputSystem;

/// the player's actions (wraps input)
public class PlayerActions {
    // -- props --
    /// the left analog stick
    InputAction mLeft;

    /// the right analog stick
    InputAction mRight;

    // -- lifetime --
    /// create a new actions wrapper
    public PlayerActions(PlayerInput input) {
        mLeft = input.currentActionMap["Left"];
        mRight = input.currentActionMap["Right"];
    }

    // -- queries --
    /// the left stick position
    public Vector2 Left {
        get => mLeft.ReadValue<Vector2>();
    }

    /// the right stick position
    public Vector2 Right {
        get => mRight.ReadValue<Vector2>();
    }
}
