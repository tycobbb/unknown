using UnityEngine;

/// a collision w/ a wall
public struct Contact {
    // -- props --
    /// the collision point
    public readonly Vector2 Pos;

    /// the normal
    public readonly Vector2 Normal;

    /// the tangent
    public readonly Vector2 Tangent;

    // -- lifetime --
    /// create a new collision
    public Contact(Vector2 pos, Vector2 normal, Vector2 tangent) {
        Pos = pos;
        Normal = normal;
        Tangent = tangent;
    }
}
