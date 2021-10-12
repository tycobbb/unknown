using System;

/// a thing that can change and tracks changes
public struct Draft<T> where T: IEquatable<T> {
    // -- props --
    /// the value
    T mValue;

    /// if last mutation changed the value
    bool mIsDirty;

    // -- lifetime --
    /// create a draft with an initial value
    public Draft(T value) {
        mValue = value;
        mIsDirty = false;
    }

    // -- props/hot --
    /// the underlying value
    public T Val {
        get => mValue;
        set {
            var prev = mValue;
            mValue = value;
            mIsDirty = !prev.Equals(value);
        }
    }

    /// if this value is dirty
    public bool IsDirty {
        get => mIsDirty;
    }
}