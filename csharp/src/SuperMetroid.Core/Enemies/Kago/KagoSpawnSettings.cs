namespace SuperMetroid.Core.Enemies.Kago;

/// <summary>How many hits a Kago nest takes (#1472).</summary>
/// <param name="HitCount">The hits, 0 through 65535, it takes.</param>
internal sealed record KagoSpawnSettings(int HitCount) : EnemySpawnSettings
{
    /// <summary>The hits, 0 through 65535, it takes.</summary>
    public int HitCount { get; init; } = Ensure.BetweenInclusive(0, ushort.MaxValue, HitCount);
}
