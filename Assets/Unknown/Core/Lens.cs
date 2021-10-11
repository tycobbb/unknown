using System;

/// a type representing a mutable value
public struct Lens<T> {
    // -- props --
    /// the getter
    Func<T> mGet;

    /// the setter
    Action<T> mSet;

    // -- lifetime --
    public Lens(Func<T> get, Action<T> set) {
        mGet = get;
        mSet = set;
    }

    // -- props/hot --
    /// the value of this lens
    public T Val {
        get => mGet.Invoke();
        set => mSet.Invoke(value);
    }
}