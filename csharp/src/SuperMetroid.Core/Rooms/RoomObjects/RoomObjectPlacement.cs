namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>
/// One decoded placement and its setup metadata. Program payloads are
/// bounded domain records, not an address space that can supply arbitrary bytes.
/// </summary>
/// <param name="Header">Decoded PLM header selecting setup and instruction-list identities.</param>
/// <param name="Block">Placement in 16-pixel room blocks, from the six-byte record's two coordinate bytes.</param>
/// <param name="Argument">What the setup reads, decoded at import; null for a PLM placed without one.</param>
/// <param name="ScrollProgram">Constructed scroll index/state byte pairs followed by a high-bit terminator; population construction copies and validates the payload.</param>
/// <param name="Cannon">The Draygon wall cannon a cannon placement controls, decoded at import; null for every other PLM.</param>
public sealed record RoomObjectPlacement(
    RoomObjectHeaderDefinition Header,
    Position16<RoomBlocks> Block,
    RoomObjectRoomArgument? Argument,
    ReadOnlyMemory<byte> ScrollProgram = default,
    DraygonWallCannon? Cannon = null)
{
    /// <summary>The retail scroll program a retail scroll trigger runs; constructed placements supply validated decoded pairs instead.</summary>
    internal RoomScrollProgram? CompiledScrollProgram { get; init; }
}
