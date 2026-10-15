using SuperMetroid.Core.Rooms.Plms;

namespace SuperMetroid.Core.Rooms.Doors;

/// <summary>Verification view of one of the three physical eye-door PLMs.</summary>
internal readonly record struct EyeDoorPlmSnapshot(
    PlmHeaderId Header,
    int BlockIndex,
    ushort RoomArgument,
    EyeDoorComponent Component,
    byte HitCounter,
    ushort InstructionPointer,
    ushort PreInstruction);
