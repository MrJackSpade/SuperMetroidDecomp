namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>One room object a room's population places: what it is, the block it stands on and what its setup reads.</summary>
public readonly record struct PlacedRoomObject(
    Position16<RoomBlocks> Block,
    RoomObjectRoomArgument? Argument);
