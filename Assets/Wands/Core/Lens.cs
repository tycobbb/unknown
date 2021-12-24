using System;

/// a type representing a mutable value
public struct Lens<T> {
    // -- props --
    /// the getter
    Func<T> m_Get;

    /// the setter
    Action<T> m_Set;

    // -- lifetime --
    public Lens(Func<T> get, Action<T> set) {
        m_Get = get;
        m_Set = set;
    }

    // -- commands --
    /// set the value
    public void Set(T val) {
        m_Set(val);
    }

    // -- props/hot --
    /// the value of this lens
    public T Val {
        get => m_Get.Invoke();
        set => m_Set.Invoke(value);
    }

    // -- operators --
    /// create a new lens mapping this one
    public Lens<U> Map<U>(Func<T, U> get, Func<U, T> set) {
        var l = this;

        return new Lens<U>(
            ( ) => get.Invoke(l.Val),
            (v) => l.Val = set.Invoke(v)
        );
    }
}