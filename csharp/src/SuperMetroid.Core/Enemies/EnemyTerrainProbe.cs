namespace SuperMetroid.Core.Enemies;

/// <summary>
/// A probe of the terrain an enemy's movers take away from the enemy itself (#2572): a body of the enemy's size standing
/// at an offset from it, which a mover moves in the enemy's stead. What the enemy keeps of the probe's movement is the
/// enemy's own choice (<see cref="ProbeMovementKept"/>).
/// </summary>
internal sealed partial class EnemyTerrainProbe : IEnemyTerrainBody
{
    /// <summary>Creates an <see cref="EnemyTerrainProbe"/>.</summary>
    /// <param name="enemy">The enemy the probe is taken for, whose size it has.</param>
    /// <param name="offset">How far from the enemy, in whole pixels, the probe stands.</param>
    public EnemyTerrainProbe(IEnemyTerrainBody enemy, Offset16<RoomPixels> offset)
    {
        this.Position = Ensure.NotNull(enemy).Position + offset;
        this.Radius = enemy.Radius;
    }

    /// <inheritdoc/>
    public FixedPosition<RoomPixels> Position { get; set; }

    /// <inheritdoc/>
    public Extent16<RoomPixels> Radius { get; }
}
