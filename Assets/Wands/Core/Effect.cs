using System.Collections;
using UnityEngine;

// -- types --
/// an effect; something that can be `Play`ed on demand.
public interface IEffect<E> {
    void Play(E evt = default);
}

/// an async effect; requires a `MonoBehaivour` to start
public interface IEffectAsync<E> {
    IEnumerator PlayAsync(E evt = default);
}

/// spawns an effect given an event
public interface IEffectSource<E> {
    IEffect<E> Init(E evt = default);
}

// -- impls --
/// extensions for event types
public static class EffectExt {
    // -- IEffectSource --
    /// spawns and plays this source's effect
    public static void Play<E>(
        this IEffectSource<E> source,
        E evt = default
    ) {
        source.Init(evt).Play(evt);
    }

    // -- IEffectAsync --
    /// plays this async effect from the source
    public static void Play<E>(
        this IEffectAsync<E> effect,
        E evt,
        MonoBehaviour source
    ) {
        source.StartCoroutine(effect.PlayAsync(evt));
    }

    /// bridge this async effect into an `IEffect` given a source
    public static IEffect<E> IntoEffect<E>(
        this IEffectAsync<E> effect,
        MonoBehaviour source
    ) {
        return new StartAsyncEffect<E>(effect, source);
    }
}

// -- boxes --
public struct StartAsyncEffect<E>: IEffect<E> {
    // -- props --
    /// the underlying async effect
    IEffectAsync<E> m_Effect;

    /// the behaviour that starts this effect
    MonoBehaviour m_Source;

    // -- lifetime --
    /// create an IEffect bridge to start the async effect
    public StartAsyncEffect(IEffectAsync<E> effect, MonoBehaviour source) {
        m_Effect = effect;
        m_Source = source;
    }

    // -- IEffect --
    public void Play(E evt) {
        m_Effect.Play(evt, m_Source);
    }
}
