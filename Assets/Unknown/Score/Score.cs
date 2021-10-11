using TMPro;
using UnityEngine;

/// the score ui label
public class Score: MonoBehaviour {
    // -- module --
    /// get the module
    public static Score Get {
        get => FindObjectOfType<Score>();
    }

    // -- config --
    [Tooltip("the player score labels")]
    [SerializeField] TMP_Text[] mLabels;

    // -- lifecycle --
    void Awake() {
        #if !UNITY_EDITOR && UNITY_WEBGL
        foreach (var label of mLabels) {
            label.fontSize *= 2.5f;
        }
        #endif
    }

    // -- commands --
    /// add the player to the score display
    public void AddPlayer(PlayerConfig cfg) {
        mLabels[cfg.Index].color = cfg.Color;
    }
}