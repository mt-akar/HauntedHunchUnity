using System;

/// <summary>
/// The type of player
/// </summary>
[Flags]
public enum PlayerType
{
    /// <summary>
    /// Starting player
    /// </summary>
    Self = 0,

    /// <summary>
    /// Second player
    /// </summary>
    Opponent = 1
}

