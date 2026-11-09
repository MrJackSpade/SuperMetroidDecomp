using SuperMetroid.Core.Rooms.RoomObjects;

namespace SuperMetroid.Core.Rooms.Doors;

/// <summary>Verification view of a resident grey-door PLM slot.</summary>
internal readonly record struct GreyDoorObjectSnapshot(
    RoomObjectHeaderId Header,
    int BlockIndex,
    ushort RoomArgument,
    ColoredDoorOrientation Orientation,
    GreyDoorCondition Condition,
    GreyDoorPhase Phase,
    byte HitCounter,
    GreyDoorObject Door);
