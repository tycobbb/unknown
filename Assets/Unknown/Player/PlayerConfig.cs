using System;
using UnityEngine;

/// the config for a Player
[Serializable]
public struct PlayerConfig {
    [Tooltip("the player's name")]
    public string Name;

    [Tooltip("the player's musical key")]
    public Root Key;

    [Tooltip("the player's line color")]
    public Color Color;

    [Tooltip("the player's instrument")]
    public Instrument Instrument;
}