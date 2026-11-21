namespace SuperMetroid.Core.Enemies;

/// <summary>
/// A species some of whose enemies are spawned joining the encounter that spawns them (#2218, #2554): the spawner
/// hands the spawn this factory, the settings and the encounter (<see cref="JoiningEnemySpawn{TEncounter, TSettings}"/>),
/// and the entity receives the encounter as it is built. No population load or respawn builds such an enemy.
/// </summary>
/// <typeparam name="TEncounter">The encounter the spawned enemy joins.</typeparam>
/// <typeparam name="TSettings">The settings the spawn gives the species.</typeparam>
internal interface IEnemyJoiningFactory<in TEncounter, in TSettings> where TEncounter : class where TSettings : EnemySpawnSettings
{
    /// <summary>Builds the entity for an occupant of <paramref name="place"/> spawned joining <paramref name="encounter"/>.</summary>
    /// <param name="occupant">The occupant the entity is created for, which carries its identity and population entry.</param>
    /// <param name="settings">The settings the spawn gives the species.</param>
    /// <param name="place">The place, already holding the occupant and its live values.</param>
    /// <param name="common">The shared participation the entity composes.</param>
    /// <param name="encounter">The encounter the spawned enemy joins.</param>
    EnemyOccupant CreateJoining(RoomEnemyOccupant occupant, TSettings settings, RoomEnemyPlace place, EnemyCommonParticipation common, TEncounter encounter);
}
