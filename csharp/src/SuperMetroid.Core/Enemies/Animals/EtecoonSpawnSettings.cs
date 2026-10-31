namespace SuperMetroid.Core.Enemies.Animals;

/// <summary>How an Etecoon starts: its heading, the hops it teaches and its route (#1472).</summary>
/// <param name="Heading">The way it first heads.</param>
/// <param name="TeachingHops">The hops, 0 through 255, it has taught.</param>
/// <param name="Route">Its route, 0 through 255.</param>
/// <param name="QuakePauseFrames">Updates, 0 through 255, it pauses for an earthquake.</param>
internal sealed record EtecoonSpawnSettings(HorizontalDirection Heading, int TeachingHops, int Route, int QuakePauseFrames) : EnemySpawnSettings
{
    /// <summary>The hops, 0 through 255, it has taught.</summary>
    public int TeachingHops { get; init; } = Ensure.BetweenInclusive(0, byte.MaxValue, TeachingHops);

    /// <summary>Its route, 0 through 255.</summary>
    public int Route { get; init; } = Ensure.BetweenInclusive(0, byte.MaxValue, Route);

    /// <summary>Updates, 0 through 255, it pauses for an earthquake.</summary>
    public int QuakePauseFrames { get; init; } = Ensure.BetweenInclusive(0, byte.MaxValue, QuakePauseFrames);
}
