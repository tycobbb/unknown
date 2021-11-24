using UnityEngine;

/// a collision w/ a wall
public struct Contact {
    // -- props --
    /// the collision point
    public readonly Vector2 Pos;

    /// the normal
    public readonly Vector2 Normal;

    // -- lifetime --
    /// create a new collision
    public Contact(Vector2 pos, Vector2 normal = default) {
        Pos = pos;
        Normal = normal;
    }
}
