using System.Collections;
using UnityEngine;

namespace Wands {

/// a hit stop effect on hit
public sealed class HitStop: MonoBehaviour, IEffectAsync<float> {
    // -- tuning --
    [Header("tuning")]
    [Tooltip("the hitstop duration curve")]
    [SerializeField] AnimationCurve m_Duration;

    // -- props --
    /// if hitstop is active
    bool m_IsActive;

    // -- commands --
    /// play the hit effect
    public IEnumerator PlayAsync(float mag) {
        var duration = m_Duration.Evaluate(mag);
        m_IsActive = true;
        yield return new WaitForSeconds(duration);
        m_IsActive = false;
    }

    // -- queries --
    /// if hitstop is active
    public bool IsActive {
        get => m_IsActive;
    }
}

}