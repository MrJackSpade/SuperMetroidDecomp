using SuperMetroid.AssetExtraction.Video;
namespace SuperMetroid.Core.Hardware;

/// <summary>
/// One logical form of the packed record consumed by <c>$80:8C83</c>.
/// </summary>
/// <param name="SizeInBytes">DMA byte count written to <c>DAS1</c>.</param>
/// <param name="SourceAddress">24-bit CPU bus address written to <c>A1T1/A1B1</c>.</param>
/// <param name="Destination">Where the transfer lands in VRAM, and the increment it writes with.</param>
public readonly record struct VramWriteEntry(
    int SizeInBytes,
    int SourceAddress,
    VramDmaDestination Destination)
{
    /// <summary>The largest byte count the sixteen-bit DMA size register holds.</summary>
    public const int MaximumSizeInBytes = ushort.MaxValue;

    /// <summary>DMA byte count written to <c>DAS1</c>, 0 through <see cref="MaximumSizeInBytes"/>.</summary>
    public int SizeInBytes { get; } = SizeInBytes is >= 0 and <= MaximumSizeInBytes
        ? SizeInBytes
        : throw new ArgumentOutOfRangeException(nameof(SizeInBytes), SizeInBytes, "A queued DMA moves 0 through 65535 bytes.");

    /// <summary>None preserves the original bus-backed record. Asset transfers resolve against current host content at drain time.</summary>
    public VramAssetId AssetId { get; init; }

    /// <summary>Whether the transfer is background character artwork, which the drain binds instead of writing (#1350).</summary>
    public bool Background { get; init; }

    /// <summary>
    /// The owned buffer the transfer reads when the queue drains, in place of the WRAM its native
    /// <see cref="SourceAddress"/> names; null for bus and asset transfers.
    /// </summary>
    public VramStagingBuffer? Staging { get; init; }

    /// <summary>The owned artwork the transfer uploads, resolved when the queue drains; null for every other transfer.</summary>
    public ObjCharacterSource? OwnedSource { get; init; }
    /// <summary>The first byte of <see cref="Staging"/> the transfer reads.</summary>
    public int StagingOffset { get; init; }

    /// <summary>
    /// The owned little-endian word table the transfer reads when the queue drains, in place of the WRAM its native
    /// <see cref="SourceAddress"/> names; null for every other transfer.
    /// </summary>
    public ushort[]? StagingWords { get; init; }
}
