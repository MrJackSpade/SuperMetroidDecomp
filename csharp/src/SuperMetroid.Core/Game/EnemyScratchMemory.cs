using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Game;

/// <summary>
/// The enemy staging surfaces in bank-$7E WRAM (#629). Several encounters reuse the same bytes
/// under different meanings; each meaning is its own cell, declared as an alias of the root.
/// </summary>
[CpuBusBoundary("catalog: enemy staging WRAM")]
public static class EnemyScratchWorkRam
{
    /// <summary>
    /// <c>$7E:2000-$7E:2FFF</c> <c>EnemyBG2Tilemap</c>: the enemy BG2 staging image Mother Brain's
    /// body and the extended-tilemap writer ($A0:96CA) fill before it is transferred.
    /// </summary>
    public static readonly CpuBusCell EnemyBg2Tilemap = CpuBusCell.WramWords(nameof(EnemyBg2Tilemap), 0x7e2000, 0x800,
        "BG2 tilemap words (u16[] little-endian)");

    /// <summary>
    /// <c>$7E:2000-$7E:2FFF</c>: the 4bpp corpse tile surface the dead-monster and Dead Torizo
    /// rotting routines ($A9:D308 and the bank-$A9 corpse family) copy and move pixel rows in.
    /// </summary>
    public static readonly CpuBusCell CorpseTiles = CpuBusCell.WramBytes(nameof(CorpseTiles), 0x7e2000, 0x1000,
        "4bpp tiles (u8[])", aliasOf: EnemyBg2Tilemap);

    /// <summary><c>$7E:2000-$7E:2FFF</c>: the Shitroid cutscene's scratch surface, cleared by its initializer.</summary>
    public static readonly CpuBusCell ShitroidScratch = CpuBusCell.WramBytes(nameof(ShitroidScratch), 0x7e2000, 0x1000,
        "scratch bytes (u8[])", aliasOf: EnemyBg2Tilemap);

    /// <summary>
    /// <c>$7E:9000-$7E:97FF</c>: the corpse-rotting heap. Dead-monster rot tables of four-byte
    /// (signed Y, delay) entries live here at native pointers from their corpse definitions.
    /// </summary>
    public static readonly CpuBusCell RottingHeap = CpuBusCell.WramBytes(nameof(RottingHeap), 0x7e9000, 0x800,
        "corpse rot tables ((s16 Y, u16 delay)[] little-endian)");

    /// <summary><c>$7E:9000-$7E:96FF</c>: Mother Brain's six-row corpse tile staging buffer, uploaded by $A9:E1F4.</summary>
    public static readonly CpuBusCell MotherBrainCorpseTiles = CpuBusCell.WramBytes(nameof(MotherBrainCorpseTiles), 0x7e9000, 0x700,
        "4bpp tiles (u8[])", aliasOf: RottingHeap);

    /// <summary><c>$7E:9700-$7E:97BF</c>: Mother Brain's 48-entry rot table.</summary>
    public static readonly CpuBusCell MotherBrainRotTable = CpuBusCell.WramBytes(nameof(MotherBrainRotTable), 0x7e9700,
        MotherBrainCorpseRottingState.EntryCount * CorpseRotTable.EntryByteCount,
        "corpse rot table ((s16 Y, u16 delay)[] little-endian)", aliasOf: RottingHeap);

    /// <summary><c>$7E:9500-$7E:973F</c>: the Dead Torizo sand strips that $A9:D69C rewrites line by line.</summary>
    public static readonly CpuBusCell DeadTorizoSand = CpuBusCell.WramBytes(nameof(DeadTorizoSand), 0x7e9500, 0x240,
        "4bpp tile bitplane words (u8[])", aliasOf: RottingHeap);
}

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

/// <summary>
/// One native corpse rot table: four-byte entries of a signed pixel-row word followed by a
/// delay word, consumed by <c>ProcessCorpseRotting</c> at <c>$A9:DB12</c> (#629).
/// </summary>
public readonly struct CorpseRotTable
{
    /// <summary>Bytes in one (signed Y, delay) entry.</summary>
    public const int EntryByteCount = 4;

    private readonly WorkRamByteTable bytes;
    private readonly int firstByte;

    internal CorpseRotTable(WorkRamByteTable bytes, int firstByte, ushort entryCount)
    {
        if (entryCount == 0)
            throw new ArgumentOutOfRangeException(nameof(entryCount));
        this.bytes = bytes;
        this.firstByte = firstByte;
        EntryCount = entryCount;
    }

    /// <summary>The number of entries processed in ascending index order.</summary>
    public ushort EntryCount { get; }

    /// <summary>Entry <paramref name="entry"/>'s signed pixel-row word.</summary>
    public ushort YOffset(int entry) => bytes.ReadWord(EntryOffset(entry));

    /// <summary>Stores entry <paramref name="entry"/>'s signed pixel-row word, low byte first.</summary>
    public void SetYOffset(int entry, ushort value) => bytes.WriteWord(EntryOffset(entry), value);

    /// <summary>Entry <paramref name="entry"/>'s delay word.</summary>
    public ushort Delay(int entry) => bytes.ReadWord(EntryOffset(entry) + 2);

    /// <summary>Stores entry <paramref name="entry"/>'s delay word, low byte first.</summary>
    public void SetDelay(int entry, ushort value) => bytes.WriteWord(EntryOffset(entry) + 2, value);

    private int EntryOffset(int entry)
    {
        if ((uint)entry >= EntryCount)
            throw new ArgumentOutOfRangeException(nameof(entry), entry, $"Rot table holds {EntryCount} entries.");
        return firstByte + entry * EntryByteCount;
    }
}
