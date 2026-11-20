namespace SuperMetroid.Core.Enemies;

/// <summary>
/// A populated place whose respawning enemy was killed in it (#1851, #2558): it holds no enemy but stays taken until the
/// enemy's death explosion rebuilds it from <see cref="LastSpawn"/>.
/// </summary>
/// <param name="lastSpawn">The entry the killed enemy was spawned from.</param>
public sealed class AwaitingRespawnEnemyPlace(RoomEnemyPlacement lastSpawn) : EnemyPlaceOccupancy
{
    /// <inheritdoc/>
    public override RoomEnemyPlacement LastSpawn { get; } = Ensure.NotNull(lastSpawn);
}
