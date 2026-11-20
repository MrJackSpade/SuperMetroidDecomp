namespace SuperMetroid.Core.Enemies;

/// <summary>
/// A species for the standalone fixture places a check binds an enemy in (#2558): a boss encounter's body, which the
/// encounter holds, or the stand-in whose definition a check reads; each runs nothing of its own.
/// </summary>
internal sealed class StandaloneFixtureSpecies : IEnemyEntityFactory
{
    /// <summary>The registry of standalone places a check binds an enemy in.</summary>
    internal static EnemySpeciesRegistry Registry { get; } = PrivateState.New<EnemySpeciesRegistry>(
        [typeof(EnemyCommonParticipation), typeof(IEnumerable<IEnemyEntityFactory>)],
        new object?[]
        {
            new EnemyCommonParticipation(() => throw new InvalidOperationException("A standalone fixture slot has no enemy system to share reactions with.")),
            (IEnumerable<IEnemyEntityFactory>)[new StandaloneFixtureSpecies()],
        });

    /// <inheritdoc/>
    public IReadOnlyList<EnemyDefinitionId> Definitions { get; } = [EnemyDefinitionId.MotherBrainBody, EnemyDefinitionId.DraygonBody, EnemyPlaceFixtures.StandIn];

    /// <inheritdoc/>
    public IEnemyEntity Create(RoomEnemyOccupant occupant, RoomEnemyPlace place, EnemyCommonParticipation common) =>
        new Body();

    /// <summary>An enemy that runs nothing of its own.</summary>
    private sealed class Body : IEnemyEntity
    {
        /// <inheritdoc/>
        public EnemyCapabilities Capabilities => EnemyCapabilities.None;
    }
}
