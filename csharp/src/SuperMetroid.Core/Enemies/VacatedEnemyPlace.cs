namespace SuperMetroid.Core.Enemies;

/// <summary>
/// A populated place whose last occupant left it (#2558), by a deletion or a kill, keeping the population entry that
/// occupant was spawned from.
/// </summary>
/// <param name="lastSpawn">The entry the place's last occupant was spawned from.</param>
/// <param name="awaitsRespawn">Whether the place waits for its killed enemy's respawn.</param>
public sealed class VacatedEnemyPlace(RoomEnemyPlacement lastSpawn, bool awaitsRespawn) : EnemyPlaceOccupancy
{
    /// <inheritdoc/>
    public override RoomEnemyPlacement LastSpawn { get; } = Ensure.NotNull(lastSpawn);

    /// <summary>
    /// Whether the place stays taken until the death explosion of the respawning enemy killed in it rebuilds that enemy
    /// from <see cref="LastSpawn"/> (#1851).
    /// </summary>
    public bool AwaitsRespawn { get; } = awaitsRespawn;
}
