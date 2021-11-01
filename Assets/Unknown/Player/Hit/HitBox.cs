using System;
using UnityEngine;

/// a cicular hitbox w/ pos and radius
[Serializable]
public struct HitBox {
    // -- props --
    /// the current position
    public Vector2 Pos;

    /// the radius
    public float Radius;

    // -- queries --
    /// check if the hitboxes overlap
    public bool Overlaps(HitBox other) {
        var dist = Vector2.Distance(Pos, other.Pos);
        var max = Radius + other.Radius;
        return dist <= max;
    }
}