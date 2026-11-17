using SuperMetroid.Core.Rooms.Scrolling;

namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>
/// One decoded placement: the kind of room object it places, where, and what its setup reads. Setup payloads are
/// bounded domain records, not a store that can supply arbitrary values.
/// </summary>
/// <param name="Kind">The kind of room object placed, which selects its setup.</param>
/// <param name="Block">Placement in 16-pixel room blocks.</param>
/// <param name="Argument">What the setup reads, decoded at import; null for a room object placed without one.</param>
/// <param name="Cannon">The Draygon wall cannon a cannon placement controls, decoded at import; null for every other room object.</param>
public sealed record RoomObjectPlacement(
    RoomObjectKind Kind,
    Position16<RoomBlocks> Block,
    RoomObjectRoomArgument? Argument,
    DraygonWallCannon? Cannon = null)
{
    /// <summary>The scroll change a scroll trigger runs; null for every other room object (#2355).</summary>
    internal RoomScrollChange? ScrollChange { get; init; }

    /// <summary>Whether <see cref="Block"/> is a block of the room's grid or of the terrain beyond the room, decoded at import.</summary>
    public RoomObjectSite Site { get; init; }
}
