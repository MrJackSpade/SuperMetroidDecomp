using SuperMetroid.Core.Rooms.RoomObjects;

namespace SuperMetroid.Core.Enemies.Projectiles;

/// <summary>
/// What a downward gate's actor keeps: the room block of the gate it draws, so the gate can wake it again, the stage of
/// its travel it stands at, and how far it has moved toward its next block.
/// </summary>
/// <param name="roomObjectBlock">The gate's room block.</param>
/// <param name="stage">The stage of its travel the gate starts at (#2284).</param>
internal sealed class DownwardGateState(Position16<RoomBlocks> roomObjectBlock, DownwardGateProjectileSegment stage) : EnemyProjectileState
{
    /// <summary>The gate's room block.</summary>
    internal Position16<RoomBlocks> RoomObjectBlock { get; } = roomObjectBlock;

    /// <summary>
    /// The stage of its travel the gate stands at, named by the animation that started it; each block it passes, and the
    /// wake that starts a closed gate rising, takes it on to <see cref="DownwardGateProjectileAnimations.NextStage"/>.
    /// </summary>
    internal DownwardGateProjectileSegment Stage { get; set; } = stage;

    /// <summary>
    /// The gate's travel to its next block, counted once for each whole pixel it moves (#2047): it reaches the next block,
    /// and starts the travel again, once it has moved a block's pixels (<see cref="RoomGridConversions.BlockPixels"/>).
    /// </summary>
    internal UpdateCountdown BlockTravel { get; } = UpdateCountdown.Lasting(RoomGridConversions.BlockPixels);
}
