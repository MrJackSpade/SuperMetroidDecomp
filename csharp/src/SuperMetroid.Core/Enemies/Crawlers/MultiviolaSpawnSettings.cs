namespace SuperMetroid.Core.Enemies.Crawlers;

/// <summary>Which way a Multiviola first bounces and how fast (#1472).</summary>
/// <param name="Heading">Its first heading, 0 through 65535.</param>
/// <param name="Speed">Its speed, 0 through 255.</param>
internal sealed record MultiviolaSpawnSettings(int Heading, int Speed) : EnemySpawnSettings
{
    /// <summary>Its first heading, 0 through 65535.</summary>
    public int Heading { get; init; } = Ensure.BetweenInclusive(0, ushort.MaxValue, Heading);

    /// <summary>Its speed, 0 through 255.</summary>
    public int Speed { get; init; } = Ensure.BetweenInclusive(0, byte.MaxValue, Speed);
}
