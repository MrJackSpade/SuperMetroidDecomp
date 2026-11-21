namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>
/// Finds the gate projectile spawned for the same block, moves its animation past its rest, and has its next step run on
/// the next frame, without allocating another (#2483).
/// </summary>
/// <param name="RoomObjectBlock">The room block of the gate's top block, where its room object sits.</param>
public sealed partial record DownwardGateProjectileWake(Position16<RoomBlocks> RoomObjectBlock)
    : DownwardGateProjectileRequest(RoomObjectBlock);
