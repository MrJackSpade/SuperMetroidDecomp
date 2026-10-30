using static SuperMetroid.AssetExtraction.Frontend.Pause.Artwork.PauseEquipmentBaseDefinitions;
using SuperMetroid.Core.Frontend.Pause;
using SuperMetroid.Core.Frontend.Pause.Artwork;
using SuperMetroid.Core.Spatial;
using SuperMetroid.AssetExtraction.Artwork;

namespace SuperMetroid.AssetExtraction.Frontend.Pause.Artwork;

/// <summary>
/// Draws the equipment screen's base page (<see cref="PauseEquipmentBaseDefinitions"/>): its panels and which cells the live
/// labels, reserves and arrows own.
/// </summary>
public static class PauseEquipmentBaseTiles
{
    /// <summary>
    /// $82:C06C-$C087 destinations: beams occupy rows16-20, suits/misc rows9-10
    /// and13-16, boots rows19-21. All own nine cells, including the Plasma overrun
    /// into adjacent words used by the native simultaneous-input VAR behavior.
    /// </summary>
    public static bool IsEquipmentLabelCell(int cell)
    {
        if ((uint)cell >= Cells) return false;
        int row = cell / Columns;
        int column = cell % Columns;
        if (row is >= 16 and <= 20 && column is >= 4 and < 13) return true;
        return column is >= 21 and < 30 &&
            (row is 9 or 10 or >= 13 and <= 16 or >= 19 and <= 21);
    }

    /// <summary>$82:C068/$C06A reserve labels occupy seven columns on rows10/11; supply digits use their own anchor.</summary>
    private static bool IsReserveCell(int cell)
    {
        if ((uint)(cell - PauseReserveUiDefinitions.DigitCell) < PauseReserveUiDefinitions.SupplyDigitPlaces)
            return true;
        if ((uint)cell >= Cells) return false;
        return cell / Columns is 10 or 11 && cell % Columns is >= 4 and < 11;
    }
    /// <summary>Returns whether runtime inventory, reserve, or wireframe state owns a tilemap cell.</summary>
    /// <param name="cell">Zero-based cell index in the 32-by-32 equipment tilemap.</param>
    /// <returns><see langword="true"/> when the static base must leave the cell for live menu composition.</returns>
    public static bool IsLiveOwnedCell(int cell)
    {
        if (IsEquipmentLabelCell(cell)) return true;
        if (IsReserveCell(cell)) return true;
        int wireframeRelative = cell - PauseWireframeDefinitions.FirstPageTile;
        return wireframeRelative >= 0 && wireframeRelative / PauseWireframeDefinitions.PageColumns < PauseWireframeDefinitions.Rows &&
            wireframeRelative % PauseWireframeDefinitions.PageColumns < PauseWireframeDefinitions.Columns;
    }

    /// <summary>Returns whether reserve or wireframe composition owns a cell outside the equipment-label footprints.</summary>
    /// <param name="cell">Zero-based cell index in the equipment tilemap.</param>
    /// <returns><see langword="true"/> for a live-owned cell that is not an equipment label.</returns>
    public static bool IsNonInventoryLiveOwnedCell(int cell) =>
        IsLiveOwnedCell(cell) && !IsEquipmentLabelCell(cell);

    /// <summary>Chooses the native frame footprint, title glyphs, and palette for each section.</summary>
    /// <remarks>Title styles and supply padding belong to the authored composition of this page.</remarks>
    private static PanelShape Panel(PanelKind kind) => kind switch
    {
        PanelKind.Supply => SupplyPanel(),
        PanelKind.Beam => InventoryPanel(PauseEquipmentCategory.Beams, 0, 4, 3, BeamTitle, 6),
        PanelKind.Suit => InventoryPanel(PauseEquipmentCategory.Suits, 0, 1, 3, SuitTitle, 2),
        PanelKind.Misc => InventoryPanel(PauseEquipmentCategory.Suits, 2, 5, 3, MiscTitle, 2),
        PanelKind.Boots => InventoryPanel(PauseEquipmentCategory.Boots, 0, 2, 3, BootsTitle, 3),
        _ => throw new ArgumentOutOfRangeException(nameof(kind)),
    };

