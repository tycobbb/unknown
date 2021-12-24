using System;
using UnityEngine;
using U = UnityEngine;

/// the params for a linear equation
[Serializable]
public struct Linear<T> {
    // -- props --
    [Tooltip("the destination value")]
    public T Val;

    [Tooltip("the length; interpretation is context-dependent.")]
    public float Len;

    // -- lifetime --
    /// create a new linear value
    public Linear(T val, float scale) {
        Val = val;
        Len = scale;
    }

    // -- factories --
    /// creates a "zero" value
    public static Linear<T> Zero {
        get => new Linear<T>(default, 0.0f);
    }
}

/// extensions on linear float values
public static class LinearExt {
    /// samples a random value from the linear float (value is min, scale is len)
    public static float Sample(this Linear<float> l) {
        return l.Val + U.Random.value * l.Len;
    }

    /// multiplies two linear values together
    public static Linear<float> Mul(this Linear<float> curr, Linear<float> other) {
        var next = curr;
        next.Val *= other.Val;
        next.Len *= other.Len;
        return next;
    }
}