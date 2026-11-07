using SuperMetroid.Core.Rooms.RoomObjects;

namespace SuperMetroid.Core.Rooms.Doors;

/// <summary>Verification view of one of the three physical eye-door PLMs.</summary>
internal readonly record struct EyeDoorObjectSnapshot(
    RoomObjectHeaderId Header,
    int BlockIndex,
    ushort RoomArgument,
    EyeDoorComponent Component,
    byte HitCounter,
    RoomObjectAnimation? Animation,
    ushort FrameBehavior);
