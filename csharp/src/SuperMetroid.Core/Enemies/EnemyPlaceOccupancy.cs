namespace SuperMetroid.Core.Enemies;

/// <summary>
/// Whether a populated place holds an enemy (<see cref="OccupiedEnemyPlace"/>), its last occupant left it
/// (<see cref="VacatedEnemyPlace"/>), or its respawning enemy was killed in it (<see cref="AwaitingRespawnEnemyPlace"/>);
/// in every case the place keeps the population entry its latest occupant was spawned from.
/// </summary>
public abstract partial class EnemyPlaceOccupancy
{
    /// <summary>Limits the occupancies to the occupied, the vacated and the awaiting place's.</summary>
    private protected EnemyPlaceOccupancy()
    {
    }

    /// <summary>The population entry the place's latest occupant was spawned from.</summary>
    public abstract RoomEnemyPlacement LastSpawn { get; }
}
