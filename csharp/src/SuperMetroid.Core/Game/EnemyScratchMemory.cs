namespace SuperMetroid.Core.Game;

/// <summary>Typed view of the enemy staging surfaces in WRAM (#629).</summary>
[CpuBusBoundary("domain view: enemy staging WRAM")]
public readonly struct EnemyScratchMemory(ISnesMemoryBus bus)
{
    /// <summary>The enemy BG2 staging image.</summary>
    public WorkRamWordTable EnemyBg2Tilemap => new(bus, EnemyScratchWorkRam.EnemyBg2Tilemap);

    /// <summary>The dead-monster and Dead Torizo corpse tile surface.</summary>
    public WorkRamByteTable CorpseTiles => new(bus, EnemyScratchWorkRam.CorpseTiles);

    /// <summary>The Shitroid cutscene scratch surface.</summary>
    public WorkRamByteTable ShitroidScratch => new(bus, EnemyScratchWorkRam.ShitroidScratch);

    /// <summary>Mother Brain's corpse tile staging buffer.</summary>
    public WorkRamByteTable MotherBrainCorpseTiles => new(bus, EnemyScratchWorkRam.MotherBrainCorpseTiles);

    /// <summary>The Dead Torizo sand strips.</summary>
    public WorkRamByteTable DeadTorizoSand => new(bus, EnemyScratchWorkRam.DeadTorizoSand);

    /// <summary>Mother Brain's rot table.</summary>
    public CorpseRotTable MotherBrainRotTable =>
        new(new WorkRamByteTable(bus, EnemyScratchWorkRam.MotherBrainRotTable), 0, MotherBrainCorpseRottingState.EntryCount);

    /// <summary>
    /// The dead-monster rot table of <paramref name="entryCount"/> entries at bank-$7E pointer
    /// <paramref name="nativePointer"/>, which must lie inside the rotting heap.
    /// </summary>
    public CorpseRotTable RotTable(ushort nativePointer, ushort entryCount)
    {
        int firstByte = nativePointer - SnesAddress.FromBusAddress(EnemyScratchWorkRam.RottingHeap.Address).Offset;
        if (firstByte < 0 || firstByte + entryCount * CorpseRotTable.EntryByteCount > EnemyScratchWorkRam.RottingHeap.ByteCount)
            throw new InvalidDataException($"Corpse rot table $7E:{nativePointer:X4} with {entryCount} entries lies outside the rotting heap.");
        return new(new WorkRamByteTable(bus, EnemyScratchWorkRam.RottingHeap), firstByte, entryCount);
    }
}
