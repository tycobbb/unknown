using UnityEngine;

/// static extensions for Vector2
public static class Vec2 {
    /// normalize the vector
    public static Vector2 Normalize(Vector2 vec) {
        vec.Normalize();
        return vec;
    }
}