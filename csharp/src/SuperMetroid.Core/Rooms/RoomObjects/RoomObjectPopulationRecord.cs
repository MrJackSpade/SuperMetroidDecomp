namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>One room object a room's population places: its place in the population, what it is, the block it stands on and what its setup reads.</summary>
public readonly record struct RoomObjectPopulationRecord(
    int RecordIndex,
    RoomObjectKind Kind,
    Position16<RoomBlocks> Block,
    RoomObjectRoomArgument? Argument);
