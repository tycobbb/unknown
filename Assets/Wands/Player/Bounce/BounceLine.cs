using DG.Tweening;
using UnityEngine;

namespace Wands {

/// the lines when bouncing off a wall
[ExecuteAlways]
public sealed class BounceLine: MonoBehaviour, IEffect<Collision> {
    // -- tuning --
    [Header("tuning")]
    [Tooltip("the width of each line in noots")]
    [SerializeField] float m_Width = 0.3f;

    [Tooltip("the length of each line in units")]
    [SerializeField] float m_Length = 0.3f;

    [Tooltip("the travel distance of each line in noots")]
    [SerializeField] float m_OffsetMax = 0.3f;

    [Tooltip("the duration of the effect")]
    [SerializeField] float m_Duration = 2.0f;

    // -- nodes --
    [Header("nodes")]
    [Tooltip("the lines")]
    [SerializeField] Shapes.Line[] m_Lines;

    // -- lifecycle --
    void Start() {
        Style();
    }

    // -- commands --
    /// apply style to lines
    void Style() {
        foreach (var l in m_Lines) {
            l.Thickness = m_Width;
            l.End = new Vector3(m_Length, 0.0f, 0.0f);
        }
    }

    /// play the bounce effect
    public void Play(Collision wall) {
        var cr = Quaternion.LookRotation(Vector3.forward, wall.Normal);

        // build tween sequence
        var seq = DOTween.Sequence();

        // tween each line
        foreach (var l in m_Lines) {
            var lt = l.transform;

            // get lens for offset and alpha
            var ld = lt.right;
            var lp = lt.position;

            var offset = Lens<float>.State().OnSet((v) => {
                lt.position = lp + ld * v;
            });

            var alpha = new Lens<float>(
                ( ) => l.Color.a,
                (v) => l.Color = l.Color.A(v)
            );

            // add tweens to sequence
            seq = seq
                .Join(alpha.Tween(1.0f, 0.0f, m_Duration))
                .Join(offset.Tween(0.0f, m_OffsetMax, m_Duration));
        }

        // set sequence props
        seq = seq
            .SetEase(Ease.OutCubic)
            .OnComplete(() => Destroy(gameObject));

        // play sequence
        seq.Play();
    }
}

}
