namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>Verification view of one occupied PLM slot.</summary>
/// <param name="ProcessingPosition">
/// The slot's place in the table's processing order, zero for the slot the room update reaches first; a spawn into an
/// empty table takes position zero.
/// </param>
/// <param name="Kind">The room object's kind.</param>
/// <param name="Block">The room block the PLM is placed at.</param>
/// <param name="RoomArgument">The room argument the PLM was placed with.</param>
/// <param name="ProjectileHit">The projectile hit the PLM recorded, if any.</param>
/// <param name="Behaviour">The typed room object the place holds, if any (#1554); it runs no animation.</param>
/// <param name="SpentCap">Whether the PLM is a door cap opened for good, whose word keeps only its spent mark.</param>
internal readonly record struct RoomObjectSlotSnapshot(
    int ProcessingPosition,
    RoomObjectKind Kind,
    Position16<RoomBlocks> Block,
    ushort RoomArgument,
    ResidentObjectProjectileHit? ProjectileHit,
    RoomObjectBehaviour? Behaviour,
    bool SpentCap);
