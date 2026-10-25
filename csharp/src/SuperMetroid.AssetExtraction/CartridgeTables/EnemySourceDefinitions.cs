using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.Core.Hardware;

namespace SuperMetroid.AssetExtraction.CartridgeTables;

/// <summary>Cartridge regions whose bank-local pointers enemy and enemy-projectile codecs resolve.</summary>
[CpuBusBoundary("catalog: enemy cartridge tables")]
internal static class EnemySourceDefinitions
{
    /// <summary>
    /// $86:8000-$86:FFFF: enemy-projectile instruction lists. A presentation frame's operand address
    /// is the bank-local location of its bank-$8D spritemap pointer word.
    /// </summary>
    internal static readonly CpuBusCell ProjectileInstructionBank = CpuBusCell.CartridgeBytes(
        nameof(ProjectileInstructionBank), 0x868000, 0x8000, "enemy-projectile instruction lists (u16 bank-local operands)");

    /// <summary>
    /// $A6:8000-$A6:FFFF: Ridley's bank. Norfair Ridley's reveal source pointers address RGB5 rows
    /// in this bank.
    /// </summary>
    internal static readonly CpuBusCell RidleyBank = CpuBusCell.CartridgeBytes(nameof(RidleyBank), 0xa68000, 0x8000,
        "Ridley BGR555 color rows (u16 little-endian)");

    /// <summary>Fifteen nonzero fixed-bank source pointers and a zero terminator at $A6:A4EB.</summary>
    internal static readonly SnesAddress RevealSourcePointers = new(0xa6, 0xa4eb);

    /// <summary>
    /// $94:C800: the first of the gunship's five consecutive takeoff character pages, the sources the takeoff list at
    /// $A2:AC07 names, each <see cref="GunshipLiftoffTransferDefinitions.ByteCount"/> bytes long (#1412).
    /// </summary>
    internal static readonly SnesAddress GunshipLiftoffFirstPage = new(0x94, 0xc800);

    /// <summary>The cartridge source of the gunship's takeoff page <paramref name="index"/>.</summary>
    internal static int GunshipLiftoffPageSource(int index) =>
        GunshipLiftoffFirstPage.Value + index * GunshipLiftoffTransferDefinitions.ByteCount;

    /// <summary>$FFFE: the word an extended spritemap component starts with when it is a BG2 command stream (#1412).</summary>
    internal const ushort Bg2StreamMarker = 0xfffe;

    /// <summary>$7E:2000: the work-RAM enemy BG2 tilemap a BG2 command stream's destinations address (#1412).</summary>
    internal const int Bg2WorkingTilemap = 0x2000;

    /// <summary>
    /// Where each auxiliary colour family's rows begin (#1412): the face-block cycle at $A8:E7CC, the dead sidehopper's
    /// stages at $A9:EBCC, whose sixteen-word rows supply fifteen colours, and the Golden Torizo body and belly health
    /// bands at $84:8032 and $84:8132.
    /// </summary>
    internal static readonly IReadOnlyDictionary<EnemyAuxiliaryPalette, EnemyAuxiliaryPaletteSource> AuxiliaryPalettes =
        new Dictionary<EnemyAuxiliaryPalette, EnemyAuxiliaryPaletteSource>
        {
            [EnemyAuxiliaryPalette.FaceBlock] = new(new(0xa8, 0xe7cc), 4),
            [EnemyAuxiliaryPalette.DeadSidehopper] = new(new(0xa9, 0xebcc), 16),
            [EnemyAuxiliaryPalette.GoldenTorizoBody] = new(new(0x84, 0x8032), 16),
            [EnemyAuxiliaryPalette.GoldenTorizoBelly] = new(new(0x84, 0x8132), 16),
        };
}
