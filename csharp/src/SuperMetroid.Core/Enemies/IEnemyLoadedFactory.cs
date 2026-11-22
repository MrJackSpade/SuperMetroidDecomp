namespace SuperMetroid.Core.Enemies;

/// <summary>
/// A species whose enemies only their room's population load places (#2554): their setup reads the load's surroundings,
/// and no spawn, respawn or encounter builds one.
/// </summary>
/// <typeparam name="TSettings">The settings its placements in the room give it.</typeparam>
internal interface IEnemyLoadedFactory<in TSettings> : IEnemyEntityFactory where TSettings : EnemySpawnSettings
{
    /// <summary>The definitions whose enemies this factory creates.</summary>
    IReadOnlyList<EnemyDefinitionId> Definitions { get; }

    /// <summary>Builds the entity for an occupant the room's population load placed in <paramref name="place"/>.</summary>
    /// <param name="occupant">The occupant the entity is created for, which carries its identity and population entry.</param>
    /// <param name="settings">The settings the occupant's population entry gives its species.</param>
    /// <param name="place">The place, already holding the occupant and its live values.</param>
    /// <param name="common">The shared participation the entity composes.</param>
    /// <param name="surroundings">The room, Samus, controller and camera the population load sets the enemy up in.</param>
    EnemyOccupant CreateLoaded(RoomEnemyOccupant occupant, TSettings settings, RoomEnemyPlace place, EnemyCommonParticipation common,
        EnemyPopulationSurroundings surroundings);

    /// <inheritdoc/>
    void IEnemyEntityFactory.OfferCreations(EnemySpeciesRegistrar registrar) => Ensure.NotNull(registrar).Loads(this);
}
