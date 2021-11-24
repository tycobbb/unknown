using System;
using UnityEngine;

/// the params for a linear equation
[Serializable]
public struct Linear<T> {
    // -- props --
    [Tooltip("the destination value.")]
    public T Value;

    [Tooltip("a scale. interpretation is context-dependent.")]
    public float Scale;

    // -- lifetime --
    /// create a new linear value
    public Linear(T val, float scale) {
        Value = val;
        Scale = scale;
    }

    // -- factories --
    /// creates a "zero" value
    public static Linear<T> Zero {
        get => new Linear<T>(default, 0.0f);
    }
}

public static class LinearExt {
    public static Linear<float> Mul(this Linear<float> curr, float val, float scale) {
        var next = curr;
        next.Value *= val;
        next.Scale *= scale;
        return next;
    }
}