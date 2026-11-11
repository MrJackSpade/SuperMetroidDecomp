using SuperMetroid.AssetExtraction.Hud;

namespace SuperMetroid.Core.Hud;

/// <summary>Cartridge values and helpers of <see cref="HudState"/> that only verification reads.</summary>
internal static class HudStateConstants
{
    /// <summary>Number of eight-pixel cells per HUD row.</summary>
    public const int WidthInTiles = GameplayHudDocumentDefinitions.Columns;
    /// <summary>Number of HUD rows the HUD's values are drawn over, excluding the preceding static row.</summary>
    public const int MutableRowCount = GameplayHudDocumentDefinitions.DrawnRows;
    /// <summary>Ninety-six cells in the three rows the HUD's values are drawn over.</summary>
    public const int MutableTileCount = WidthInTiles * MutableRowCount;
}
