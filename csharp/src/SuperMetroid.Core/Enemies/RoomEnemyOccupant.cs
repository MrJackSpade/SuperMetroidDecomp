namespace SuperMetroid.Core.Enemies;

/// <summary>
/// The enemy holding a room enemy place (#2558): its identity and the population entry it was spawned from. A place
/// holds its occupant from the binding that installs it until the occupant leaves; a place that holds no enemy holds no
/// occupant, so no identity and no population entry stands for an empty place.
/// </summary>
/// <param name="spawn">The population entry the occupant is spawned from, which names its enemy.</param>
public sealed class RoomEnemyOccupant(RoomEnemyPlacement spawn)
{
    /// <summary>The occupant's enemy definition, which the game re-reads each time the enemy runs.</summary>
    public EnemyDefinitionId DefinitionId => Spawn.DefinitionId;

    /// <summary>
    /// The population entry the occupant was spawned from, kept unchanged while its live state moves on; a respawn
    /// rebuilds the enemy from it.
    /// </summary>
    public RoomEnemyPlacement Spawn { get; } = Ensure.NotNull(spawn);

    /// <summary>The settings the occupant's population entry gives its species, as the <typeparamref name="T"/> it reads.</summary>
    /// <exception cref="InvalidDataException">The entry carries other settings.</exception>
    internal T SpawnSettings<T>() where T : EnemySpawnSettings => Spawn.SpawnSettingsOf<T>();
}
