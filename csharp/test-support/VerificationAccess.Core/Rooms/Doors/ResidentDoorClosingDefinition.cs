using SuperMetroid.Core.Rooms.RoomObjects;

namespace SuperMetroid.Core.Rooms.Doors;

/// <summary>One resident door header and the closing instruction list it selects.</summary>
internal readonly record struct ResidentDoorClosingDefinition(RoomObjectHeaderId Header, RoomObjectProgramStart Closing);
