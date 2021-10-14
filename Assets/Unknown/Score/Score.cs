using System;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;

/// the score ui label
public class Score: MonoBehaviour {
    // -- constants --
    /// the winning score
    const float cMaxScore = 42.0f;

    // -- module --
    /// get the module
    public static Score Get {
        get => FindObjectOfType<Score>();
    }

    // -- tuning --
    [Header("tuning")]
    [Tooltip("the scale to apply to hit speed")]
    [SerializeField] float mHitSpeedScale = 0.3f;

    // -- nodes --
    [Header("config")]
    [Tooltip("the timer label")]
    [SerializeField] TMP_Text mTimerLabel;

    [FormerlySerializedAs("mSavedTimerLabels")]
    [FormerlySerializedAs("mLabels")]
    [Tooltip("the finish time labels")]
    [SerializeField] TMP_Text[] mFinishTimeLabels;

    [FormerlySerializedAs("mLabels")]
    [Tooltip("the player score labels")]
    [SerializeField] TMP_Text[] mScoreLabels;

    // -- props --
    /// the start time
    float? mElapsed;

    /// the player score records
    PlayerRecord[] mRecords = new PlayerRecord[2];

    // -- lifecycle --
    void FixedUpdate() {
        SyncTimer();
    }

    // -- commands --
    /// add the player to the score display
    public void AddPlayer(PlayerConfig cfg) {
        // add player
        var i = cfg.Index;
        mRecords[i] = new PlayerRecord();

        // show score label
        var label = mScoreLabels[i];
        label.color = cfg.Color;
        label.enabled = true;

        // start timer once second player joins
        if (i >= 1) {
            RecordStart();
        }

        // show scores
        SyncScores();
        SyncTimer();
    }

    /// record the start of the game
    void RecordStart() {
        mElapsed = 0.0f;
        mTimerLabel.color = Color.white;
        mTimerLabel.enabled = true;
    }

    /// record a hit
    public void RecordHit(PlayerConfig cfg, float speed) {
        var rec = mRecords[cfg.Index];

        // record finish if already reached max
        if (!rec.FinalHit && CalcScore(rec) >= cMaxScore) {
            RecordFinish(cfg);
        }

        // add hit
        rec.HitSpeed += speed;

        // sync display
        SyncScores();
    }

    /// record a player finishing
    void RecordFinish(PlayerConfig cfg) {
        var rec = mRecords[cfg.Index];
        rec.FinalHit = true;
        rec.FinishTime = mElapsed;
        ShowFinishTime(cfg);
    }

    /// sync the time display
    void SyncTimer() {
        if (mElapsed == null) {
            return;
        }

        mElapsed += Time.fixedDeltaTime;
        mTimerLabel.text = TimeToString(mElapsed.Value);
    }

    /// show the next available finish time label
    void ShowFinishTime(PlayerConfig cfg) {
        var rec = mRecords[cfg.Index];

        // find next label
        var label = null as TMP_Text;
        foreach (var l in mFinishTimeLabels) {
            if (l.enabled) {
                continue;
            }

            if (label == null || l.transform.position.y < label.transform.position.y) {
                label = l;
            }
        }

        // if there's a label and time
        if (label == null || rec.FinishTime == null) {
            return;
        }

        // show the label
        var c = Color.white;
        c.a = 0.5f;
        label.color = c;
        label.enabled = true;

        // with the time
        label.text = TimeToString(rec.FinishTime.Value);
    }

    /// sync the score display
    void SyncScores() {
        for (var i = 0; i < mRecords.Length; i++) {
            var rec = mRecords[i];
            if (rec == null) {
                continue;
            }

            var score = CalcScore(rec);

            // apply ceiling
            if (score >= cMaxScore) {
                score = rec.FinalHit ? 42.0f : 39.9f;
            }

            mScoreLabels[i].text = $"{score:0.0}";
        }
    }

    // -- queries --
    /// calculate total score from record
    float CalcScore(PlayerRecord record) {
        return record.HitSpeed * mHitSpeedScale;
    }

    /// format time as a string
    string TimeToString(float time) {
        var span = TimeSpan.FromSeconds(time);
        return span.ToString(@"mm\:ss\.ff");
    }

    // -- data --
    /// the record for a single player's actions
    sealed class PlayerRecord {
        /// the cumulative hit speed
        public float HitSpeed;

        /// if the player made the final hit
        public bool FinalHit;

        /// the players completion time
        public float? FinishTime;
    }
}