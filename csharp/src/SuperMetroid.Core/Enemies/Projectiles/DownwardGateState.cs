using SuperMetroid.Core.Rooms.RoomObjects;

namespace SuperMetroid.Core.Enemies.Projectiles;

/// <summary>
/// What a downward gate's actor keeps: the room block of the gate it draws, so the gate can wake it again, the stage of
/// its travel it stands at, and how far it has moved toward its next block.
/// </summary>
/// <param name="roomObjectBlock">The gate's room block.</param>
/// <param name="stage">The stage of <see cref="DownwardGateProjectileAnimations.Travel"/> the gate starts at.</param>
internal sealed class DownwardGateState(Position16<RoomBlocks> roomObjectBlock, int stage) : EnemyProjectileState
{
    /// <summary>The gate's room block.</summary>
    internal Position16<RoomBlocks> RoomObjectBlock { get; } = roomObjectBlock;

    /// <summary>
    /// The stage of <see cref="DownwardGateProjectileAnimations.Travel"/> the gate stands at; each block it passes, and the
    /// wake that starts a closed gate rising, takes it on to the next.
    /// </summary>
    internal int Stage
    {
        get;
        set => field = Ensure.BetweenInclusive(0, DownwardGateProjectileAnimations.Travel.Count - 1, value);
    } = Ensure.BetweenInclusive(0, DownwardGateProjectileAnimations.Travel.Count - 1, stage);

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
