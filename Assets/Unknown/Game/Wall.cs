using UnityEngine;

// -- types --
/// a wall
public interface Wall {
    /// the wall's index
    int Index { get; }

    /// check for a collision at the point
    Contact? Collide(Vector2 pt);
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
    public readonly int Index;

    // -- lifetime --
    /// create a new wall
    SquareWall(int i) {
        Index = i;
    }

    // -- Wall --
    /// if a point overlaps the wall
    public Contact? Collide(Vector2 p) {
        // if point overlaps
        var isOverlap = Index switch {
            c_Top    => p.y >= 1.0f,
            c_Bottom => p.y <= 0.0f,
            c_Left   => p.x <= 0.0f,
            _        => p.x >= 1.0f,
        };

        if (!isOverlap) {
            return null;
        }

        // init the collision
        var collision = Index switch {
            c_Top    => new Contact(p, Vector2.down,  Vector2.right),
            c_Bottom => new Contact(p, Vector2.up,    Vector2.right),
            c_Left   => new Contact(p, Vector2.right, Vector2.up),
            _        => new Contact(p, Vector2.left,  Vector2.up),
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
