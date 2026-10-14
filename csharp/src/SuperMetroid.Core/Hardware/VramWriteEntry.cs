namespace SuperMetroid.Core.Hardware;

/// <summary>
/// One logical form of the packed record consumed by <c>$80:8C83</c>.
/// </summary>
/// <param name="SizeInBytes">DMA byte count written to <c>DAS1</c>.</param>
/// <param name="SourceAddress">24-bit CPU bus address written to <c>A1T1/A1B1</c>.</param>
/// <param name="EncodedVramDestination">
/// Destination word address; bit 15 selects a 32-word rather than one-word increment.
/// </param>
public readonly record struct VramWriteEntry(
    ushort SizeInBytes,
    int SourceAddress,
    ushort EncodedVramDestination)
{
    /// <summary>None preserves the original bus-backed record. Asset transfers resolve against current host content at drain time.</summary>
    public VramAssetId AssetId { get; init; }
}
