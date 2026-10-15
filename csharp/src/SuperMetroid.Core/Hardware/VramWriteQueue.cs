namespace SuperMetroid.Core.Hardware;

/// <summary>
/// Debuggable representation of Super Metroid's ordinary VRAM write table at WRAM
/// <c>$00D0-$02CF</c>, consumed during NMI by routine <c>$80:8C83</c>.
/// </summary>
/// <remarks>
/// The ROM stores tightly packed seven-byte records:
/// <code>
/// +0  u16 transfer byte count (zero terminates the table)
/// +2  u16 source offset
/// +4  u8  source bank
/// +5  u16 encoded VRAM word destination
/// </code>
/// This port uses typed records but preserves the byte-counted tail because hundreds of
/// original call sites advance that tail by seven. Seeing <see cref="TailInBytes"/> in the
/// debugger therefore remains directly comparable to WRAM <c>$0330</c>.
/// </remarks>
[PpuLayoutBoundary("backend: queued VRAM DMA entries and their native encoding")]
public sealed class VramWriteQueue
{
    /// <summary>Bytes reserved by the original WRAM layout before the Mode 7 table.</summary>
    [PpuLayoutAccess]
    public const int StorageByteCount = 512;

    /// <summary>Physical size of one packed queue entry.</summary>
    [PpuLayoutAccess]
    public const int EntryByteCount = 7;

    /// <summary>NMI writes a zero-size terminator at the tail, so every accepted record must leave two free bytes.</summary>
    private const int TerminatorByteCount = 2;

    /// <summary>Typed records in native insertion order, drained with append-during-drain behavior.</summary>
    internal readonly List<VramWriteEntry> _entries = [];

    /// <summary>
    /// Byte offset at which the ROM would write the next record, corresponding to WRAM
    /// <c>$0330</c>. It advances by seven rather than by one.
    /// </summary>
    [PpuLayoutAccess]
    public int TailInBytes { get; private set; }

    /// <summary>
    /// Appends the typed equivalent of one packed seven-byte record.
    /// </summary>
    public void Enqueue(ushort sizeInBytes, int sourceAddress, VramDmaDestination destination)
    {
        if (sizeInBytes == 0)
            throw new ArgumentOutOfRangeException(nameof(sizeInBytes), "Zero is reserved for the queue terminator.");
        if ((uint)sourceAddress > SnesBusLayout.MaximumAddress)
            throw new ArgumentOutOfRangeException(nameof(sourceAddress), sourceAddress, "Source must be a 24-bit SNES CPU address.");

        Append(new VramWriteEntry(sizeInBytes, sourceAddress, destination.Encoded));
    }

    /// <summary>Queues a typed installed resource without copying artwork into emulated/debugger state.</summary>
    public void EnqueueAsset(VramAssetId asset, ushort sizeInBytes, VramDmaDestination destination)
    {
        if (asset == VramAssetId.None || !Enum.IsDefined(asset)) throw new ArgumentOutOfRangeException(nameof(asset));
        if (sizeInBytes == 0) throw new ArgumentOutOfRangeException(nameof(sizeInBytes));
        Append(new VramWriteEntry(sizeInBytes, 0, destination.Encoded) { AssetId = asset });
    }

    /// <summary>Adds one packed-size record only when the two-byte NMI terminator still fits.</summary>
    private void Append(VramWriteEntry entry)
    {
        int newTail = TailInBytes + EntryByteCount;
        if (newTail + TerminatorByteCount > StorageByteCount)
        {
            throw new InvalidOperationException(
                $"VRAM write queue overflow: entry would place the tail at ${newTail:X3}, " +
                $"leaving no two-byte terminator inside the ${StorageByteCount:X3}-byte table.");
        }

        _entries.Add(entry);
        TailInBytes = newTail;
    }

    /// <summary>Rebinds a known legacy pending artwork transfer without changing its position or byte count.</summary>
    [PpuLayoutAccess]
    public void RebindBusSource(int sourceAddress, ushort sizeInBytes, VramAssetId asset)
    {
        if (asset == VramAssetId.None || !Enum.IsDefined(asset)) throw new ArgumentOutOfRangeException(nameof(asset));
        for (int i = 0; i < _entries.Count; i++)
            if (_entries[i].AssetId == VramAssetId.None && _entries[i].SourceAddress == sourceAddress && _entries[i].SizeInBytes == sizeInBytes)
                _entries[i] = _entries[i] with { SourceAddress = 0, AssetId = asset };
    }

    /// <summary>
    /// Executes every queued transfer in insertion order, then clears the table just as
    /// the NMI consumer does at <c>$80:8CC9</c>.
    /// </summary>
    public void DrainTo(SnesVram vram, ISnesMutableMemory memory, IVramAssetProvider? assets = null)
    {
        Ensure.NotNull(vram);
        Ensure.NotNull(memory);

        // Use an index rather than foreach to make append-during-drain behavior explicit:
        // this object is single-threaded like the original main-loop/NMI handshake.
        for (int index = 0; index < _entries.Count; index++)
        {
            VramWriteEntry entry = _entries[index];
            if (entry.AssetId == VramAssetId.None)
            {
                if (assets is IInstalledArtworkTransferSource artwork &&
                    artwork.TryResolve(entry.SourceAddress, entry.SizeInBytes,
                        out ReadOnlyMemory<byte> selected))
                    vram.ExecuteQueuedAssetWrite(selected.Span, entry.EncodedVramDestination);
                else
                    vram.ExecuteQueuedMemoryWrite(memory, entry.SourceAddress, entry.SizeInBytes,
                        entry.EncodedVramDestination);
            }
            else
            {
                if (assets is null) throw new InvalidOperationException($"Queued VRAM asset {entry.AssetId} has no bound provider.");
                ReadOnlyMemory<byte> data = assets.Resolve(entry.AssetId);
                if (data.Length != entry.SizeInBytes)
                    throw new InvalidDataException($"VRAM asset {entry.AssetId} has {data.Length} bytes; queued transfer requires {entry.SizeInBytes}.");
                vram.ExecuteQueuedAssetWrite(data.Span, entry.EncodedVramDestination);
            }
        }

        // The assembly always zeroes the tail, even when the queue was empty. Clearing the
        // typed records at the same point prevents stale transfers on the next frame.
        _entries.Clear();
        TailInBytes = 0;
    }
}
