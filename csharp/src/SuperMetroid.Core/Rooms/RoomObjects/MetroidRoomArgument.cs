using SuperMetroid.Core.Progression;

namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>The event a Metroid room's clearance watch marks once the room's enemy deaths reach its kill quota.</summary>
/// <param name="Cleared">The room's cleared event, or null for a watch that observes nothing.</param>
public sealed record MetroidRoomArgument(StoryEvent? Cleared) : RoomObjectRoomArgument
{
    /// <summary>A clearance watch that observes nothing and marks no event.</summary>
    public static MetroidRoomArgument ObservesNothing { get; } = new(Cleared: null);
}
