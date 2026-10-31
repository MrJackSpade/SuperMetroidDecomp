namespace SuperMetroid.Core.Enemies.Chozo;

/// <summary>Which of the two Chozo statues a placement is (#1472).</summary>
/// <param name="Variant">The statue, 0 or 1.</param>
internal sealed record ChozoStatueSpawnSettings(int Variant) : EnemySpawnSettings
{
    /// <summary>The statue, 0 or 1.</summary>
    public int Variant { get; init; } = Ensure.BetweenInclusive(0, short.MaxValue, Variant);
}
