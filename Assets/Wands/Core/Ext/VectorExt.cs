using UnityEngine;

/// extensions for Vector2
public static class Vec2 {
    /// normalize the vector
    public static Vector2 Normalize(Vector2 vec) {
        vec.Normalize();
        return vec;
    }

    /// the magnitude of the vector
    public static float Magnitude(Vector2 vec) {
        return vec.magnitude;
    }
}