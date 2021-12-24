using DG.Tweening;
using UnityEngine;

namespace Wands {

/// a scale effect on the hand on hit
public sealed class HitScale: MonoBehaviour, IEffect<Nothing> {
    // -- tuning --
    [Header("tuning")]
    [Tooltip("the scale of the hand on hit")]
    [SerializeField] Linear<float> m_Scale;

    // -- nodes --
    [Header("nodes")]
    [Tooltip("the player's hand")]
    [SerializeField] Shapes.Disc m_Hand;

    // -- commands --
    /// play the hit effect
    public void Play(Nothing _ = default) {
        // get start and end radius
        var r0 = m_Hand.Radius;
        var r1 = m_Scale.Mul(r0, 0.5f);

        // get lens
        var radius = new Lens<float>(
            ( ) => m_Hand.Radius,
            (v) => m_Hand.Radius = v
        );

        // create tween
        radius
            .TweenTo(r0, r1)
            .SetLoops(2, LoopType.Yoyo)
            .SetEase(Ease.InOutCubic);
    }
}

}