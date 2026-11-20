namespace SuperMetroid.Core.Enemies.PipeBugs;

/// <summary>
/// How a Norfair pipe bug flies and whether it leads its formation (#1472, #1519): the leader and its followers each have
/// settings of their own (#2536), so only the leader carries the width Samus must come within.
/// </summary>
/// <param name="Speed">The speed at which it flies across the room.</param>
internal abstract record NorfairPipeBugSpawnSettings(SignedSixteenSixteen Speed) : EnemySpawnSettings
{
    /// <inheritdoc/>
    internal override EnemyPlaced Place(EnemySpeciesRegistry species, EnemyPlacing placing) => Ensure.NotNull(species).Place(this, placing);

    /// <inheritdoc/>
    internal override int PartsHeadedIn(EnemySpeciesRegistry species, EnemyDefinitionId definition) => Ensure.NotNull(species).PartsHeadedBy(this, definition);
}
