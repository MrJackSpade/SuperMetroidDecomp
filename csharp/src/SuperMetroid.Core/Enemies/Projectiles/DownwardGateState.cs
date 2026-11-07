namespace SuperMetroid.Core.Enemies.Projectiles;

/// <summary>What a downward gate's actor keeps: the room block of the gate it draws, so the gate can wake it again.</summary>
/// <param name="roomObjectBlockIndex">The gate's room block, counted row by row.</param>
internal sealed class DownwardGateState(int roomObjectBlockIndex) : EnemyProjectileState
{
    /// <summary>The gate's room block, counted row by row.</summary>
    internal int RoomObjectBlockIndex { get; } = Ensure.AtLeastZero(roomObjectBlockIndex);
}
