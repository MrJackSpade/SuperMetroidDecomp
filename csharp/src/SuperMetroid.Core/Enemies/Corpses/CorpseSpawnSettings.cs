namespace SuperMetroid.Core.Enemies.Corpses;

/// <summary>Which authored corpse of its species a placement is (#1472).</summary>
/// <param name="Variant">The corpse, 0 through 32767.</param>
internal sealed record CorpseSpawnSettings(int Variant) : EnemySpawnSettings
{
    /// <summary>The corpse, 0 through 32767.</summary>
    public int Variant { get; init; } = Ensure.BetweenInclusive(0, short.MaxValue, Variant);
}
