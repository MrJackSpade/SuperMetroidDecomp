namespace SuperMetroid.Core.Enemies.Animals;

/// <summary>How an Etecoon starts: the route it takes after its first landing, the one thing its placements vary (#1472, #1519, #2023).</summary>
/// <param name="Route">The route it takes after its first landing.</param>
internal sealed record EtecoonSpawnSettings(EtecoonRoute Route) : EnemySpawnSettings
{
    /// <inheritdoc/>
    internal override EnemyPlaced Place(EnemySpeciesRegistry species, EnemyPlacing placing) => Ensure.NotNull(species).Place(this, placing);
}
