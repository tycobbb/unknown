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
    [Tooltip("the score label")]
    [SerializeField] TMP_Text mLabel;

    // -- lifecycle --
    void Awake() {
        #if !UNITY_EDITOR && UNITY_WEBGL
        mLabel.fontSize *= 2.5f;
        #endif
    }
}