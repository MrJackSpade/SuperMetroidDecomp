namespace SuperMetroid.Core.Enemies;

/// <summary>
/// A species whose population entries tell it <typeparamref name="TSettings"/> (#2554): an entry carrying those
/// settings hands them to the factory as it builds the entry's enemy, which holds them for its life.
/// </summary>
/// <typeparam name="TSettings">The settings the species' entries tell it.</typeparam>
internal interface IEnemySettingsFactory<TSettings> : IEnemySpeciesFactory where TSettings : EnemySpawnSettings
{
    /// <summary>
    /// Builds the enemy for a newly installed occupant of <paramref name="place"/> with its state from the
    /// <paramref name="settings"/> its entry tells it, running its species' initialization as the occupant is set up.
    /// </summary>
    /// <param name="occupant">The occupant just installed, which names its identity and the entry it is spawned from.</param>
    /// <param name="place">The place, already holding the occupant.</param>
    /// <param name="common">The shared participation the entity composes.</param>
    /// <param name="setup">How the occupant comes to be set up: by the population load, spawned, or spawned joining an encounter.</param>
    /// <param name="settings">What the occupant's entry tells its species.</param>
    IEnemyEntity Create(RoomEnemyOccupant occupant, RoomEnemyPlace place, EnemyCommonParticipation common, EnemySetup setup, TSettings settings);

    /// <inheritdoc/>
    void IEnemySpeciesFactory.EnrollIn(EnemySpeciesRegistry registry) => Ensure.NotNull(registry).Add(this);
}
