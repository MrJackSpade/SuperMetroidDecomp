namespace SuperMetroid.Core.Enemies;

/// <summary>A species owner that creates the entities for the definitions it owns.</summary>
internal interface IEnemyEntityFactory
{
    /// <summary>The definitions whose enemies this factory creates; each definition has exactly one factory.</summary>
    IReadOnlyList<EnemyDefinitionId> Definitions { get; }

    /// <summary>Creates the entity for a newly installed occupant of <paramref name="slot"/>.</summary>
    /// <param name="slot">The slot, already holding the new identity.</param>
    /// <param name="common">The shared participation the entity composes.</param>
    IEnemyEntity Create(RoomEnemySlot slot, EnemyCommonParticipation common);
}
