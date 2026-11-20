namespace SuperMetroid.Core.Enemies;

/// <summary>
/// A species owner some of whose enemies are spawned joining the encounter that spawns them (#2218, #2554): the entity
/// receives the encounter as it is built.
/// </summary>
/// <typeparam name="TEncounter">The encounter the spawned enemy joins.</typeparam>
internal interface IEnemyJoiningFactory<in TEncounter> where TEncounter : class
{
    /// <summary>Builds the entity for an occupant of <paramref name="place"/> spawned joining <paramref name="encounter"/>.</summary>
    /// <param name="occupant">The occupant the entity is created for, which carries its identity and population entry.</param>
    /// <param name="place">The place, already holding the occupant and its live values.</param>
    /// <param name="common">The shared participation the entity composes.</param>
    /// <param name="encounter">The encounter the spawned enemy joins.</param>
    EnemyOccupant CreateJoining(RoomEnemyOccupant occupant, RoomEnemyPlace place, EnemyCommonParticipation common, TEncounter encounter);
}
