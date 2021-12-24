using UnityEngine;

namespace Wands {

// -- types --
/// a wall
public interface Wall {
    /// check for a collision at the point
    Collision? Collide(Vector2 pt);
}

// -- impls --
/// one of the room's walls
public struct SquareWall: Wall {
    // -- constants --
    const int c_Top = 0;
    const int c_Bottom = 1;
    const int c_Left = 2;
    const int c_Right = 3;

    // -- props --
    /// the index
    public readonly int m_I;

    // -- lifetime --
    /// create a new wall
    SquareWall(int i) {
        m_I = i;
    }

    // -- Wall --
    /// if a point overlaps the wall
    public Collision? Collide(Vector2 p) {
        // if point overlaps
        var isOverlap = m_I switch {
            c_Top    => p.y >= 1.0f,
            c_Bottom => p.y <= 0.0f,
            c_Left   => p.x <= 0.0f,
            _        => p.x >= 1.0f,
        };

        if (!isOverlap) {
            return null;
        }

        // init the collision
        var collision = m_I switch {
            c_Top    => new Collision(p, Vector2.down),
            c_Bottom => new Collision(p, Vector2.up),
            c_Left   => new Collision(p, Vector2.right),
            _        => new Collision(p, Vector2.left),
        };

        return collision;
    }

    // -- factories --
    /// create a list of 4 square walls
    public static Wall[] InitSquare() {
        return new Wall[4] {
            new SquareWall(c_Top),
            new SquareWall(c_Bottom),
            new SquareWall(c_Left),
            new SquareWall(c_Right)
        };
    }
}

}