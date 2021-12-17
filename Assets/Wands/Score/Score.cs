using System;
using TMPro;
using UnityEngine;

namespace Wands {

/// the score module
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
    [SerializeField] float m_HitSpeedScale = 0.3f;

    // -- nodes --
    [Header("config")]
    [Tooltip("the timer label")]
    [SerializeField] TMP_Text m_TimerLabel;

    [Tooltip("the finish time labels")]
    [SerializeField] TMP_Text[] m_FinishTimeLabels;

    [Tooltip("the player score labels")]
    [SerializeField] TMP_Text[] m_ScoreLabels;

    // -- props --
    /// the start time
    float? m_Elapsed;

    /// the player score records
    PlayerRecord[] m_Records = new PlayerRecord[2];

    // -- lifecycle --
    void FixedUpdate() {
        SyncTimer();
    }

    // -- commands --
    /// add the player to the score display
    public void AddPlayer(PlayerConfig cfg) {
        // add player
        var i = cfg.Index;
        m_Records[i] = new PlayerRecord();

        // show score label
        var label = m_ScoreLabels[i];
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
        m_Elapsed = 0.0f;
        m_TimerLabel.color = Color.white;
        m_TimerLabel.enabled = true;
    }

    /// record a hit
    public void RecordHit(PlayerConfig cfg, float speed) {
        var rec = m_Records[cfg.Index];

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
        var rec = m_Records[cfg.Index];
        rec.FinalHit = true;
        rec.FinishTime = m_Elapsed;
        ShowFinishTime(cfg);
    }

    /// sync the time display
    void SyncTimer() {
        if (m_Elapsed == null) {
            return;
        }

        m_Elapsed += Time.fixedDeltaTime;
        m_TimerLabel.text = TimeToString(m_Elapsed.Value);
    }

    /// show the next available finish time label
    void ShowFinishTime(PlayerConfig cfg) {
        var rec = m_Records[cfg.Index];

        // find next label
        var label = null as TMP_Text;
        foreach (var l in m_FinishTimeLabels) {
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
        for (var i = 0; i < m_Records.Length; i++) {
            var rec = m_Records[i];
            if (rec == null) {
                continue;
            }

            var score = CalcScore(rec);

            // apply ceiling
            if (score >= cMaxScore) {
                score = rec.FinalHit ? 42.0f : 39.9f;
            }

            m_ScoreLabels[i].text = $"{score:0.0}";
        }
    }

    // -- queries --
    /// calculate total score from record
    float CalcScore(PlayerRecord record) {
        return record.HitSpeed * m_HitSpeedScale;
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

}