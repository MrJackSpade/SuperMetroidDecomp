using SuperMetroid.Core.Rooms.RoomObjects;

namespace SuperMetroid.Core.Enemies.Projectiles;

/// <summary>
/// What a downward gate's actor keeps: the room block of the gate it draws, so the gate can wake it again, and how far it
/// has moved toward its next block.
/// </summary>
/// <param name="roomObjectBlockIndex">The gate's room block, counted row by row.</param>
internal sealed class DownwardGateState(int roomObjectBlockIndex) : EnemyProjectileState
{
    /// <summary>The gate's room block, counted row by row.</summary>
    internal int RoomObjectBlockIndex { get; } = Ensure.AtLeastZero(roomObjectBlockIndex);

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
