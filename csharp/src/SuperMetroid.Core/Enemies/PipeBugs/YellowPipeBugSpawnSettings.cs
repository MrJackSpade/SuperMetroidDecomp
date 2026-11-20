namespace SuperMetroid.Core.Enemies.PipeBugs;

/// <summary>Which way a yellow pipe bug flies and how fast (#1472, #1519).</summary>
/// <param name="Facing">The way it flies.</param>
/// <param name="Speed">The speed, in pixels per update, at which it flies.</param>
internal sealed record YellowPipeBugSpawnSettings(HorizontalDirection Facing, SignedSixteenSixteen Speed) : EnemySpawnSettings
{
    /// <inheritdoc/>
    internal override EnemyPlaced Place(EnemySpeciesRegistry species, EnemyPlacementRequest placing) => Ensure.NotNull(species).Place(this, placing);

    /// <inheritdoc/>
    internal override int PartsHeadedIn(EnemySpeciesRegistry species, EnemyDefinitionId definition) => Ensure.NotNull(species).PartsHeadedBy(this, definition);
}
