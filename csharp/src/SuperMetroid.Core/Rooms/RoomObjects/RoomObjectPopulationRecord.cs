namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>One immutable six-byte record decoded from a room population.</summary>
public readonly record struct RoomObjectPopulationRecord(
    int RecordIndex,
    RoomObjectHeaderId Header,
    Position16<RoomBlocks> Block,
    RoomObjectRoomArgument? Argument);
