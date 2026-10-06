using SuperMetroid.Core.Frontend;

namespace SuperMetroid.Core.Assets;

/// <summary>Equipment-page base artwork and the compiled footprints owned by live menu state.</summary>
public static class PauseEquipmentBaseDefinitions
{
    public const int Version = 1;
    public const string FileName = "pause-equipment-base.json";
    public const int Columns = 32, Rows = 32, Cells = Columns * Rows;
    /// <summary>$B6:E800, native 32x32 equipment-page template.</summary>
    public const int Source = PauseMenuRomData.EquipmentTilemap;

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
    public static bool IsLiveOwnedCell(int cell)
    {
        if (IsEquipmentLabelCell(cell)) return true;
        if (IsReserveCell(cell)) return true;
        int wireframeRelative = cell - PauseWireframeDefinitions.DestinationByte / sizeof(ushort);
        return wireframeRelative >= 0 && wireframeRelative / (PauseWireframeDefinitions.DestinationStride / sizeof(ushort)) < PauseWireframeDefinitions.Rows &&
            wireframeRelative % (PauseWireframeDefinitions.DestinationStride / sizeof(ushort)) < PauseWireframeDefinitions.Columns;
    }

    public static bool IsNonInventoryLiveOwnedCell(int cell) =>
        IsLiveOwnedCell(cell) && !IsEquipmentLabelCell(cell);

    public static bool IsArrowCell(int cell)
    {
        int vertical = cell - PauseReserveUiDefinitions.VerticalStartCell;
        if (vertical >= 0 && vertical % PauseReserveUiDefinitions.RowStrideCells == 0 &&
            vertical / PauseReserveUiDefinitions.RowStrideCells < PauseReserveUiDefinitions.VerticalCount) return true;
        return (uint)(cell - PauseReserveUiDefinitions.HorizontalStartCell) < PauseReserveUiDefinitions.HorizontalCount;
    }
    /// <summary>$B6:E800 template border artwork: side $140, corner $141, horizontal $142 and title end $143.</summary>
    private const int PanelSide = 0x140, PanelCorner = 0x141, PanelHorizontal = 0x142, PanelTitleEnd = 0x143;
    /// <summary>$B6:E800 template title fragments: SUPPLY $107, BEAM $F9, SUIT $F6, MISC $1B0 and BOOTS $A0.</summary>
    private const int SupplyTitle = 0x107, BeamTitle = 0xf9, SuitTitle = 0xf6, MiscTitle = 0x1b0, BootsTitle = 0xa0;
    /// <summary>$B6:E95A-E965: six-character SAMUS title begins at $13A.</summary>
    private const int SamusTitle = 0x13a;
    /// <summary>$B6:E952-E96D title ornament uses repeated bar $BE and end $BD.</summary>
    private const int TitleBar = 0xbe, TitleBarEnd = 0xbd;
    /// <summary>$B6:E902-EB02: reserve arrow tip/stem/elbow characters $14C/$15C/$16C/$16F.</summary>
    private const int ArrowTip = 0x14c, ArrowStem = 0x15c, ArrowTurn = 0x16c, ArrowJoin = 0x16f;
    /// <summary>$B6:EB06-EB0F: repeated empty reserve-gauge segment $FC and endcap $FE.</summary>
    private const int EmptyGauge = 0xfc, GaugeEnd = 0xfe;

    private enum PanelKind { Supply, Beam, Suit, Misc, Boots }
    private readonly record struct PanelShape(int Left, int Top, int Right, int Bottom,
        int TextColumn, int TextWidth, int TextTile, int TextPalette, bool TextPriority);

    // These chosen panel bounds, text placement and styles remain required layout
    // inputs under PauseEquipmentBasePresentation.tilemap, not an artwork exemption.
    private static PanelShape Panel(PanelKind kind) => kind switch
    {
        PanelKind.Supply => new(2, 9, 11, 13, 5, 4, SupplyTitle, 3, false),
        PanelKind.Beam => new(3, 15, 9, 21, 5, 3, BeamTitle, 6, true),
        PanelKind.Suit => new(20, 8, 30, 11, 24, 3, SuitTitle, 2, true),
        PanelKind.Misc => new(20, 12, 30, 17, 24, 3, MiscTitle, 2, true),
        PanelKind.Boots => new(20, 18, 30, 22, 24, 3, BootsTitle, 3, true),
        _ => throw new ArgumentOutOfRangeException(nameof(kind)),
    };

