using UnityEngine;
using UnityEngine.InputSystem;

/// the player's actions (wraps input)
public class PlayerActions {
    // -- props --
    /// the move action
    InputAction m_Move;

    /// the flick action
    InputAction m_Flick;

    // -- lifetime --
    /// create a new actions wrapper
    public PlayerActions(PlayerInput input) {
        m_Move = input.currentActionMap["Move"];
        m_Flick = input.currentActionMap["Flick"];
    }

    // -- queries --
    /// the move position
    public Vector2 Move {
        get => m_Move.ReadValue<Vector2>();
    }

    /// the flick position
    public Vector2 Flick {
        get => m_Flick.ReadValue<Vector2>();
    }
}
