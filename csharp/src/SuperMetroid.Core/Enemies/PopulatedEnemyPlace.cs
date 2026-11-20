namespace SuperMetroid.Core.Enemies;

/// <summary>
/// The history of a place an enemy has been spawned in (#2558): who holds it now, or that its last occupant left it, and
/// the population entry its latest occupant was spawned from. The place keeps this one history from its first spawn for
/// the rest of its room, as the game keeps a place's spawn entry until the place is populated again, so a death
/// explosion's respawn cue rebuilds whatever entry the place keeps when the cue runs (#1542), double deaths included.
/// </summary>
public sealed class PopulatedEnemyPlace : EnemyPlaceHistory
{
    /// <summary>Starts the history of <paramref name="place"/> with its first occupant.</summary>
    /// <param name="place">The place the history belongs to.</param>
    /// <param name="first">The first enemy spawned there.</param>
    internal PopulatedEnemyPlace(RoomEnemyPlace place, RoomEnemyOccupant first)
    {
        Place = Ensure.NotNull(place);
        Occupancy = new OccupiedEnemyPlace(first);
    }

    /// <summary>The place whose history this is.</summary>
    public RoomEnemyPlace Place { get; }

    /// <summary>Whether an enemy holds the place now, or its last occupant left it.</summary>
    public EnemyPlaceOccupancy Occupancy { get; private set; }

    /// <summary>The population entry the place's latest occupant was spawned from.</summary>
    public RoomEnemyPlacement LastSpawn => Occupancy.LastSpawn;

    /// <summary>
    /// Whether the place holds no enemy but stays taken until the death explosion of the respawning enemy killed in it
    /// rebuilds that enemy (<see cref="RoomEnemyPlace.AwaitsRespawn"/>).
    /// </summary>
    internal bool AwaitsRespawn => Occupancy is AwaitingRespawnEnemyPlace;

    /// <summary>Records that <paramref name="occupant"/> now holds the place.</summary>
    internal void Occupy(RoomEnemyOccupant occupant) => Occupancy = new OccupiedEnemyPlace(occupant);

    /// <summary>Records that the place's occupant left it, keeping its entry, with no respawn pending.</summary>
    internal void Vacate() => Occupancy = new VacatedEnemyPlace(LastSpawn);

    /// <summary>Records that the place, already vacated, waits for its killed enemy's respawn.</summary>
    /// <exception cref="InvalidOperationException">The place still holds an enemy.</exception>
    internal void AwaitRespawn()
    {
        if (Occupancy is OccupiedEnemyPlace held)
            throw new InvalidOperationException($"Enemy place {Place.Ordinal} still holds {held.Occupant.DefinitionId}; only a killed enemy's place awaits its respawn.");
        Occupancy = new AwaitingRespawnEnemyPlace(LastSpawn);
    }
}
