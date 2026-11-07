namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>One direction-selected fallback door-closing PLM definition.</summary>
internal readonly record struct DoorClosingObjectDefinition(
    RoomObjectHeaderId Header,
    RoomObjectProgramStart Start);
