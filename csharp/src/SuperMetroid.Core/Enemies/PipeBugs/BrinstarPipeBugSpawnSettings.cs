namespace SuperMetroid.Core.Enemies.PipeBugs;

/// <summary>Whether a Brinstar pipe bug is the strong kind (#1472).</summary>
/// <param name="Kind">Which kind of pipe bug it is.</param>
internal sealed record BrinstarPipeBugSpawnSettings(BrinstarPipeBugKind Kind) : EnemySpawnSettings
{
    /// <inheritdoc/>
    internal override EnemyPlaced Place(EnemySpeciesRegistry species, EnemyPlacing placing) => Ensure.NotNull(species).Place(this, placing);

    /// <inheritdoc/>
    internal override int PartsHeadedIn(EnemySpeciesRegistry species, EnemyDefinitionId definition) => Ensure.NotNull(species).PartsHeadedBy(this, definition);
}
