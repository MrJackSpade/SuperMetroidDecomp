using SuperMetroid.Core.Rooms.Scrolling;

namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>
/// One decoded placement: the kind of room object it places, where, and what its setup reads. Setup payloads are
/// bounded domain records, not a store that can supply arbitrary values.
/// </summary>
/// <param name="Kind">The kind of room object placed, which selects its setup.</param>
/// <param name="Block">Placement in 16-pixel room blocks.</param>
/// <param name="Argument">What the setup reads, decoded at import; null for a room object placed without one.</param>
/// <param name="ScrollChange">A constructed scroll trigger's ordered scroll writes, the change ending after its last write; null when the placement runs a retail scroll change or is not a scroll trigger. Population construction copies and validates it.</param>
/// <param name="Cannon">The Draygon wall cannon a cannon placement controls, decoded at import; null for every other room object.</param>
public sealed record RoomObjectPlacement(
    RoomObjectKind Kind,
    Position16<RoomBlocks> Block,
    RoomObjectRoomArgument? Argument,
    IReadOnlyList<ScreenScrollChange>? ScrollChange = null,
    DraygonWallCannon? Cannon = null)
{
    /// <summary>The retail scroll change a retail scroll trigger runs; constructed placements supply validated decoded writes instead.</summary>
    internal RoomScrollChange? CompiledScrollChange { get; init; }
}
