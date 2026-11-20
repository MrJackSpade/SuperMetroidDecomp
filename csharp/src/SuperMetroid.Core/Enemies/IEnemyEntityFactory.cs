namespace SuperMetroid.Core.Enemies;

/// <summary>
/// A species owner that builds the entities for the definitions it owns, each with the state it holds from its setup
/// (#2554): the place asks for the entity once its occupant's identity and live values are installed, where the
/// occupant is set up.
/// </summary>
internal interface IEnemyEntityFactory
{
    /// <summary>The definitions whose enemies this factory creates; each definition has exactly one factory.</summary>
    IReadOnlyList<EnemyDefinitionId> Definitions { get; }

    /// <summary>
    /// Builds the entity for an occupant of <paramref name="place"/> spawned on another enemy's behalf or rebuilt by a
    /// respawn, with the setup it has.
    /// </summary>
    /// <param name="occupant">The occupant the entity is created for, which carries its identity and population entry.</param>
    /// <param name="place">The place, already holding the occupant and its live values.</param>
    /// <param name="common">The shared participation the entity composes.</param>
    EnemyOccupant Create(RoomEnemyOccupant occupant, RoomEnemyPlace place, EnemyCommonParticipation common);

    /// <summary>
    /// Builds the entity for an occupant the room's population load placed in <paramref name="place"/>, which sees the
    /// load's <paramref name="surroundings"/>; a species whose setup reads none builds it as a spawn's.
    /// </summary>
    /// <param name="occupant">The occupant the entity is created for, which carries its identity and population entry.</param>
    /// <param name="place">The place, already holding the occupant and its live values.</param>
    /// <param name="common">The shared participation the entity composes.</param>
    /// <param name="surroundings">The room, Samus, controller and camera the population load sets the enemy up in.</param>
    EnemyOccupant CreateLoaded(RoomEnemyOccupant occupant, RoomEnemyPlace place, EnemyCommonParticipation common,
        EnemyPopulationSurroundings surroundings) => Create(occupant, place, common);
}
