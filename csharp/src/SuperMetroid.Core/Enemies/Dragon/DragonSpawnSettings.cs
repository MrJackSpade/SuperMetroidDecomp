namespace SuperMetroid.Core.Enemies.Dragon;

/// <summary>
/// Whether a Dragon placement is the body or its wing (#1472), which its species reads once to build that part's own
/// entity (#2175).
/// </summary>
/// <param name="Part">Which part of the dragon it is.</param>
internal sealed record DragonSpawnSettings(DragonPart Part) : EnemySpawnSettings
{
    /// <inheritdoc/>
    internal override EnemyPlaced Place(EnemySpeciesRegistry species, EnemyPlacing placing) => Ensure.NotNull(species).Place(this, placing);
}
