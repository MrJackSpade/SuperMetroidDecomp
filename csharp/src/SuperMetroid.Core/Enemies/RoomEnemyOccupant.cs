namespace SuperMetroid.Core.Enemies;

/// <summary>
/// The enemy holding a room enemy place (#2558): its identity, the population entry it was spawned from and the history
/// of the place it was spawned in, which creates it. A place holds its occupant from the binding that installs it until
/// the occupant leaves; a place that holds no enemy holds no occupant, so no identity and no population entry stands for
/// an empty place. Only an occupant is killed, so a kill always names the enemy it acts on.
/// </summary>
public sealed class RoomEnemyOccupant
{
    /// <summary>Creates the occupant <paramref name="history"/>'s place is populated with from <paramref name="spawn"/>.</summary>
    /// <param name="history">The history of the place the occupant is spawned in.</param>
    /// <param name="spawn">The population entry the occupant is spawned from, which names its enemy.</param>
    /// <param name="definition">The occupant's resolved definition.</param>
    internal RoomEnemyOccupant(PopulatedEnemyPlace history, RoomEnemyPlacement spawn, RoomEnemyDefinition definition)
    {
        History = Ensure.NotNull(history);
        Spawn = Ensure.NotNull(spawn);
        Definition = definition;
    }

    /// <summary>The occupant's enemy definition, which the game re-reads each time the enemy runs.</summary>
    public EnemyDefinitionId DefinitionId => Spawn.DefinitionId;

    /// <summary>
    /// The occupant's resolved definition (#2558), supplying its initialization, collision and AI values: the occupant's
    /// own, so a place that holds no enemy holds no definition.
    /// </summary>
    public RoomEnemyDefinition Definition { get; }

    /// <summary>
    /// The population entry the occupant was spawned from, kept unchanged while its live state moves on; a respawn
    /// rebuilds the enemy from it.
    /// </summary>
    public RoomEnemyPlacement Spawn { get; }

    /// <summary>The history of the place the occupant was spawned in, which keeps it as the place's latest occupant.</summary>
    public PopulatedEnemyPlace History { get; }

    /// <summary>The place the occupant was spawned in.</summary>
    public RoomEnemyPlace Place => History.Place;

    /// <summary>The settings the occupant's population entry gives its species, as the <typeparamref name="T"/> it reads.</summary>
    /// <exception cref="InvalidDataException">The entry carries other settings.</exception>
    internal T SpawnSettings<T>() where T : EnemySpawnSettings => Spawn.SpawnSettingsOf<T>();

    /// <summary>
    /// Empties the occupant's place when the occupant is killed (<see cref="RoomEnemyPlace.Clear"/>) and, for a respawning
    /// enemy, keeps it taken until its death explosion rebuilds it (<see cref="RoomEnemyPlace.AwaitsRespawn"/>). An
    /// occupant already killed is killed again when a second death reaches it in the same update, emptying its place again.
    /// </summary>
    /// <param name="respawns">Whether the killed enemy respawns from the place's entry.</param>
    /// <returns>The place's history, which keeps the entry a respawn rebuilds the enemy from.</returns>
    internal PopulatedEnemyPlace Kill(bool respawns)
    {
        Place.Clear();
        if (respawns)
            History.AwaitRespawn();
        return History;
    }
}
