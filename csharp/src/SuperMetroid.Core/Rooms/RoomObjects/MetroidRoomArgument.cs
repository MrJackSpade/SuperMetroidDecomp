using SuperMetroid.Core.Progression;

namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>The event a Metroid room's clearance watch marks once the room's enemy deaths reach its kill quota.</summary>
/// <param name="Cleared">The room's cleared event.</param>
public sealed record MetroidRoomArgument(StoryEvent Cleared) : RoomObjectRoomArgument;
