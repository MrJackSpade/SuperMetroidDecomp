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
    /// How far the gate has moved since it last advanced a block, in 256ths of a pixel; it advances a block, and starts
    /// again from zero, once this reaches <see cref="DownwardGateDefinitions.OneBlockDistance"/>.
    /// </summary>
    internal int TravelSinceBlock
    {
        get;
        set => field = Ensure.BetweenInclusive(0, DownwardGateDefinitions.OneBlockDistance - 1, value);
    }
}
