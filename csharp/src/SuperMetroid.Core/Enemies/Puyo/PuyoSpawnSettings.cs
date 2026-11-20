namespace SuperMetroid.Core.Enemies.Puyo;

/// <summary>How a Puyo hops (#1472, #1519).</summary>
/// <param name="HopCooldown">The updates between hops.</param>
/// <param name="HopDistance">The distance in pixels inside which it makes its short hops.</param>
internal sealed record PuyoSpawnSettings(int HopCooldown, int HopDistance) : EnemySpawnSettings
{
    /// <inheritdoc/>
    internal override EnemyPlaced Place(EnemySpeciesRegistry species, EnemyPlacing placing) => Ensure.NotNull(species).Place(this, placing);

    /// <inheritdoc/>
    internal override int PartsHeadedIn(EnemySpeciesRegistry species, EnemyDefinitionId definition) => Ensure.NotNull(species).PartsHeadedBy(this, definition);
}
