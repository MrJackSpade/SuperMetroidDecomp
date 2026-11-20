namespace SuperMetroid.Core.Enemies;

/// <summary>A populated place an enemy holds (#2558).</summary>
/// <param name="occupant">The enemy holding the place.</param>
public sealed class OccupiedEnemyPlace(RoomEnemyOccupant occupant) : EnemyPlaceOccupancy
{
    /// <summary>The enemy holding the place.</summary>
    public RoomEnemyOccupant Occupant { get; } = Ensure.NotNull(occupant);

    /// <inheritdoc/>
    public override RoomEnemyPlacement LastSpawn => Occupant.Spawn;
}
