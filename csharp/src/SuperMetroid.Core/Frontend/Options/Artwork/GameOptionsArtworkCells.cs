using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Input;
using static SuperMetroid.Core.Frontend.Options.Artwork.GameOptionsPresentationDefinitions;

namespace SuperMetroid.Core.Frontend.Options.Artwork;

/// <summary>
/// Draws and places the options screen's artwork (<see cref="GameOptionsPresentationDefinitions"/>): each controller label's
/// cells, the cursor and heading anchors, and the cells the language and toggle highlights recolour.
/// </summary>
public static class GameOptionsArtworkCells
{
    /// <summary>Builds the native button glyph from its letter, atlas rows and geometric reflections.</summary>
    internal static BackgroundTile ControllerLabelTile(ControllerButton label, int cell)
    {
        if ((uint)cell >= ControllerLabelCellCount) throw new ArgumentOutOfRangeException(nameof(cell));
        int row = cell / ControllerLabelWidth;
        int column = cell % ControllerLabelWidth;
        int tile;
        TileFlip flips = TileFlip.None;
        switch (label)
        {
            case ControllerButton.A:
            case ControllerButton.Y:
                tile = column == 2 ? ButtonPaddingTile : (label == ControllerButton.A ? AButtonTile : YButtonTile) + row * ButtonAtlasRowStride;
                if (column == 1) flips = TileFlip.Horizontal;
                break;
            case ControllerButton.X:
                tile = column == 2 ? ButtonPaddingTile : XButtonTile + (row ^ column) * ButtonAtlasRowStride;
                if (column == 1) flips = TileFlip.Horizontal | TileFlip.Vertical;
                break;
            case ControllerButton.B:
                tile = column == 2 ? ButtonPaddingTile : BButtonTile + column + row * ButtonAtlasRowStride;
                break;
            case ControllerButton.Select:
                tile = SelectButtonTile + column + row * ButtonAtlasRowStride;
                break;
            case ControllerButton.L:
            case ControllerButton.R:
                tile = column == 1 ? (label == ControllerButton.L ? LeftShoulderTile : RightShoulderTile) + row * ButtonAtlasRowStride : ShoulderCornerTile;
                if (column != 1)
                    flips = (row == 1 ? TileFlip.Vertical : TileFlip.None)
                        | (column == 2 ? TileFlip.Horizontal : TileFlip.None);
                break;
            case ControllerButton.None:
            case ControllerButton.Right:
            case ControllerButton.Left:
            case ControllerButton.Down:
            case ControllerButton.Up:
            case ControllerButton.Start:
            default:
                throw new ArgumentOutOfRangeException(nameof(label));
        }
        return new BackgroundTile(tile, 0, highPriority: false, flips);
    }
    /// <summary>$82:F639 controller label boxes: three tile rows per action.</summary>
    internal static Position16<TilemapCells> ControllerAnchor(int action) => GameOptionsPositions.ControllerLabelAnchor(action);

    /// <summary>$82:F307/F31B/F33F selection missile rows, including controller scroll displacement.</summary>
    internal static Position16<SpritePixels> CursorAnchor(GameOptionsPage page, int row)
    {
        if ((uint)row >= GameOptionsNames.CursorCount(page))
            throw new InvalidDataException($"Options cursor {page}[{row}] is not authored.");
        return page switch
        {
            GameOptionsPage.Primary => new(GameOptionsDefinitions.Cursors.PrimaryX, GameOptionsPositions.PrimaryY(row)),
            GameOptionsPage.Controller => new(GameOptionsDefinitions.Cursors.ControllerX, GameOptionsPositions.ControllerY(row)),
            GameOptionsPage.Special => new(GameOptionsDefinitions.Cursors.SpecialX, GameOptionsPositions.SpecialY(row)),
            _ => throw new ArgumentOutOfRangeException(nameof(page)),
        };
    }

    /// <summary>$82:F34B/F353/F35B and F369 named page heading setup anchors.</summary>
    internal static Position16<SpritePixels> HeadingAnchor(GameOptionsPage page) =>
        new(GameOptionsPositions.HeadingX(page), GameOptionsDefinitions.Headings.HeadingY);

    /// <summary>$82:EDF2..EE51 language highlight calls cover consecutive tile words in two text rows.</summary>
    internal static IEnumerable<int> LanguageCells(int index)
    {
        var region = GameOptionsPositions.LanguagePaletteRegion(index);
        for (int word = 0; word < region.ByteCount / sizeof(ushort); word++)
            yield return region.ByteOffset / sizeof(ushort) + word;
    }

    /// <summary>$82:F149..F158 special-setting choice boxes: two rows of six words.</summary>
    internal static IEnumerable<int> ToggleCells(GameOptionsToggle toggle, GameOptionsToggleState state)
    {
        var layout = toggle switch
        {
            GameOptionsToggle.IconCancel => GameOptionsPositions.IconCancel,
            GameOptionsToggle.Moonwalk => GameOptionsPositions.Moonwalk,
            _ => throw new ArgumentOutOfRangeException(nameof(toggle)),
        };
        int first = state == GameOptionsToggleState.Enabled ? layout.EnabledTop : layout.DisabledTop;
        int second = state == GameOptionsToggleState.Enabled ? layout.EnabledBottom : layout.DisabledBottom;
        for (int row = 0; row < 2; row++)
        for (int word = 0; word < GameOptionsDefinitions.SpecialToggles.PaletteRegionByteCount / sizeof(ushort); word++)
            yield return (row == 0 ? first : second) / sizeof(ushort) + word;
    }
}
