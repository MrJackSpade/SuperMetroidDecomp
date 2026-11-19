namespace SuperMetroid.Core.Enemies;

/// <summary>
/// A probe of the terrain an enemy's movers take away from the enemy itself (#2572): a body of the enemy's size standing
/// at an offset from it, which a mover moves in the enemy's stead. What the enemy keeps of the probe's movement is the
/// enemy's own choice (<see cref="ProbeMovementKept"/>).
/// </summary>
/// <param name="enemy">The enemy the probe is taken for, whose size it has.</param>
/// <param name="offset">How far from the enemy, in whole pixels, the probe stands.</param>
internal sealed class EnemyTerrainProbe(IEnemyTerrainBody enemy, Offset16<RoomPixels> offset) : IEnemyTerrainBody
{
    /// <inheritdoc/>
    public FixedPosition<RoomPixels> Position { get; set; } = Ensure.NotNull(enemy).Position + offset;

    /// <inheritdoc/>
    public Extent16<RoomPixels> Radius { get; } = enemy.Radius;
}
