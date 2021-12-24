using System.Collections;
using DG.Tweening;
using UnityEngine;

namespace Wands {

/// the event data for the hit ring
public readonly struct HitRingEvent {
    /// the player config for the hit
    public readonly PlayerConfig Config;

    /// the hitbox of the thing hit
    public readonly HitBox HitBox;

    /// create a new event
    public HitRingEvent(PlayerConfig config, HitBox hitBox) {
        Config = config;
        HitBox = hitBox;
    }
}

/// a hit ring that expands from the hit position
public sealed class HitRing: MonoBehaviour, IEffect<HitRingEvent> {
    // -- tuning --
    [Header("tuning")]
    [Tooltip("the length of the effect")]
    [SerializeField] float m_Duration = 2.0f;

    [Tooltip("the multiplier to grow the hit radius by")]
    [SerializeField] float m_SizeMultiplier = 10.0f;

    // -- nodes --
    [Header("nodes")]
    [Tooltip("the hit circle")]
    [SerializeField] Shapes.Disc m_Circle;

    // -- commands --
    /// play the hit effect
    public void Play(HitRingEvent evt) {
        // build tween sequence
        var seq = DOTween.Sequence();

        // get lens for circle radius & color
        var radius = new Lens<float>(
            ( ) => m_Circle.Radius,
            (v) => m_Circle.Radius = v
        );

        var alpha = new Lens<float>(
            ( ) => m_Circle.Color.a,
            (v) => m_Circle.Color = m_Circle.Color.A(v)
        );

        // get props from event
        var c0 = evt.Config.Color.ToHsv().Add(v: 0.3f).ToRgb();
        var r0 = evt.HitBox.Radius;
        var r1 = r0 * m_SizeMultiplier;

        // build tween
        seq = seq
            .Join(radius.Tween(
                src: r0,
                dst: r1,
                dur: m_Duration
            ))
            .Join(alpha.Tween(
                src: c0.a,
                dst: 0.0f,
                dur: m_Duration
            ))
            .SetEase(Ease.OutCubic)
            .OnComplete(() => Destroy(gameObject));

        // and play it
        seq.Play();
    }
}

}
