using SuperMetroid.Core.Enemies.Corpses;

namespace SuperMetroid.Core.Enemies;

/// <summary>
/// The enemy staging surfaces the cartridge keeps in bank-$7E WRAM, as two owned buffers. Group A
/// (<c>$7E:2000-$2FFF</c>) is the enemy BG2 staging image, which the corpse-rotting routines and the Shitroid cutscene
/// reuse; group B (<c>$7E:9000-$97FF</c>) is the corpse-rotting heap, which Mother Brain's corpse and Dead Torizo's
/// sand reuse. Each meaning is a view of the same buffer, so a write through one is visible through the others, as on
/// the cartridge. The session's enemy binding owns one instance.
/// </summary>
public sealed class EnemyStagingSurfaces : IWorkRamStagingSource
{
    /// <summary>Group A, <c>$7E:2000-$2FFF</c>.</summary>
    internal readonly VramStagingBuffer Bg2 = new(EnemyStagingDefinitions.Bg2ByteCount);

    /// <summary>Group B, <c>$7E:9000-$97FF</c>.</summary>
    internal readonly VramStagingBuffer RottingHeap = new(EnemyStagingDefinitions.RottingHeapByteCount);

    /// <summary>The enemy BG2 staging image Mother Brain's body and the extended-tilemap writer (<c>$A0:96CA</c>) fill.</summary>
    public StagingWordView EnemyBg2Tilemap => Bg2.WordView(0, Bg2.Length / sizeof(ushort));

    /// <summary>The 4bpp corpse tile surface the dead-monster and Dead Torizo rotting routines move pixel rows in.</summary>
    public StagingByteView CorpseTiles => Bg2.ByteView(0, Bg2.Length);

    /// <summary>The Shitroid cutscene's scratch surface, cleared by its initializer.</summary>
    public StagingByteView ShitroidScratch => Bg2.ByteView(0, Bg2.Length);

    /// <summary>Mother Brain's six-row corpse tile staging buffer, uploaded by <c>$A9:E1F4</c>.</summary>
    public StagingByteView MotherBrainCorpseTiles =>
        RottingHeap.ByteView(0, EnemyStagingDefinitions.MotherBrainCorpseTileByteCount);

    /// <summary>The Dead Torizo sand strips that <c>$A9:D69C</c> rewrites line by line.</summary>
    public StagingByteView DeadTorizoSand =>
        RottingHeap.ByteView(EnemyStagingDefinitions.DeadTorizoSandOffset, EnemyStagingDefinitions.DeadTorizoSandByteCount);

    /// <summary>Mother Brain's 48-entry rot table.</summary>
    public CorpseRotTable MotherBrainRotTable =>
        new(RottingHeap.ByteView(0, RottingHeap.Length), EnemyStagingDefinitions.MotherBrainRotTableOffset,
            EnemyStagingDefinitions.MotherBrainRotTableEntryCount);

    /// <summary>
    /// The dead-monster rot table of <paramref name="entryCount"/> entries at bank-$7E pointer
    /// <paramref name="nativePointer"/>, which must lie inside the rotting heap.
    /// </summary>
    public CorpseRotTable RotTable(ushort nativePointer, int entryCount)
    {
        int firstByte = nativePointer - SnesAddress.FromBusAddress(EnemyStagingDefinitions.RottingHeapNativeAddress).Offset;
        if (firstByte < 0 || firstByte + entryCount * CorpseRotTable.EntryByteCount > RottingHeap.Length)
            throw new InvalidDataException($"Corpse rot table $7E:{nativePointer:X4} with {entryCount} entries lies outside the rotting heap.");
        return new(RottingHeap.ByteView(0, RottingHeap.Length), firstByte, entryCount);
    }

    /// <summary>
    /// Queues a native corpse transfer record whose source lies in one of the surfaces; the queue reads the surface when
    /// it drains, as the cartridge's NMI DMA reads WRAM.
    /// </summary>
    internal void EnqueueNativeTransfer(VramWriteQueue queue, VramWriteEntry transfer)
    {
        Ensure.NotNull(queue);
        (VramStagingBuffer buffer, int offset) = Resolve(transfer.SourceAddress, transfer.SizeInBytes);
        queue.EnqueueStaging(transfer.SizeInBytes, transfer.SourceAddress, buffer, offset,
            transfer.Destination);
    }

    /// <summary>The bytes a native transfer whose source lies in one of the surfaces reads now.</summary>
    /// <param name="sourceAddress">The transfer's native WRAM source.</param>
    /// <param name="byteCount">Bytes the transfer moves.</param>
    internal ReadOnlySpan<byte> ReadNativeTransfer(int sourceAddress, int byteCount)
    {
        (VramStagingBuffer buffer, int offset) = Resolve(sourceAddress, byteCount);
        return buffer.Bytes.Slice(offset, byteCount);
    }

    /// <inheritdoc/>
    public bool TryResolve(int nativeAddress, int byteCount, out VramStagingBuffer buffer, out int offset)
    {
        offset = nativeAddress - EnemyStagingDefinitions.Bg2NativeAddress;
        buffer = Bg2;
        if (offset >= 0 && offset + byteCount <= Bg2.Length)
            return true;
        offset = nativeAddress - EnemyStagingDefinitions.RottingHeapNativeAddress;
        buffer = RottingHeap;
        return offset >= 0 && offset + byteCount <= RottingHeap.Length;
    }

    /// <summary>Maps a native bank-$7E source span to the surface and offset that hold it.</summary>
    private StagingLocation Resolve(int sourceAddress, int byteCount) =>
        TryResolve(sourceAddress, byteCount, out VramStagingBuffer buffer, out int offset)
            ? new StagingLocation(buffer, offset)
            : throw new InvalidDataException(
                $"Enemy VRAM transfer ${sourceAddress:X6}+{byteCount} lies outside the enemy staging surfaces.");
}
