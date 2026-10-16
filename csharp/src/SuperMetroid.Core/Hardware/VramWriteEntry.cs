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

    /// <summary>Whether the transfer writes zero bytes instead of reading a source.</summary>
    public bool ZeroFill { get; init; }

    /// <summary>
    /// The owned buffer the transfer reads when the queue drains, in place of the WRAM its native
    /// <see cref="SourceAddress"/> names; null for bus, asset and zero-fill transfers.
    /// </summary>
    public VramStagingBuffer? Staging { get; init; }

    /// <summary>The first byte of <see cref="Staging"/> the transfer reads.</summary>
    public int StagingOffset { get; init; }

    /// <summary>
    /// The owned little-endian word table the transfer reads when the queue drains, in place of the WRAM its native
    /// <see cref="SourceAddress"/> names; null for every other transfer.
    /// </summary>
    public ushort[]? StagingWords { get; init; }
}
