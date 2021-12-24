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

    /// add a side effect on set
    public Lens<T> OnSet(Action<T> action) {
        var l = this;

        return new Lens<T>(
            ( ) => l.Val,
            (v) => {
                l.Val = v;
                action.Invoke(v);
            }
        );
    }

    // -- factories --
    /// create a lens with an internal value
    public static Lens<T> State(T from = default) {
        var val = from;

        return new Lens<T>(
            ( ) => val,
            (v) => val = v
        );
    }
}