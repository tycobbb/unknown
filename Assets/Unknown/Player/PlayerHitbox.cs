using System;
using UnityEngine;

/// the player's current hitbox
[Serializable]
public struct PlayerHitbox {
    // -- props --
    /// the current position
    public Vector2 Position;

    /// the radius
    public float Radius;

    // -- queries --
    /// check if the hitboxes overlap
    public bool Overlaps(PlayerHitbox other) {
        var dist = Vector2.Distance(Position, other.Position);
        var max = Radius + other.Radius;
        return dist <= max;
    }
}