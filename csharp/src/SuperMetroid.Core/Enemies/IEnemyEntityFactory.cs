namespace SuperMetroid.Core.Enemies;

/// <summary>A species whose population entries tell it nothing, building the enemies of its definitions by definition.</summary>
internal interface IEnemyEntityFactory : IEnemySpeciesFactory
{
    /// <summary>
    /// Builds the enemy for a newly installed occupant of <paramref name="place"/> with its state, running its species'
    /// initialization at the point the occupant is set up (#2554).
    /// </summary>
    /// <param name="occupant">The occupant just installed, which names its identity and the entry it is spawned from.</param>
    /// <param name="place">The place, already holding the occupant.</param>
    /// <param name="common">The shared participation the entity composes.</param>
    /// <param name="setup">How the occupant comes to be set up: by the population load, spawned, or spawned joining an encounter.</param>
    IEnemyEntity Create(RoomEnemyOccupant occupant, RoomEnemyPlace place, EnemyCommonParticipation common, EnemySetup setup);

    /// <inheritdoc/>
    void IEnemySpeciesFactory.EnrollIn(EnemySpeciesRegistry registry) => Ensure.NotNull(registry).Add(this);
}
