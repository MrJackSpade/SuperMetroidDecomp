namespace SuperMetroid.Core.Enemies.Cacatac;

/// <summary>
/// The behaviour Cacatac's main AI runs each frame. Initialization selects one of the first three by its authored
/// direction (moving left, moving right, or stopped while an attack animation owns the actor); no other start is
/// authored.
/// Its values are arbitrary.
/// </summary>
public enum CacatacAction
{
    /// <summary>Advances the split 16.16 left velocity, turns right below the minimum X bound, then evaluates the random attack gate.</summary>
    MovingLeft = 1,
    /// <summary>Advances the split 16.16 right velocity, turns left at or beyond the maximum X bound, then evaluates the random attack gate.</summary>
    MovingRight,
    /// <summary>Performs no patrol update while attack animation cues own the actor; also the initial target for population direction selector two.</summary>
    Stopped,
}
