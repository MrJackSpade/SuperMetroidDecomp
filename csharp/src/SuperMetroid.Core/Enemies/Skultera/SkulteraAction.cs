namespace SuperMetroid.Core.Enemies.Skultera;

/// <summary>
/// The behaviour a Skultera's main AI runs.
/// Its values are arbitrary.
/// </summary>
public enum SkulteraAction
{
    /// <summary>
    /// Swims its heading (<see cref="SkulteraEnemyState.Heading"/>) at that side's speed, with whole-pixel sine-offset
    /// changes; unobstructed movement advances the angle, and either axis collision starts a turn the other way.
    /// </summary>
    Swimming,
    /// <summary>
    /// Waits without swimming until animation sets the turn-finished flag, then clears it, negates the angle delta, and
    /// installs swimming toward its heading, the way it turned.
    /// </summary>
    Turning,
}
