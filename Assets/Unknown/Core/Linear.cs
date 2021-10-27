using System;
using UnityEngine;

/// the params for a linear equation
[Serializable]
public struct Linear {
    // -- props --
    [Tooltip("the destination value.")]
    public float Value;

    /// the time value, interpretable in a variety of ways
    [Tooltip("the time value. interpretation is context-dependent.")]
    public float Scale;

    // -- lifetime --
    /// create a new linear value
    public Linear(float val, float scale) {
        Value = val;
        Scale = scale;
    }

    // -- factories --
    /// creates a "zero" value
    public static Linear Zero {
        get => new Linear(0.0f, 0.0f);
    }
}