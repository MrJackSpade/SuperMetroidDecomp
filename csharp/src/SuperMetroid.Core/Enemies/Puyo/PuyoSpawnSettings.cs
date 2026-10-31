namespace SuperMetroid.Core.Enemies.Puyo;

/// <summary>How a Puyo hops (#1472).</summary>
/// <param name="HopCooldown">Updates, 0 through 65535, between hops.</param>
/// <param name="HopDistance">The distance, 0 through 65535 pixels, inside which it makes its short hops.</param>
internal sealed record PuyoSpawnSettings(int HopCooldown, int HopDistance) : EnemySpawnSettings
{
    /// <summary>Updates, 0 through 65535, between hops.</summary>
    public int HopCooldown { get; init; } = Ensure.BetweenInclusive(0, ushort.MaxValue, HopCooldown);

    /// <summary>The distance, 0 through 65535 pixels, inside which it makes its short hops.</summary>
    public int HopDistance { get; init; } = Ensure.BetweenInclusive(0, ushort.MaxValue, HopDistance);
}
