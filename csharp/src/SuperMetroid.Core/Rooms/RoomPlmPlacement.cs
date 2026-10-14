namespace SuperMetroid.Core.Rooms;

/// <summary>
/// One decoded placement and its setup metadata. Program/graphics payloads are
/// bounded domain records, not an address space that can supply arbitrary bytes.
/// </summary>
/// <param name="Header">Decoded bank-$84 PLM header selecting setup and instruction-list identities.</param>
/// <param name="Block">Placement in 16-pixel room blocks, from the native six-byte record's two coordinate bytes.</param>
/// <param name="RoomArgument">Native per-placement word interpreted by the selected setup, such as an item/door index or scroll-program identity.</param>
/// <param name="ScrollProgram">Constructed scroll index/state byte pairs followed by a high-bit terminator; population construction copies and validates the payload.</param>
/// <param name="DynamicGraphic">Optional decoded permanent-collectible tiles and palette offsets; population construction validates and copies their payloads.</param>
public sealed record RoomPlmPlacement(
    RoomPlmHeaderDefinition Header,
    Position16<RoomBlocks> Block,
    ushort RoomArgument,
    ReadOnlyMemory<byte> ScrollProgram = default,
    RoomPlmDynamicCollectibleGraphic? DynamicGraphic = null)
{
    /// <summary>Internal retail identity; constructed placements must still supply validated decoded pairs.</summary>
    internal ushort? CompiledScrollSource { get; init; }
}
