namespace SuperMetroid.Core.Enemies.MamaTurtles;

/// <summary>How fast a turtle first walks and which way (#1472).</summary>
/// <param name="Speed">Its horizontal speed, -32768 through 32767 pixels per update; negative walks left.</param>
internal sealed record TurtleSpawnSettings(int Speed) : EnemySpawnSettings
{
    /// <summary>Its horizontal speed, -32768 through 32767 pixels per update; negative walks left.</summary>
    public int Speed { get; init; } = Ensure.BetweenInclusive(short.MinValue, short.MaxValue, Speed);
}
