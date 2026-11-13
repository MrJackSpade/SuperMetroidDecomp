namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>One compiled room-object population record: its kind, block and room argument.</summary>
/// <param name="Kind">The kind of room object placed.</param>
/// <param name="Block">The block it is placed on.</param>
/// <param name="Argument">The decoded room argument, or null.</param>
/// <param name="Cannon">The Draygon wall cannon a cannon placement controls; null for every other room object.</param>
/// <param name="ScrollProgram">The retail scroll program a scroll trigger runs; null for every other room object.</param>
internal readonly record struct RoomObjectPlacementDefinition(
    RoomObjectKind Kind, Position16<RoomBlocks> Block, RoomObjectRoomArgument? Argument, DraygonWallCannon? Cannon = null,
    RoomScrollProgram? ScrollProgram = null);
