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

    /// <summary>The Shitroid cutscene's scratch surface, cleared by its initializer.</summary>
    public StagingByteView ShitroidScratch => Bg2.ByteView(0, Bg2.Length);

    /// <summary>Mother Brain's 48-entry rot table.</summary>
    public CorpseRotTable MotherBrainRotTable =>
        new(RottingHeap.ByteView(0, RottingHeap.Length), EnemyStagingDefinitions.MotherBrainRotTableOffset,
            EnemyStagingDefinitions.MotherBrainRotTableEntryCount);

    /// <summary>
    /// The dead-monster rot table of <paramref name="entryCount"/> entries <paramref name="firstByte"/> bytes into the
    /// rotting heap, which it must lie inside.
    /// </summary>
    public CorpseRotTable RotTable(int firstByte, int entryCount)
    {
        if (firstByte < 0 || firstByte + entryCount * CorpseRotTable.EntryByteCount > RottingHeap.Length)
            throw new InvalidDataException($"Corpse rot table at heap byte {firstByte} with {entryCount} entries lies outside the rotting heap.");
        return new(RottingHeap.ByteView(0, RottingHeap.Length), firstByte, entryCount);
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
}
