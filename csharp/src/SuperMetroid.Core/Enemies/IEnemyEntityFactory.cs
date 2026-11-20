namespace SuperMetroid.Core.Enemies;

/// <summary>A species whose population entries tell it nothing, building the enemies of its definitions by definition.</summary>
internal interface IEnemyEntityFactory : IEnemySpeciesFactory
{
    /// <summary>
    /// Builds the enemy for a newly installed occupant of <paramref name="place"/> with its state, running its species'
    /// initialization at the point the occupant is set up (#2554).
    /// </summary>
    /// <param name="place">
    /// The place, already holding the new identity and, for an occupant placed from a population entry, the entry it is
    /// spawned from.
    /// </param>
    /// <param name="common">The shared participation the entity composes.</param>
    /// <param name="setup">How the occupant comes to be set up: by the population load, spawned, or spawned joining an encounter.</param>
    IEnemyEntity Create(RoomEnemyPlace place, EnemyCommonParticipation common, EnemySetup setup);

    /// <inheritdoc/>
    void IEnemySpeciesFactory.EnrollIn(EnemySpeciesRegistry registry) => Ensure.NotNull(registry).Add(this);
}
