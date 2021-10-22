using System.Collections;
using DG.Tweening;
using UnityEngine;

public class Hit: MonoBehaviour {
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
    /// play the hit effect w/ the player config and initial radius
    public void Play(PlayerConfig cfg, float r0) {
        StartCoroutine(PlayAsync(cfg, r0));
    }

    /// play the hit effect w/ the player config and initial radius
    IEnumerator PlayAsync(PlayerConfig cfg, float r0) {
        // get color
        var hsv = cfg.Color.ToHsv();
        var rgb = hsv.Add(v: 0.3f).ToRgb();

        // set initial state
        m_Circle.Color = rgb;
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
        Destroy(this);
    }
}
