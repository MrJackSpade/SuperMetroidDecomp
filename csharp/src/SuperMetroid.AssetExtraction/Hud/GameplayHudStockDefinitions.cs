using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Spatial;

namespace SuperMetroid.AssetExtraction.Hud;

/// <summary>
/// The cartridge's HUD tiles and layout that its HUD routines hold as immediates rather than tables (#1466): the stock
/// values the import writes into the editable HUD document. The game reads only the decoded document.
/// </summary>
internal static class GameplayHudStockDefinitions
{
    /// <summary>Revision 2 of the editable HUD schema, including the separately supplied static top row and named dynamic visual owners.</summary>
    internal const int Version = 2;

    /// <summary>The canonical blank HUD tile used by native icon guards and MANUAL clearing: HUD glyph $0F in colour variant three, in the upper priority tier.</summary>
    internal static BackgroundTile Blank { get; } = new(0x0f, 3, highPriority: true);

    /// <summary>The filled energy-tank tile written by <c>$80:9BCF</c>.</summary>
    internal static BackgroundTile FilledEnergyTank { get; } = new(0x31, 2, highPriority: true);

    /// <summary>The empty energy-tank tile written by <c>$80:9BCF</c>.</summary>
    internal static BackgroundTile EmptyEnergyTank { get; } = new(0x30, 5, highPriority: true);

    /// <summary>BG palette selector 4 used by $80:9BD3..9C20 to highlight the newly selected HUD item.</summary>
    internal const int SelectedPalette = 4;

    /// <summary>BG palette selector 5 restored to the previous HUD item by $80:9BD3..9C20; tile number, priority and flips are preserved.</summary>
    internal const int DeselectedPalette = 5;

    /// <summary>$8C: byte offset of the energy digits' anchor in the HUD tilemap.</summary>
    internal const int HealthAnchorByteOffset = 0x8c;

    /// <summary>$94: byte offset of the missile count's anchor in the HUD tilemap.</summary>
    internal const int MissileAnchorByteOffset = 0x94;

    /// <summary>$9C: byte offset of the super missile count's anchor in the HUD tilemap.</summary>
    internal const int SuperMissileAnchorByteOffset = 0x9c;

    /// <summary>$A2: byte offset of the power bomb count's anchor in the HUD tilemap.</summary>
    internal const int PowerBombAnchorByteOffset = 0xa2;

    /// <summary>The minimap's upper-left cell: <c>$90:A91B</c> writes its five columns from word 26 of each of the three rows.</summary>
    internal static Position16<TilemapCells> MinimapAnchor { get; } = new(26, 0);

    /// <summary>Each icon's width in cells, in Missile, Super Missile, Power Bomb, Grapple and X-Ray order: the missile has a central shaft.</summary>
    internal static readonly int[] IconWidths = [3, 2, 2, 2, 2];
}
