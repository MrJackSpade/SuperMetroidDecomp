using SuperMetroid.Core.Frontend.Pause;
using SuperMetroid.Core.Hardware;

namespace SuperMetroid.AssetExtraction.CartridgeTables;

/// <summary>Cartridge regions whose bank-local pointers pause-menu codecs resolve.</summary>
[CpuBusBoundary("catalog: pause menu cartridge tables")]
internal static class PauseMenuCartridgeTables
{
    /// <summary>
    /// $82:8000-$82:FFFF: pause-menu tilemap data. Equipment-label sources and reserve-screen label
    /// pointers are bank-local words resolved in this bank.
    /// </summary>
    internal static readonly CpuBusCell MenuBank = CpuBusCell.CartridgeBytes(nameof(MenuBank), 0x828000, 0x8000,
        "pause tilemap words (u16 bank-local pointers)");

    /// <summary>$82:8EDA copies the mutable button image from $B6:E400 to WRAM $3400.</summary>
    internal const int PauseButtonTilemap = 0xb6e400;

    /// <summary>$82:8EDA LoadPauseScreen_BaseTilemaps copies the $B6:E000 BG2 image.</summary>
    internal const int PauseFrameTilemap = 0xb6e000;

    /// <summary>$82:93C3 loads the twelve-word area label through $82:965F.</summary>
    internal const int PauseAreaLabelPointers = 0x82965f;

    /// <summary>Area-label fragments use the fixed bank $82.</summary>
    internal const int PauseAreaLabelBank = 0x820000;

    /// <summary>$82:C18E equipment-selector positions: tanks, weapons, suit/misc, boots.</summary>
    internal const int PauseSelectorPositionPointers = 0x82c18e;

    /// <summary>$82:C0EC points to selector timing entries, (duration, unused, sprite offset).</summary>
    internal const int PauseSelectorAnimation = 0x82c0ec;

    /// <summary>$82:AB98 initializes the selector timer from the first byte at $82:C10C.</summary>
    internal const int PauseSelectorInitialTimer = 0x82c10c;

    /// <summary>$82:C0DA identifies the low-byte category variable at WRAM $0755.</summary>
    internal const int PauseSelectorVariant = 0x82c0da;

    /// <summary>$7E:0755 PauseMenu_EquipmentScreenCategoryIndex; the animation reads only its low byte.</summary>
    internal const ushort PauseSelectorCategoryVariable = 0x0755;

    /// <summary>$82:C1E8 points to the four category sprite bases.</summary>
    internal const int PauseSelectorBases = 0x82c1e8;

    /// <summary>$82:C100 is the native caller palette word, selecting OBJ palette three.</summary>
    internal const int PauseSelectorPalette = 0x82c100;

    /// <summary>Bank $82 owns selector positions, timing and sprite-pointer data.</summary>
    internal const int PauseSelectorBank = 0x820000;

    /// <summary>$82:C068, the equipment-page places of the MODE/MANUAL and RESERVE TANK labels.</summary>
    internal const int ReserveLabelPlacements = 0x82c068;

    /// <summary>$82:C088, the source pointers of the MODE/MANUAL and RESERVE TANK labels.</summary>
    internal const int ReserveLabelSources = 0x82c088;

    /// <summary>$82:B25F Samus_Wireframe_Tilemaps pointer order: Power, Power/Hi-Jump, Varia, Varia/Hi-Jump.</summary>
    internal const int PauseWireframePointers = 0x82b25f;

    /// <summary>All four native artwork patches share bank $82.</summary>
    internal const int PauseWireframeBank = 0x820000;

    /// <summary>$7E:3800, the equipment page's mutable tilemap, which label and reserve destinations address.</summary>
    internal const int EquipmentTilemapBase = 0x3800;

    /// <summary>$82:C202 bases: the selector spritemap each equipment category binds, Reserve=$14, Beam=$15, Suit and Boots=$16.</summary>
    internal static ushort SelectorSpritemap(PauseEquipmentCategory category) => category switch
    {
        PauseEquipmentCategory.Reserves => 0x14,
        PauseEquipmentCategory.Beams => 0x15,
        PauseEquipmentCategory.Suits or PauseEquipmentCategory.Boots => 0x16,
        _ => throw new InvalidOperationException($"Undefined PauseEquipmentCategory {category}."),
    };
}
