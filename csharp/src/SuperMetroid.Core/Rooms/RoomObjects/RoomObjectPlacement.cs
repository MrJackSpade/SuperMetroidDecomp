namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>
/// One decoded placement and its setup metadata. Program payloads are
/// bounded domain records, not a store that can supply arbitrary values.
/// </summary>
/// <param name="Header">Decoded room-object identity selecting setup and script identities.</param>
/// <param name="Block">Placement in 16-pixel room blocks.</param>
/// <param name="Argument">What the setup reads, decoded at import; null for a room object placed without one.</param>
/// <param name="ScrollProgram">A constructed scroll trigger's ordered scroll writes, the program ending after its last write; null when the placement runs a retail program or is not a scroll trigger. Population construction copies and validates it.</param>
/// <param name="Cannon">The Draygon wall cannon a cannon placement controls, decoded at import; null for every other room object.</param>
public sealed record RoomObjectPlacement(
    RoomObjectHeaderDefinition Header,
    Position16<RoomBlocks> Block,
    RoomObjectRoomArgument? Argument,
    IReadOnlyList<RoomScrollWrite>? ScrollProgram = null,
    DraygonWallCannon? Cannon = null)
{
    /// <summary>The retail scroll program a retail scroll trigger runs; constructed placements supply validated decoded writes instead.</summary>
    internal RoomScrollProgram? CompiledScrollProgram { get; init; }
}