    /// <summary>Frames the reserve-mode and tank labels with two-cell left padding and centered title.</summary>
    private static PanelShape SupplyPanel()
    {
        int label = PauseReserveUiTiles.StockLabelOffset(PauseReserveLabel.Mode) / sizeof(ushort);
        int last = PauseReserveUiTiles.StockLabelOffset(PauseReserveLabel.ReserveTank) / sizeof(ushort);
        int left = label % Columns - 2, right = label % Columns + PauseReserveUiDefinitions.LabelWords;
        const int titleWidth = 4;
        return new(left, label / Columns - 1, right, last / Columns + 2,
            (left + right - titleWidth + 1) / 2, titleWidth, SupplyTitle, 3, false);
    }
    /// <summary>Derives an equipment frame from its first and last stock labels, leaving a row above and below.</summary>
    private static PanelShape InventoryPanel(PauseEquipmentCategory category, int firstItem, int lastItem,
        int titleWidth, int titleGlyph, int titlePalette)
    {
        int first = PauseEquipmentLabelTiles.StockPageByte(PauseEquipmentLabelTiles.Label(category, firstItem)) / sizeof(ushort);
        int last = PauseEquipmentLabelTiles.StockPageByte(PauseEquipmentLabelTiles.Label(category, lastItem)) / sizeof(ushort);
        int left = first % Columns - 1, right = first % Columns + PauseEquipmentLabelTiles.WordCount(category);
        return new(left, first / Columns - 1, right, last / Columns + 1,
            (left + right - titleWidth + 1) / 2, titleWidth, titleGlyph, titlePalette, true);
    }
    /// <summary>Shared stock text, rectangular borders, title bars and reserve-arrow primitives.</summary>
    /// <remarks>The exact panel/header/arrow/gauge/guide composition and bounded metadata are retained choices;
    /// the presentation stores exact independent differences rather than dropping them.</remarks>
    internal static BackgroundTile StockTile(int cell)
    {
        if ((uint)cell >= Cells) throw new ArgumentOutOfRangeException(nameof(cell));
        int row = cell / Columns, column = cell % Columns;
        foreach (var label in PauseEquipmentLabelTiles.Labels())
        {
            int first = PauseEquipmentLabelTiles.StockPageByte(label.Label) / sizeof(ushort);
            int index = cell - first;
            if ((uint)index >= PauseEquipmentLabelTiles.StockWordCount(label.Label)) continue;
            BackgroundTile tile = PauseEquipmentLabelGlyphs.StockTile(label.Label, index);
            bool grayTemplate = label.Category == PauseEquipmentCategory.Boots ||
                (label.Category == PauseEquipmentCategory.Suits && label.Item < 2);
            return grayTemplate ? tile with { ColourVariant = PauseEquipmentLabelArtworkDefinitions.DisabledPalette } : tile;
        }
        // Copied transparent edge metadata and guide-layer policy are separately accounted
        // composition inputs, not unexplained full-word fallbacks.
        if (column == Columns - 1 && (row == 4 || row is >= 14 and <= 18)) return Tile(0, 2, false);
        PanelShape beam = Panel(PanelKind.Beam);
        int diagonalColumn = column - beam.Right, diagonalRow = row - beam.Top;
        if (diagonalColumn is 1 or 2 && diagonalRow == 2 - diagonalColumn)
            return Tile(GuideDiagonal, diagonalColumn == 1 ? 6 : 2);
        if (diagonalColumn is 1 or 2 && diagonalRow == 3 - diagonalColumn)
            return Tile(GuideDiagonal + AtlasRowStride, diagonalColumn == 1 ? 6 : 2);
        if (column == beam.Right && row == beam.Top + 2) return Tile(GuideBorderJunction, 6);
        foreach (PanelKind kind in GuidedPanels)
        {
            PanelShape panel = Panel(kind);
            int guideRow = kind == PanelKind.Boots ? panel.Bottom - 1 : panel.Top + 2;
            if (column == panel.Left && row == guideRow)
                return Tile(GuideBorderJunction, 6, flips: TileFlip.Horizontal | (kind == PanelKind.Boots ? TileFlip.Vertical : TileFlip.None));
        }
        if (row == 12 && column == 2) return Tile(ArrowJoin, 7);
        if (row == 12 && column is >= 3 and <= 6) return Tile(EmptyGauge, 7, false);
        if (row == 12 && column == 7) return Tile(GaugeEnd, 7);
        foreach (PanelKind kind in Enum.GetValues<PanelKind>())
        {
            PanelShape panel = Panel(kind);
            if (column < panel.Left || column > panel.Right || row < panel.Top || row > panel.Bottom) continue;
            if (row == panel.Top)
            {
                if (column == panel.Left || column == panel.Right)
                    return Tile(PanelCorner, 6, flips: (column == panel.Right ? TileFlip.Horizontal : TileFlip.None));
                if (column >= panel.TextColumn && column < panel.TextColumn + panel.TextWidth)
                    return Tile(panel.TextTile + column - panel.TextColumn, panel.TextPalette, panel.TextPriority);
                if (column == panel.TextColumn - 1 || column == panel.TextColumn + panel.TextWidth)
                    return Tile(PanelTitleEnd, 6, flips: (column > panel.TextColumn ? TileFlip.Horizontal : TileFlip.None));
                return Tile(PanelHorizontal, 6);
            }
            if (row == panel.Bottom)
                return Tile(column == panel.Left || column == panel.Right ? PanelCorner : PanelHorizontal,
                    6, flips: (column == panel.Right ? TileFlip.Horizontal : TileFlip.None) | TileFlip.Vertical);
            if (column == panel.Left || column == panel.Right)
                return Tile(PanelSide, 6, flips: (column == panel.Right ? TileFlip.Horizontal : TileFlip.None));
            if (kind == PanelKind.Supply) return Tile(1, 2);
        }
        if (column == 1 && row is >= 4 and <= 12)
            return row == 4 ? Tile(ArrowTip, 7) : Tile(row == 12 ? ArrowTurn : ArrowStem, 7, row == 5);
        if (row == 4 && column is >= 12 and <= 30) return Tile(0, 2);
        if (row == 5 && column is >= 9 and <= 22)
        {
            if (column is 9 or 10 or 21 or 22) return Tile(TitleBar, 2);
            if (column is 11 or 20) return Tile(TitleBarEnd, 2, flips: (column == 11 ? TileFlip.Horizontal : TileFlip.None));
            if (column is 12 or 19) return Tile(1, 2);
            return Tile(SamusTitle + column - 13, 2);
        }
        return default;
    }

    /// <summary>Places a glyph in <paramref name="palette"/>, prioritised and mirrored as given.</summary>
    private static BackgroundTile Tile(int character, int palette, bool priority = true, TileFlip flips = TileFlip.None) =>
        new(character, palette, priority, flips);
}
