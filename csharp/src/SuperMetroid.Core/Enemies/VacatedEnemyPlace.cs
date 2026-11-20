namespace SuperMetroid.Core.Enemies;

/// <summary>
/// A populated place whose last occupant left it (#2558), by a deletion or a kill, keeping the population entry that
/// occupant was spawned from, with no respawn pending: the place is free for a spawn.
/// </summary>
/// <param name="lastSpawn">The entry the place's last occupant was spawned from.</param>
public sealed class VacatedEnemyPlace(RoomEnemyPlacement lastSpawn) : EnemyPlaceOccupancy
{
    /// <inheritdoc/>
    public override RoomEnemyPlacement LastSpawn { get; } = Ensure.NotNull(lastSpawn);
}
