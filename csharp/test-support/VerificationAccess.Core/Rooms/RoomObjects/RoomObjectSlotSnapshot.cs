namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>Verification view of one occupied PLM slot.</summary>
/// <param name="ProcessingPosition">
/// The slot's place in the table's processing order, zero for the slot the room update reaches first; a spawn into an
/// empty table takes position zero.
/// </param>
/// <param name="Block">The room block the PLM is placed at.</param>
/// <param name="RoomArgument">
/// The word native memory holds for the PLM's argument, or null for an object whose record carries no argument, hit count
/// or scroll program, whose word is its placement's filler or its spawner's, which the port does not keep.
/// </param>
/// <param name="ProjectileHit">The projectile hit the PLM recorded, if any.</param>
/// <param name="Behaviour">The typed room object the place holds, if any (#1554); it runs no animation.</param>
/// <param name="SpentCap">Whether the PLM is a door cap opened for good, whose word keeps only its spent mark.</param>
internal readonly record struct RoomObjectSlotSnapshot(
    int ProcessingPosition,
    Position16<RoomBlocks> Block,
    ushort? RoomArgument,
    ResidentObjectProjectileHit? ProjectileHit,
    RoomObjectBehaviour? Behaviour,
    bool SpentCap)
{
    /// <summary>
    /// Whether the place holds a downward gate's shot block: the terrain change a gate trigger's setup leaves running,
    /// which draws the trigger's shot block once before it leaves the room.
    /// </summary>
    internal bool HoldsDownwardGateShotBlock => Behaviour is TerrainChangeObject { Change: var change }
        && DownwardGateShotBlockDefinitions.Triggers.Any(trigger => trigger.Change == change);
}
