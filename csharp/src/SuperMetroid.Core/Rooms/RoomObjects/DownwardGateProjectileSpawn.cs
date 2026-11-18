namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>
/// Places a gate projectile at the gate's block, as a gate starts or sets up closed (#2483); a full pool places none.
/// </summary>
/// <param name="Kind">The moving or initially closed gate projectile the spawn allocates.</param>
/// <param name="RoomObjectBlock">The room block of the gate's top block, where its room object sits.</param>
public sealed record DownwardGateProjectileSpawn(DownwardGateProjectileKind Kind, Position16<RoomBlocks> RoomObjectBlock)
    : DownwardGateProjectileRequest(RoomObjectBlock);
