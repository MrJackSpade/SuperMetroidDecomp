namespace SuperMetroid.Core.Rooms.Plms;

/// <summary>Verification view of one occupied physical PLM slot.</summary>
internal readonly record struct RoomPlmSlotSnapshot(
    int NativeSlotIndex,
    PlmHeaderId HeaderPointer,
    int BlockIndex,
    ushort RoomArgument,
    ushort InstructionPointer,
    ushort PreInstruction,
    ushort InstructionTimer,
    ushort LinkInstruction,
    ushort LoopTimer,
    PlmAnimation? Animation);
