using System;
using UnityEngine;

/// a player's config
[Serializable]
public sealed class PlayerConfig {
    [Tooltip("the players index")]
    public int Index;

    [Tooltip("the player's musical key")]
    public Root Key;

    [Tooltip("the player's line color")]
    public Color Color;

    [Tooltip("the player's initial percent")]
    public float Percent;

    [Tooltip("the player's instrument")]
    public Instrument VoiceInstrument;

    [Tooltip("the player's footsteps instrument")]
    public Instrument FootstepsInstrument;
}