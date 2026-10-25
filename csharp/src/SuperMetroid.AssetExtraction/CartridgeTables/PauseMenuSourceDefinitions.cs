using System.Collections.Frozen;
using SuperMetroid.Core.Frontend.Pause;
using SuperMetroid.Core.Hardware;

namespace SuperMetroid.AssetExtraction.CartridgeTables;

/// <summary>Cartridge regions whose bank-local pointers pause-menu codecs resolve.</summary>
[CpuBusBoundary("catalog: pause menu cartridge tables")]
internal static class PauseMenuSourceDefinitions
{
    /// <summary>
    /// $82:8000-$82:FFFF: pause-menu tilemap data. Equipment-label sources and reserve-screen label
    /// pointers are bank-local words resolved in this bank.
    /// </summary>
    internal static readonly CpuBusCell MenuBank = CpuBusCell.CartridgeBytes(nameof(MenuBank), 0x828000, 0x8000,
        "pause tilemap words (u16 bank-local pointers)");

    /// <summary>$82:8EDA copies the mutable button image from $B6:E400 to WRAM $3400.</summary>
    internal static readonly SnesAddress PauseButtonTilemap = new(0xb6, 0xe400);

    /// <summary>$82:8EDA LoadPauseScreen_BaseTilemaps copies the $B6:E000 BG2 image.</summary>
    internal static readonly SnesAddress PauseFrameTilemap = new(0xb6, 0xe000);

    /// <summary>$82:93C3 loads the twelve-word area label through $82:965F.</summary>
    internal static readonly SnesAddress PauseAreaLabelPointers = new(0x82, 0x965f);

    /// <summary>Area-label fragments use the fixed bank $82.</summary>
    internal static readonly SnesAddress PauseAreaLabelBank = new(0x82, 0x0000);

    /// <summary>$82:C18E equipment-selector positions: tanks, weapons, suit/misc, boots.</summary>
    internal static readonly SnesAddress PauseSelectorPositionPointers = new(0x82, 0xc18e);

    /// <summary>$82:C0EC points to selector timing entries, (duration, unused, sprite offset).</summary>
    internal static readonly SnesAddress PauseSelectorAnimation = new(0x82, 0xc0ec);

    /// <summary>$82:AB98 initializes the selector timer from the first byte at $82:C10C.</summary>
    internal static readonly SnesAddress PauseSelectorInitialTimer = new(0x82, 0xc10c);

    /// <summary>$82:C0DA identifies the low-byte category variable at WRAM $0755.</summary>
    internal static readonly SnesAddress PauseSelectorVariant = new(0x82, 0xc0da);

    /// <summary>$7E:0755 PauseMenu_EquipmentScreenCategoryIndex; the animation reads only its low byte.</summary>
    internal const ushort PauseSelectorCategoryVariable = 0x0755;

    /// <summary>$82:C1E8 points to the four category sprite bases.</summary>
    internal static readonly SnesAddress PauseSelectorBases = new(0x82, 0xc1e8);

    /// <summary>$82:C100 is the native caller palette word, selecting OBJ palette three.</summary>
    internal static readonly SnesAddress PauseSelectorPalette = new(0x82, 0xc100);

    /// <summary>Bank $82 owns selector positions, timing and sprite-pointer data.</summary>
    internal static readonly SnesAddress PauseSelectorBank = new(0x82, 0x0000);

    /// <summary>$82:C068, the equipment-page places of the MODE/MANUAL and RESERVE TANK labels.</summary>
    internal static readonly SnesAddress ReserveLabelPlacements = new(0x82, 0xc068);

    /// <summary>$82:C088, the source pointers of the MODE/MANUAL and RESERVE TANK labels.</summary>
    internal static readonly SnesAddress ReserveLabelSources = new(0x82, 0xc088);

    /// <summary>$82:B25F Samus_Wireframe_Tilemaps pointer order: Power, Power/Hi-Jump, Varia, Varia/Hi-Jump.</summary>
    internal static readonly SnesAddress PauseWireframePointers = new(0x82, 0xb25f);

    /// <summary>All four native artwork patches share bank $82.</summary>
    internal static readonly SnesAddress PauseWireframeBank = new(0x82, 0x0000);

    /// <summary>$7E:3800, the equipment page's mutable tilemap, which label and reserve destinations address.</summary>
    internal const int EquipmentTilemapBase = 0x3800;

    /// <summary>$B6:E800, the equipment page's 32-by-32 template tilemap.</summary>
    internal const int EquipmentPage = 0xb6e800;


    /// <summary>
    /// The equipment-page label tables of each inventory category, named by the category pointer tables at $82:C02C
    /// (label places) and $82:C044 (label tilemaps).
    /// </summary>
    internal static readonly FrozenDictionary<PauseEquipmentCategory, PauseEquipmentLabelTables> EquipmentLabelTables =
        new Dictionary<PauseEquipmentCategory, PauseEquipmentLabelTables>
        {
            [PauseEquipmentCategory.Beams] = new(Places: 0x82c06c, Labels: 0x82c08c),
            [PauseEquipmentCategory.Suits] = new(Places: 0x82c076, Labels: 0x82c096),
            [PauseEquipmentCategory.Boots] = new(Places: 0x82c082, Labels: 0x82c0a2),
        }.ToFrozenDictionary();

    /// <summary>$82:C01A, kEquipmentScreenTilemap_Blank: the bank-$82 word of the nine-cell blank label.</summary>
    internal const ushort BlankEquipmentLabel = 0xc01a;

    /// <summary>$82:C0A8, kHyperBeamWeaponsTilemaps: the beam label pointers Hyper mode shows instead.</summary>
    internal const int HyperBeamLabels = 0x82c0a8;

    /// <summary>$82:C1D6, EquipmentScreen_ReserveTank_Xpositions: the six reserve-gauge columns, the end cap's last.</summary>
    internal const int ReserveTankXPositions = 0x82c1d6;

    /// <summary>$82:C1E2: the reserve gauge's row plus one, which the draw routine decrements.</summary>
    internal const int ReserveTankYPosition = 0x82c1e2;

    /// <summary>$82:B3D9: the two identical eight-entry tables of partly filled tank spritemaps.</summary>
    internal const int ReserveTankPartialFills = 0x82b3d9;

    /// <summary>The partly filled tank spritemaps of both tables at <see cref="ReserveTankPartialFills"/>.</summary>
    internal const int ReserveTankPartialFillCount = 16;

    /// <summary>$82:BF22, the MANUAL label that replaces the mode label's last four cells.</summary>
    internal const int ReserveManualText = 0x82bf22;

    /// <summary>$82:BF2A, the AUTO label that replaces the mode label's last four cells.</summary>
    internal const int ReserveAutoText = 0x82bf2a;

    /// <summary>$82:AD5D, the reserve arrow's first animated colour, one per pulse frame.</summary>
    internal const int ReserveArrowFirstColours = 0x82ad5d;

    /// <summary>$82:AD9D, the reserve arrow's second animated colour, one per pulse frame.</summary>
    internal const int ReserveArrowSecondColours = 0x82ad9d;

    /// <summary>The spritemap each equipment category's selector shows.</summary>
    internal static readonly FrozenDictionary<PauseEquipmentCategory, ushort> SelectorSpritemaps =
        new Dictionary<PauseEquipmentCategory, ushort>
        {
            [PauseEquipmentCategory.Reserves] = 0x14,
            [PauseEquipmentCategory.Beams] = 0x15,
            [PauseEquipmentCategory.Suits] = 0x16,
            [PauseEquipmentCategory.Boots] = 0x16,
        }.ToFrozenDictionary();
}
