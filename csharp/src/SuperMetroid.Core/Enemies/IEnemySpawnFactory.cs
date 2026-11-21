namespace SuperMetroid.Core.Enemies;

/// <summary>
/// A species whose enemies are built the same way wherever they are set up (#2554): placed by their room's population
/// load, rebuilt by a respawn or spawned on another enemy's behalf. Its setup reads nothing of the load's surroundings.
/// </summary>
/// <typeparam name="TSettings">The settings its placements in the room give it.</typeparam>
internal interface IEnemySpawnFactory<in TSettings> : IEnemyEntityFactory where TSettings : EnemySpawnSettings
{
    /// <summary>The definitions whose enemies this factory creates.</summary>
    IReadOnlyList<EnemyDefinitionId> Definitions { get; }

    /// <summary>Builds the entity for an occupant of <paramref name="place"/>.</summary>
    /// <param name="occupant">The occupant the entity is created for, which carries its identity and population entry.</param>
    /// <param name="settings">The settings the occupant's population entry gives its species.</param>
    /// <param name="place">The place, already holding the occupant and its live values.</param>
    /// <param name="common">The shared participation the entity composes.</param>
    EnemyOccupant Create(RoomEnemyOccupant occupant, TSettings settings, RoomEnemyPlace place, EnemyCommonParticipation common);

    /// <inheritdoc/>
    void IEnemyEntityFactory.OfferCreations(EnemySpeciesRegistrar registrar) => Ensure.NotNull(registrar).Spawns<TSettings>(Definitions, Create);
}
