namespace SuperMetroid.Core.Enemies.Skultera;

/// <summary>
/// The behaviour a Skultera's main AI runs.
/// Its values are arbitrary.
/// </summary>
public enum SkulteraAction
{
    /// <summary>
    /// Swims its heading (<see cref="SkulteraEnemyState.Heading"/>) at its spawn speed, with whole-pixel sine-offset
    /// changes; unobstructed movement advances the angle, and either axis collision starts a turn the other way.
    /// </summary>
    Swimming,
    /// <summary>Turns without swimming while its turn animation plays.</summary>
    Turning,
    /// <summary>
    /// The turn has finished, as its animation's last cue says (#2450): the next update installs swimming toward its
    /// heading, the way it turned, its wave stepping the other way.
    /// </summary>
    TurnFinished,
}
