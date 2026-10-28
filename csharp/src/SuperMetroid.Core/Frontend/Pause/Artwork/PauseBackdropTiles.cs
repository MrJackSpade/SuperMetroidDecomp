using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Progression;
using static SuperMetroid.Core.Frontend.Pause.Artwork.PauseBackdropDefinitions;

namespace SuperMetroid.Core.Frontend.Pause.Artwork;

/// <summary>
/// Draws the pause screen's backdrops (<see cref="PauseBackdropDefinitions"/>): each area's name, the button panel and the frame.
/// </summary>
public static class PauseBackdropTiles
{
    /// <summary>Native area-name semantics; Ceres is displayed as COLONY.</summary>
    private static string AreaName(AreaId area) => area switch
    {
        AreaId.Crateria => "CRATERIA", AreaId.Brinstar => "BRINSTAR",
        AreaId.Norfair => "NORFAIR", AreaId.WreckedShip => "WRECKED SHIP",
        AreaId.Maridia => "MARIDIA", AreaId.Tourian => "TOURIAN", AreaId.Ceres => "COLONY",
        _ => throw new ArgumentOutOfRangeException(nameof(area)),
    };

    /// <summary>Centered text and shared rectangular backdrop geometry, with independent selected differences kept by the presentation.</summary>
    /// <remarks>Chosen frame/control placement, glyph origins and style inputs remain required under the original backdrop entries.</remarks>
    internal static BackgroundTile StockAreaTile(AreaId area, int cell)
    {
        string name = AreaName(area);
        if ((uint)cell >= Cells) throw new ArgumentOutOfRangeException(nameof(cell));
        int letter = cell - LabelCell - (LabelWords - name.Length) / 2;
        if ((uint)(cell - LabelCell) < LabelWords)
            return (uint)letter < name.Length && name[letter] != ' '
                ? new BackgroundTile(LetterA + name[letter] - 'A', LetterPalette, highPriority: true) : BlankTile;
        return StockFrameTile(cell);
    }

    /// <summary>$B6:E400-E7FF is the lower sixteen rows of the same backdrop template.</summary>
    internal static BackgroundTile StockButtonTile(int cell)
    {
        if ((uint)cell >= ButtonCells) throw new ArgumentOutOfRangeException(nameof(cell));
        return StockFrameTile(cell + (Rows - ButtonRows) * Columns);
    }

    /// <summary>Calculates a pause BG2 frame, inset, title, or bottom control cell from its row and column.</summary>
    private static BackgroundTile StockFrameTile(int cell)
    {
        int row = cell / Columns, column = cell % Columns;
        if (row is >= 6 and <= 23)
            return column is 0 or 31 ? Styled(Side, flips: (column == 0 ? TileFlip.Horizontal : TileFlip.None)) : InsetTile;
        if (row == 24)
            return Styled(column is 0 or 31 ? Corner : Bar, flips: (column == 0 ? TileFlip.Horizontal : TileFlip.None) | TileFlip.Vertical);
        if (row == 5)
        {
            if (column is 0 or 31) return Styled(Corner, flips: (column == 0 ? TileFlip.Horizontal : TileFlip.None));
            if (column is 9 or 22) return Styled(TitleEnd, flips: (column == 9 ? TileFlip.Horizontal : TileFlip.None));
            return column is >= 10 and <= 21 ? BlankTile : Styled(Bar);
        }
        if (row is 25 or 26)
        {
            int lower = (row - 25) * ControlAtlasStride;
            if (column is >= 1 and <= 4) return Styled(ShoulderButton + column - 1 + lower);
            if (column is >= 5 and <= 9) return Styled(MapButton + column - 5 + lower);
            if (column is 10 or 21) return Styled(ControlSeparator, flips: (column == 10 ? TileFlip.Horizontal : TileFlip.None));
            if (column is >= 12 and <= 15) return Styled(ExitButton + column - 12 + lower);
            if (column is >= 16 and <= 19)
                return column == 19 && row == 25 ? Styled(StartButton, flips: TileFlip.Horizontal)
                    : Styled(StartButton + column - 16 + lower);
            if (column is >= 22 and <= 26) return Styled(SamusButton + column - 22 + lower);
            if (column is 27 or 30) return Styled(ShoulderButton + column - 27 + lower);
            if (column is 28 or 29) return Styled(RightButtonText + column - 28 + lower);
        }
        return BlankTile;
    }

    /// <summary>Places a frame glyph in the frame's palette with priority, mirrored by <paramref name="flips"/>.</summary>
    private static BackgroundTile Styled(int glyph, TileFlip flips = TileFlip.None) =>
        new(glyph, FramePalette, highPriority: true, flips);
}
