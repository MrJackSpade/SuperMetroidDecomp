namespace SuperMetroid.Core.Enemies.Nuclear;

/// <summary>How a nuclear waffle orbits (#1472, #1519).</summary>
/// <param name="AngularSpeed">How fast it sweeps, each update, toward increasing angles.</param>
/// <param name="OrbitRadius">Its orbit radius in pixels.</param>
/// <param name="Direction">The way it sweeps.</param>
/// <param name="WaitTime">The updates it waits between sweeps.</param>
internal sealed record NuclearWaffleSpawnSettings(AngularSpeed AngularSpeed, int OrbitRadius, NuclearWaffleSweepDirection Direction, int WaitTime) : EnemySpawnSettings
{
    /// <inheritdoc/>
    internal override EnemyPlaced Place(EnemySpeciesRegistry species, EnemyPlacing placing) => Ensure.NotNull(species).Place(this, placing);

    /// <inheritdoc/>
    internal override int PartsHeadedIn(EnemySpeciesRegistry species, EnemyDefinitionId definition) => Ensure.NotNull(species).PartsHeadedBy(this, definition);

    /// <summary>
    /// The sweep its <see cref="Direction"/> runs (#2474): its start and end angles, the spacing of its links and the
    /// angles at which they turn.
    /// </summary>
    internal NuclearWaffleSweepDefinition Sweep => NuclearWaffleSweeps.Sweep(Direction);
}
