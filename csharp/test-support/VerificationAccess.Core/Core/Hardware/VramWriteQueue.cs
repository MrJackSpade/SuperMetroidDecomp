using SuperMetroid.Core.Assets;
using SuperMetroid.AssetExtraction.Video;
using SuperMetroid.AssetExtraction.Cartridge;
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
    internal readonly List<VramWriteEntry> Entries = [];

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

        Append(new VramWriteEntry(sizeInBytes, sourceAddress, destination));
    }

    /// <summary>Queues background character artwork named by its cartridge source; the drain binds it (#1350).</summary>
    public void EnqueueBackground(ushort sizeInBytes, int sourceAddress, VramDmaDestination destination)
    {
        if (sizeInBytes == 0)
            throw new ArgumentOutOfRangeException(nameof(sizeInBytes), "Zero is reserved for the queue terminator.");
        if ((uint)sourceAddress > SnesBusLayout.MaximumAddress)
            throw new ArgumentOutOfRangeException(nameof(sourceAddress), sourceAddress, "Source must be a 24-bit SNES CPU address.");
        Append(new VramWriteEntry(sizeInBytes, sourceAddress, destination) { Background = true });
    }

    /// <summary>Queues an installed background artwork asset; the drain binds it (#1350).</summary>
    public void EnqueueBackgroundAsset(InstalledArtworkId asset, ushort sizeInBytes, VramDmaDestination destination)
    {
        if (!Enum.IsDefined(asset)) throw new ArgumentOutOfRangeException(nameof(asset));
        if (sizeInBytes == 0) throw new ArgumentOutOfRangeException(nameof(sizeInBytes));
        Append(new VramWriteEntry(sizeInBytes, 0, destination) { AssetId = asset, Background = true });
    }

    /// <summary>Queues named owned background artwork; the drain binds it (#1350).</summary>
    public void EnqueueBackgroundSource(SpriteArtworkSource source, ushort sizeInBytes, VramDmaDestination destination)
    {
        Ensure.NotNull(source);
        if (sizeInBytes == 0) throw new ArgumentOutOfRangeException(nameof(sizeInBytes));
        Append(new VramWriteEntry(sizeInBytes, 0, destination) { OwnedSource = source, Background = true });
    }

    /// <summary>Queues a typed installed resource without copying artwork into emulated/debugger state.</summary>
    public void EnqueueAsset(InstalledArtworkId asset, ushort sizeInBytes, VramDmaDestination destination)
    {
        if (!Enum.IsDefined(asset)) throw new ArgumentOutOfRangeException(nameof(asset));
        if (sizeInBytes == 0) throw new ArgumentOutOfRangeException(nameof(sizeInBytes));
        Append(new VramWriteEntry(sizeInBytes, 0, destination) { AssetId = asset });
    }

    /// <summary>Adds one packed-size record only when the two-byte NMI terminator still fits.</summary>
    internal void Append(VramWriteEntry entry)
    {
        int newTail = TailInBytes + EntryByteCount;
        if (newTail + TerminatorByteCount > StorageByteCount)
        {
            throw new InvalidOperationException(
                $"VRAM write queue overflow: entry would place the tail at ${newTail:X3}, " +
                $"leaving no two-byte terminator inside the ${StorageByteCount:X3}-byte table.");
        }

        Entries.Add(entry);
        TailInBytes = newTail;
    }

    /// <summary>Whether a transfer of <paramref name="asset"/> is still waiting for the next NMI.</summary>
    public bool Queues(InstalledArtworkId asset)
    {
        foreach (VramWriteEntry entry in Entries)
            if (entry.AssetId == asset)
                return true;
        return false;
    }

    /// <summary>Rebinds a known legacy pending artwork transfer without changing its position or byte count.</summary>
    [PpuLayoutAccess]
    public void RebindBusSource(int sourceAddress, ushort sizeInBytes, InstalledArtworkId asset)
    {
        if (!Enum.IsDefined(asset)) throw new ArgumentOutOfRangeException(nameof(asset));
        for (int i = 0; i < Entries.Count; i++)
            if (Entries[i].AssetId == default && Entries[i].SourceAddress == sourceAddress && Entries[i].SizeInBytes == sizeInBytes)
                Entries[i] = Entries[i] with { SourceAddress = 0, AssetId = asset };
    }

    /// <summary>
    /// Executes every queued transfer in insertion order, then clears the table just as
    /// the NMI consumer does at <c>$80:8CC9</c>. Every native-source record names cartridge artwork, which the
    /// installed artwork must resolve; a record it does not resolve fails rather than reading memory.
    /// </summary>
    /// <param name="vram">VRAM receiving the transfers.</param>
    /// <param name="assets">Resolves installed artwork transfers and named owned sources.</param>
    /// <param name="objects">Records what each transfer that reaches the OBJ character table shows, when gameplay draws from it.</param>
    /// <param name="backgrounds">Binds the queued background character artwork (#1350); required when any is queued.</param>
    public void DrainTo(SnesVram vram, IInstalledArtworkContent? assets = null, SpriteArtworkPlacements? objects = null,
        BgArtworkBindings? backgrounds = null)
    {
        Ensure.NotNull(vram);

        // Use an index rather than foreach to make append-during-drain behavior explicit:
        // this object is single-threaded like the original main-loop/NMI handshake.
        for (int index = 0; index < Entries.Count; index++)
        {
            VramWriteEntry entry = Entries[index];
            if (entry.OwnedSource is { } source)
            {
                if (assets is not IInstalledArtworkContent resolver)
                    throw new InvalidOperationException($"Queued upload of {source} has no bound artwork resolver.");
                ReadOnlyMemory<byte> data = source.Resolve(resolver);
                if (data.Length != entry.SizeInBytes)
                    throw new InvalidDataException($"{source} has {data.Length} bytes; queued transfer requires {entry.SizeInBytes}.");
                if (entry.Background)
                    BindBackground(backgrounds, entry, new BgArtworkIdentity(source.ToString() ?? source.GetType().Name), data.Span);
                else
                {
                    vram.ExecuteQueuedAssetWrite(data.Span, entry.Destination.Encoded);
                    objects?.RecordUpload(entry.Destination, entry.SizeInBytes, source);
                }
            }
            else if (entry.Staging is { } staging)
                WriteStaged(vram, objects, entry, staging.Bytes.Slice(entry.StagingOffset, entry.SizeInBytes));
            else if (entry.StagingWords is { } words)
                WriteStaged(vram, objects, entry, LittleEndianBytes(words, entry.SizeInBytes));
            else if (entry.AssetId == default)
            {
                if (assets is not IInstalledArtworkTransferSource artwork ||
                    !artwork.TryResolve(entry.SourceAddress, entry.SizeInBytes, out ReadOnlyMemory<byte> selected))
                    throw new InvalidOperationException(
                        $"Queued VRAM transfer of {entry.SizeInBytes} bytes from ${entry.SourceAddress:X6} has no installed artwork.");
                if (entry.Background)
                    BindBackground(backgrounds, entry, BgArtworkIdentityConstants.Transfer(entry.SourceAddress), selected.Span);
                else
                {
                    vram.ExecuteQueuedAssetWrite(selected.Span, entry.Destination.Encoded);
                    objects?.RecordUpload(entry.Destination, entry.SizeInBytes,
                        new QueuedTransferCharacters(entry.SourceAddress, entry.SizeInBytes));
                }
            }
            else
            {
                if (assets is null) throw new InvalidOperationException($"Queued VRAM asset {entry.AssetId} has no bound provider.");
                ReadOnlyMemory<byte> data = assets.Resolve(entry.AssetId);
                if (data.Length != entry.SizeInBytes)
                    throw new InvalidDataException($"VRAM asset {entry.AssetId} has {data.Length} bytes; queued transfer requires {entry.SizeInBytes}.");
                if (entry.Background)
                    BindBackground(backgrounds, entry, BgArtworkIdentity.Asset(entry.AssetId), data.Span);
                else
                {
                    vram.ExecuteQueuedAssetWrite(data.Span, entry.Destination.Encoded);
                    objects?.RecordUpload(entry.Destination, entry.SizeInBytes, new InstalledArtworkCharacters(entry.AssetId));
                }
            }
        }

        // The assembly always zeroes the tail, even when the queue was empty. Clearing the
        // typed records at the same point prevents stale transfers on the next frame.
        Entries.Clear();
        TailInBytes = 0;
    }

    /// <summary>Binds a queued background artwork transfer at its destination; it moves consecutive words.</summary>
    private static void BindBackground(BgArtworkBindings? backgrounds, VramWriteEntry entry, BgArtworkIdentity identity, ReadOnlySpan<byte> bytes)
    {
        if (backgrounds is null)
            throw new InvalidOperationException($"Queued background artwork {identity.Name} has no bindings to drain into.");
        if (entry.Destination.WordIncrement != 1)
            throw new InvalidDataException($"Background artwork {identity.Name} must upload consecutive words.");
        BgCharacterWords.Place(identity, entry.Destination.Word, bytes.ToArray()).BindTo(backgrounds);
    }

    /// <summary>
    /// Writes staged bytes and records them where they reach the OBJ character table: a staged surface keeps changing,
    /// so the table shows the bytes the upload read.
    /// </summary>
    private static void WriteStaged(SnesVram vram, SpriteArtworkPlacements? objects, VramWriteEntry entry, ReadOnlySpan<byte> bytes)
    {
        vram.ExecuteQueuedAssetWrite(bytes, entry.Destination.Encoded);
        objects?.RecordUpload(entry.Destination, entry.SizeInBytes, new StagedObjCharacters(bytes));
    }

    /// <summary>The first <paramref name="byteCount"/> bytes of <paramref name="words"/>, low byte first.</summary>
    private static byte[] LittleEndianBytes(ushort[] words, int byteCount)
    {
        var bytes = new byte[byteCount];
        for (int index = 0; index < byteCount; index += sizeof(ushort))
        {
            bytes[index] = LittleEndianBytes.LowByte(words[index / sizeof(ushort)]);
            bytes[index + 1] = LittleEndianBytes.HighByte(words[index / sizeof(ushort)]);
        }
        return bytes;
    }
}
