namespace SuperMetroid.Core.Enemies;

/// <summary>A species owner that creates the entities for the definitions it owns.</summary>
internal interface IEnemyEntityFactory
{
    /// <summary>The definitions whose enemies this factory creates; each definition has exactly one factory.</summary>
    IReadOnlyList<EnemyDefinitionId> Definitions { get; }

    /// <summary>Creates the entity for a newly installed occupant of <paramref name="place"/>.</summary>
    /// <param name="occupant">The occupant the entity is created for, which carries its identity.</param>
    /// <param name="place">
    /// The place, already holding the new identity and, for an occupant placed from a population entry, the entry it is
    /// spawned from.
    /// </param>
    /// <param name="common">The shared participation the entity composes.</param>
    IEnemyEntity Create(RoomEnemyOccupant occupant, RoomEnemyPlace place, EnemyCommonParticipation common);
}
