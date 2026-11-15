using SuperMetroid.Core.Rooms.RoomObjects;

namespace SuperMetroid.Core.Enemies.Projectiles;

/// <summary>
/// What a downward gate's actor keeps: the room block of the gate it draws, so the gate can wake it again, and how far it
/// has moved toward its next block.
/// </summary>
/// <param name="roomObjectBlock">The gate's room block.</param>
internal sealed class DownwardGateState(Position16<RoomBlocks> roomObjectBlock) : EnemyProjectileState
{
    /// <summary>The gate's room block.</summary>
    internal Position16<RoomBlocks> RoomObjectBlock { get; } = roomObjectBlock;

    /// <summary>
    /// How many whole pixels the gate has moved since it last reached a block (#2047); it reaches the next block, and starts
    /// again from zero, once it has moved <see cref="DownwardGateDefinitions.PixelsPerRoomBlock"/>.
    /// </summary>
    internal int PixelsSinceBlock
    {
        get;
        set => field = Ensure.BetweenInclusive(0, DownwardGateDefinitions.PixelsPerRoomBlock - 1, value);
    }
}
