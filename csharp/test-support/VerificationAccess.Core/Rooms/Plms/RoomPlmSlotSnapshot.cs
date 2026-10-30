namespace SuperMetroid.Core.Rooms.Plms;

/// <summary>Verification view of one occupied PLM slot.</summary>
/// <param name="ProcessingPosition">
/// The slot's place in the table's processing order, zero for the slot the room update reaches first; a spawn into an
/// empty table takes position zero.
/// </param>
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
