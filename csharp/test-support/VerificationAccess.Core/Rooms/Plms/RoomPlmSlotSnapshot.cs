namespace SuperMetroid.Core.Rooms.Plms;

/// <summary>Verification view of one occupied PLM slot.</summary>
/// <param name="ProcessingPosition">
/// The slot's place in the table's processing order, zero for the slot the room update reaches first; a spawn into an
/// empty table takes position zero.
/// </param>
/// <param name="HeaderPointer">The PLM's header identity.</param>
/// <param name="BlockIndex">The room block the PLM is placed at.</param>
/// <param name="RoomArgument">The room argument the PLM was placed with.</param>
/// <param name="PreInstruction">The PLM's pre-instruction.</param>
/// <param name="InstructionTimer">Updates left before the PLM's next instruction.</param>
/// <param name="LoopTimer">The PLM program's loop counter.</param>
/// <param name="ProjectileHit">The projectile hit the PLM recorded, if any.</param>
/// <param name="Animation">The PLM's running animation, if any.</param>
internal readonly record struct RoomPlmSlotSnapshot(
    int ProcessingPosition,
    PlmHeaderId HeaderPointer,
    int BlockIndex,
    ushort RoomArgument,
    ushort PreInstruction,
    ushort InstructionTimer,
    ushort LoopTimer,
    ResidentPlmProjectileHit? ProjectileHit,
    PlmAnimation? Animation);