    /// <summary>Shared stock text, rectangular borders, title bars and reserve-arrow primitives.</summary>
    /// <remarks>Uncovered artwork and selected layout/style inputs remain required;
    /// the presentation stores exact independent differences rather than dropping them.</remarks>
    internal static ushort StockWord(int cell)
    {
        if ((uint)cell >= Cells) throw new IndexOutOfRangeException();
        int row = cell / Columns, column = cell % Columns;
        foreach (var label in PauseEquipmentLabelDefinitions.Labels())
        {
            int first = PauseEquipmentLabelDefinitions.StockDestinationByte(label.Key) / sizeof(ushort);
            int index = cell - first;
            if ((uint)index >= PauseEquipmentLabelDefinitions.StockWordCount(label.Key)) continue;
            ushort word = PauseEquipmentLabelDefinitions.StockWord(label.Key, index);
            bool grayTemplate = label.Category == PauseEquipmentCategories.Boots ||
                (label.Category == PauseEquipmentCategories.Suits && label.Item < 2);
            return grayTemplate ? (ushort)((word & ~0x1c00) | PauseEquipmentLabelDefinitions.DisabledPalette << 10) : word;
        }
        if (row == 12 && column == 2) return Tile(ArrowJoin, 7);
        if (row == 12 && column is >= 3 and <= 6) return Tile(EmptyGauge, 7, false);
        if (row == 12 && column == 7) return Tile(GaugeEnd, 7);
        for (PanelKind kind = PanelKind.Supply; kind <= PanelKind.Boots; kind++)
        {
            PanelShape panel = Panel(kind);
            if (column < panel.Left || column > panel.Right || row < panel.Top || row > panel.Bottom) continue;
            if (row == panel.Top)
            {
                if (column == panel.Left || column == panel.Right)
                    return Tile(PanelCorner, 6, flipX: column == panel.Right);
                if (column >= panel.TextColumn && column < panel.TextColumn + panel.TextWidth)
                    return Tile(panel.TextTile + column - panel.TextColumn, panel.TextPalette, panel.TextPriority);
                if (column == panel.TextColumn - 1 || column == panel.TextColumn + panel.TextWidth)
                    return Tile(PanelTitleEnd, 6, flipX: column > panel.TextColumn);
                return Tile(PanelHorizontal, 6);
            }
            if (row == panel.Bottom)
                return Tile(column == panel.Left || column == panel.Right ? PanelCorner : PanelHorizontal,
                    6, flipX: column == panel.Right, flipY: true);
            if (column == panel.Left || column == panel.Right)
                return Tile(PanelSide, 6, flipX: column == panel.Right);
            if (kind == PanelKind.Supply) return Tile(1, 2);
        }
        if (column == 1 && row is >= 4 and <= 12)
            return row == 4 ? Tile(ArrowTip, 7) : Tile(row == 12 ? ArrowTurn : ArrowStem, 7, false);
        if (row == 4 && column is >= 12 and <= 30) return Tile(0, 2);
        if (row == 5 && column is >= 9 and <= 22)
        {
            if (column is 9 or 10 or 21 or 22) return Tile(TitleBar, 2);
            if (column is 11 or 20) return Tile(TitleBarEnd, 2, flipX: column == 11);
            if (column is 12 or 19) return Tile(1, 2);
            return Tile(SamusTitle + column - 13, 2);
        }
        return 0;
    }

    private static ushort Tile(int character, int palette, bool priority = true, bool flipX = false, bool flipY = false) =>
        (ushort)(character | palette << 10 | (priority ? 0x2000 : 0) | (flipX ? 0x4000 : 0) | (flipY ? 0x8000 : 0));
}
