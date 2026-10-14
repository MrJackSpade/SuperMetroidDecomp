using SuperMetroid.Core.Bosses.MotherBrain;
using SuperMetroid.Core.Enemies.Corpses;

namespace SuperMetroid.Core.Enemies;

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
