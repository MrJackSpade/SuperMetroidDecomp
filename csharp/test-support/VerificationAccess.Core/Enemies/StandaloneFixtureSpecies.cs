namespace SuperMetroid.Core.Enemies;

/// <summary>
/// A species for the standalone fixture places a check binds an enemy in (#2558): a boss encounter's body, which the
/// encounter holds, or the stand-in whose definition a check reads; each runs nothing of its own.
/// </summary>
internal sealed class StandaloneFixtureSpecies : IEnemyEntityFactory
{
    /// <summary>The registry of standalone places a check binds an enemy in.</summary>
    internal static EnemySpeciesRegistry Registry { get; } = EnemyPlaceRegistries.Of(new StandaloneFixtureSpecies());

    /// <summary>The definitions the species places.</summary>
    internal static IReadOnlyList<EnemyDefinitionId> Definitions { get; } = [EnemyDefinitionId.MotherBrainBody, EnemyDefinitionId.DraygonBody, EnemyPlaceFixtures.StandIn];

    /// <inheritdoc/>
    public void OfferCreations(EnemySpeciesRegistrar registrar) =>
        Ensure.NotNull(registrar).Spawns<EnemySpawnSettings>(Definitions, (occupant, _, place, common) => EnemyOccupant.PlacedOnly(new Body()));

    /// <summary>An enemy that runs nothing of its own.</summary>
    private sealed class Body : IEnemyEntity
    {
        /// <inheritdoc/>
        public EnemyCapabilities Capabilities => EnemyCapabilities.None;
    }
}
