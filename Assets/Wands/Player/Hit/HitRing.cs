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
public sealed class HitRing: MonoBehaviour, IEffectAsync<HitRingEvent> {
    // -- tuning --
    [Header("tuning")]
    [Tooltip("the length of the effect")]
    [SerializeField] float m_Duration = 2.0f;

    [Tooltip("the multiplier to grow the hit radius by")]
    [SerializeField] float m_SizeMultiplier = 10.0f;

    // -- config --
    [Header("config")]
    [Tooltip("the hit circle")]
    [SerializeField] Shapes.Disc m_Circle;

    // -- commands --
    /// play the hit effect
    public IEnumerator PlayAsync(HitRingEvent evt) {
        // get props from event
        var c0 = evt.Config.Color;
        var r0 = evt.HitBox.Radius;

        // get color
        var color = c0.ToHsv().Add(v: 0.3f).ToRgb();

        // set initial state
        m_Circle.Color = color;
        m_Circle.Radius = r0;

        // tween radius
        DOTween
            .To(
                ( ) => m_Circle.Radius,
                (v) => m_Circle.Radius = v,
                r0 * m_SizeMultiplier,
                m_Duration
            )
            .SetEase(Ease.OutCubic);

        // tween alpha
        DOTween
            .To(
                ( ) => m_Circle.Color.a,
                (v) => m_Circle.Color = m_Circle.Color.A(v),
                0.0f,
                m_Duration
            )
            .SetEase(Ease.OutCubic);

        // remove on complete
        yield return new WaitForSeconds(m_Duration);
        Destroy(gameObject);
    }
}

}
