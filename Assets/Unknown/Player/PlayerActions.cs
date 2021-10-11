using UnityEngine;
using UnityEngine.InputSystem;

/// the player's actions (wraps input)
public class PlayerActions {
    // -- props --
    /// the move action
    InputAction mMove;

    /// the flick action
    InputAction mFlick;

    // -- lifetime --
    /// create a new actions wrapper
    public PlayerActions(PlayerInput input) {
        mMove = input.currentActionMap["Move"];
        mFlick = input.currentActionMap["Flick"];
    }

    // -- queries --
    /// the move position
    public Vector2 Move {
        get => mMove.ReadValue<Vector2>();
    }

    /// the flick position
    public Vector2 Flick {
        get => mFlick.ReadValue<Vector2>();
    }
}
