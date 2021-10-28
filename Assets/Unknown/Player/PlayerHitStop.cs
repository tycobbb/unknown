using System.Collections;
using UnityEngine;

/// the hit stop effect
public class PlayerHitStop: MonoBehaviour {
    // -- tuning --
    [Header("tuning")]
    [Tooltip("the hit stop duration curve")]
    [SerializeField] AnimationCurve m_DurationCurve;

    // -- props --
    /// if hitstop is active
    bool m_IsActive;

    // -- commands --
    /// play hitstop effect
    public void Play(float mag) {
        StartCoroutine(PlayAsync(mag));
    }

    /// play hitstop effect
    IEnumerator PlayAsync(float mag) {
        var duration = m_DurationCurve.Evaluate(mag);

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