using SuperMetroid.Core.Rooms.RoomObjects;

namespace SuperMetroid.Core.Rooms.Doors;

/// <summary>Verification view of a resident colored-door PLM slot.</summary>
internal readonly record struct ColoredDoorObjectSnapshot(
    RoomObjectHeaderId Header,
    Position16<RoomBlocks> Block,
    ushort RoomArgument,
    ColoredDoorColor Color,
    ColoredDoorOrientation Orientation,
    ColoredDoorPhase Phase,
    byte HitCounter);
