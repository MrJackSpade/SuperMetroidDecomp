namespace SuperMetroid.Core.Enemies.Metroids;

/// <summary>How much ice damage freezes a Metroid (#1472).</summary>
/// <param name="IceDamageToFreeze">The ice damage, 0 through 65535, that freezes it.</param>
internal sealed record MetroidSpawnSettings(int IceDamageToFreeze) : EnemySpawnSettings
{
    /// <summary>The ice damage, 0 through 65535, that freezes it.</summary>
    public int IceDamageToFreeze { get; init; } = Ensure.BetweenInclusive(0, ushort.MaxValue, IceDamageToFreeze);
}
