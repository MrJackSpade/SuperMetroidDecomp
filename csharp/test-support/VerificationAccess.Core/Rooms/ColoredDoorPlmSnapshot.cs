namespace SuperMetroid.Core.Rooms;

/// <summary>Verification view of a resident colored-door PLM slot.</summary>
internal readonly record struct ColoredDoorPlmSnapshot(
    PlmHeaderId Header,
    int BlockIndex,
    ushort RoomArgument,
    ColoredDoorColor Color,
    ColoredDoorOrientation Orientation,
    ColoredDoorPhase Phase,
    byte HitCounter);
