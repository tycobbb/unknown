using UnityEngine;

namespace Wands {

/// a collision w/ a wall
public struct Collision {
    // -- props --
    /// the collision point
    public readonly Vector2 Pos;

    /// the normal
    public readonly Vector2 Normal;

    // -- lifetime --
    /// create a new collision
    public Collision(Vector2 pos, Vector2 normal = default) {
        Pos = pos;
        Normal = normal;
    }
}

}