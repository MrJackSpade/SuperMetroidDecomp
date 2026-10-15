using SuperMetroid.Core.Rooms.Plms;

namespace SuperMetroid.Core.Rooms.Doors;

/// <summary>Verification view of a resident grey-door PLM slot.</summary>
internal readonly record struct GreyDoorPlmSnapshot(
    PlmHeaderId Header,
    int BlockIndex,
    ushort RoomArgument,
    ColoredDoorOrientation Orientation,
    GreyDoorCondition Condition,
    GreyDoorPhase Phase,
    byte HitCounter,
    PlmAnimation? Animation);
