namespace SuperMetroid.Core.Rooms.Plms;

/// <summary>
/// One decoded placement and its setup metadata. Program payloads are
/// bounded domain records, not an address space that can supply arbitrary bytes.
/// </summary>
/// <param name="Header">Decoded bank-$84 PLM header selecting setup and instruction-list identities.</param>
/// <param name="Block">Placement in 16-pixel room blocks, from the native six-byte record's two coordinate bytes.</param>
/// <param name="Argument">What the setup reads, decoded at import; null for a PLM placed without one.</param>
/// <param name="ScrollProgram">Constructed scroll index/state byte pairs followed by a high-bit terminator; population construction copies and validates the payload.</param>
/// <param name="CannonControl">The cannon-control word a Draygon wall cannon reads and writes, decoded at import; null for every other PLM.</param>
public sealed record RoomPlmPlacement(
    RoomPlmHeaderDefinition Header,
    Position16<RoomBlocks> Block,
    PlmRoomArgument? Argument,
    ReadOnlyMemory<byte> ScrollProgram = default,
    DraygonCannonControlWord? CannonControl = null)
{
    /// <summary>The retail scroll program a retail scroll trigger runs; constructed placements supply validated decoded pairs instead.</summary>
    internal RoomScrollProgram? CompiledScrollProgram { get; init; }
}
