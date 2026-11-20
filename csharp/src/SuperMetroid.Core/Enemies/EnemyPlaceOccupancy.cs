namespace SuperMetroid.Core.Enemies;

/// <summary>
/// Whether a populated place holds an enemy (<see cref="OccupiedEnemyPlace"/>) or its last occupant left it
/// (<see cref="VacatedEnemyPlace"/>); either way the place keeps the population entry its latest occupant was spawned from.
/// </summary>
public abstract class EnemyPlaceOccupancy
{
    /// <summary>Limits the occupancies to the occupied and the vacated place's.</summary>
    private protected EnemyPlaceOccupancy()
    {
    }

    /// <summary>The population entry the place's latest occupant was spawned from.</summary>
    public abstract RoomEnemyPlacement LastSpawn { get; }
}
