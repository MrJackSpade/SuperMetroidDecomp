namespace SuperMetroid.Core.Rooms;

/// <summary>Verification view of a resident grey-door PLM slot.</summary>
internal readonly record struct GreyDoorPlmSnapshot(
    PlmHeaderId Header,
    int BlockIndex,
    ushort RoomArgument,
    ColoredDoorOrientation Orientation,
    GreyDoorCondition Condition,
    GreyDoorPhase Phase,
    ushort InitialList,
    ushort FlashList,
    ushort OpeningList);
