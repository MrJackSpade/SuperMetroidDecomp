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

    /// <summary>$A6:E2AA: Ceres Ridley's sixteen three-colour eye-fade rows (#1412).</summary>
    internal const int CeresRidleyEyeFadeRows = 0xa6e2aa;

    /// <summary>$A6:E1CF: Norfair Ridley's thirty-two initial body and tail colours (#1412).</summary>
    internal const int NorfairRidleyInitialColors = 0xa6e1cf;

    /// <summary>$A7:86C7: Kraid's sixteen room-background target colours (#1412).</summary>
    internal const int KraidRoomBackgroundColors = 0xa786c7;

    /// <summary>$A7:B3D3: Kraid's flash and eight health bands (#1412).</summary>
    internal const int KraidHealthColors = 0xa7b3d3;

    /// <summary>$A7:B513: Kraid's secondary flash and eight health bands (#1412).</summary>
    internal const int KraidSecondaryColors = 0xa7b513;

    /// <summary>$A7:B4F3: Kraid's sixteen death-arm colours (#1412).</summary>
    internal const int KraidDeathArmColors = 0xa7b4f3;

    /// <summary>$AA:D765: the Tourian entrance statue's sixteen colours (#1412).</summary>
    internal const int TourianStatueColors = 0xaad765;

    /// <summary>$AA:D785: the Tourian entrance statue base decoration's sixteen colours (#1412).</summary>
    internal const int TourianStatueBaseColors = 0xaad785;

    /// <summary>$86:B91E: the Tourian statue's four four-colour eye glows, Phantoon, Ridley, Draygon, Kraid (#1415).</summary>
    internal const int TourianStatueEyeColors = 0x86b91e;

    /// <summary>$87:839C: the eight grey colours a released Tourian statue turns (#1415).</summary>
    internal const int TourianStatueGreyColors = 0x87839c;

    /// <summary>$A8:CCC1: the Work Robot's six four-colour, timed palette-cycle records (#1412).</summary>
    internal const int WorkRobotPaletteCycleRecords = 0xa8ccc1;

    /// <summary>Bank $A0, which holds the 64-byte enemy headers (#1412).</summary>
    internal const int EnemyHeaderBank = 0xa00000;

    /// <summary>Bank $B4, which holds the enemy graphics-set records (#1412).</summary>
    internal const int GraphicsSetBank = 0xb40000;

    /// <summary>The low fifteen bits of an enemy header's tile word: the bytes of its tile set (#1412).</summary>
    internal const int EnemyHeaderTileByteCountBits = 0x7fff;

    /// <summary>Bit 15 of an enemy header's tile word: its graphics set shows the tiles at the record's fixed block (#1412).</summary>
    internal const int EnemyHeaderFixedBlockBit = 0x8000;

    /// <summary>$7E:2000: the work-memory enemy BG2 staging image, whose corpse rows a rot upload may read (#1412).</summary>
    internal const int EnemyBg2Staging = 0x7e2000;

    /// <summary>$7E:9000: the work-memory corpse-rotting heap (#1412).</summary>
    internal const int CorpseRottingHeap = 0x7e9000;

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
