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

    // -- props --
    /// the number of hits
    int mHits = 0;

    /// the number of misses
    int mMisses = 0;

    // -- lifecycle --
    void Awake() {
        #if !UNITY_EDITOR && UNITY_WEBGL
        mLabel.fontSize *= 2.5f;
        #endif
    }

    // -- commands --
    /// record a hit
    public void RecordHit() {
        mHits++;
        SyncScore();
    }

    /// record a miss
    public void RecordMiss() {
        mMisses++;
        SyncScore();
    }

    /// update and show the current score
    void SyncScore() {
        // hits - misses
        var score = mHits - mMisses;

        // if score drops to 0 just reset everything
        if (score <= 0) {
            score = 0;
            mHits = 0;
            mMisses = 0;
        }

        // show the score
        mLabel.text = score.ToString();
    }
}