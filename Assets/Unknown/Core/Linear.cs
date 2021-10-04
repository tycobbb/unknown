using System;
using UnityEngine;

/// a value that can be lerp'd
[Serializable]
public struct Linear<T> {
    // -- props --
    [Tooltip("the destination value.")]
    public T Dst;

    /// the time value, interpretable in a variety of ways
    [Tooltip("the time value. interpretation is context-dependent.")]
    public float Time;

    // -- lifetime --
    /// create a new linear value
    public Linear(T dst, float time) {
        Dst = dst;
        Time = time;
    }

    // -- factories --
    /// creates a "zero" value
    public static Linear<T> Zero {
        get => new Linear<T>(default, 0.0f);
    }
}