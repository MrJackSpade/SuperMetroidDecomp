using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Frontend.Options;
using SuperMetroid.Core.Frontend.Options.Artwork;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Spatial;
using static SuperMetroid.AssetExtraction.Frontend.Options.Artwork.GameOptionsPageDefinitions;

namespace SuperMetroid.AssetExtraction.Frontend.Options.Artwork;

/// <summary>
/// Lays out the options pages' dynamic cells as the cartridge does (<see cref="GameOptionsPageDefinitions"/>, #1467): each
/// controller label's glyph cells and box, the language-label rows and the toggle choice boxes the cartridge recolours.
/// The import cuts each picture of them from the pages; the game never addresses a page cell.
/// </summary>
public static class GameOptionsPageCells
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
    internal static Position16<TilemapCells> ControllerAnchor(int action) => ControllerLabelAnchor(action);

    /// <summary>The page cell of each controller-action label's upper-left cell, action 0..6, each three rows below the last.</summary>
    /// <exception cref="ArgumentOutOfRangeException">The action is outside the seven assignable rows.</exception>
    public static Position16<TilemapCells> ControllerLabelAnchor(int action)
    {
        if ((uint)action >= GameOptionsDefinitions.Rows.ControllerActionCount) throw new ArgumentOutOfRangeException(nameof(action));
        return new(ControllerLabels.AnchorColumn, checked((ushort)(ControllerLabels.FirstAnchorRow + ControllerLabels.AnchorRowSpacing * action)));
    }

    /// <summary>
    /// One of the primary page's four language-label rows: the English label's two rows, then the Japanese label's, which
    /// take the selected palette when Japanese is active.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">The index is outside the four rows.</exception>
    public static GameOptionsLanguagePaletteRegion LanguagePaletteRegion(int index)
    {
        if ((uint)index >= GameOptionsDefinitions.LanguagePaletteRegionCount) throw new ArgumentOutOfRangeException(nameof(index));
        int language = index / 2;
        int row = index % 2;
        int rowBytes = GameOptionsDefinitions.MenuTilemapWidth * sizeof(ushort);
        return new(LanguageRegions.FirstByteOffset + rowBytes * (row + LanguageRegions.LanguageRowSpacing * language),
            language == 0 ? LanguageRegions.EnglishByteCount : LanguageRegions.JapaneseByteCount,
            HighlightWhenJapanese: language != 0);
    }

    /// <summary>$82:EDF2..EE51 language highlight calls cover consecutive tile words in two text rows.</summary>
    internal static IEnumerable<int> LanguageCells(int index)
    {
        GameOptionsLanguagePaletteRegion region = LanguagePaletteRegion(index);
        for (int word = 0; word < region.ByteCount / sizeof(ushort); word++)
            yield return region.ByteOffset / sizeof(ushort) + word;
    }

    /// <summary>The Icon Cancel toggle's choice boxes.</summary>
    public static GameOptionsToggleLayout IconCancel => ToggleLayout(SpecialRowIndex(GameOptionsSpecialRow.IconCancel));

    /// <summary>The Moonwalk toggle's choice boxes, six tilemap rows below Icon Cancel's.</summary>
    public static GameOptionsToggleLayout Moonwalk => ToggleLayout(SpecialRowIndex(GameOptionsSpecialRow.Moonwalk));

    /// <summary>The cursor position of the special page's row <paramref name="row"/>.</summary>
    private static int SpecialRowIndex(GameOptionsSpecialRow row) => Array.IndexOf(GameOptionsLayout.SpecialRows, row);

    /// <summary>
    /// The two-row choice boxes of Icon Cancel (0) or Moonwalk (1): the toggles are six tilemap rows apart and their
    /// choices seven cells apart. Exit has no boxes.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">The setting is not a toggle.</exception>
    public static GameOptionsToggleLayout ToggleLayout(int setting)
    {
        if ((uint)setting >= (uint)SpecialRowIndex(GameOptionsSpecialRow.Exit)) throw new ArgumentOutOfRangeException(nameof(setting));
        int rowStride = GameOptionsDefinitions.MenuTilemapWidth * sizeof(ushort);
        int origin = SpecialToggles.FirstByteOffset + SpecialToggles.SettingRowSpacing * rowStride * setting;
        int choiceStride = SpecialToggles.ChoiceCellSpacing * sizeof(ushort);
        return new(origin, origin + rowStride, origin + choiceStride, origin + rowStride + choiceStride);
    }

    /// <summary>$82:F149..F158 special-setting choice boxes: two rows of six words.</summary>
    internal static IEnumerable<int> ToggleCells(GameOptionsToggle toggle, GameOptionsToggleState state)
    {
        GameOptionsToggleLayout layout = toggle switch
        {
            GameOptionsToggle.IconCancel => IconCancel,
            GameOptionsToggle.Moonwalk => Moonwalk,
            _ => throw new ArgumentOutOfRangeException(nameof(toggle)),
        };
        int first = state == GameOptionsToggleState.Enabled ? layout.EnabledTop : layout.DisabledTop;
        int second = state == GameOptionsToggleState.Enabled ? layout.EnabledBottom : layout.DisabledBottom;
        for (int row = 0; row < 2; row++)
        for (int word = 0; word < SpecialToggles.PaletteRegionByteCount / sizeof(ushort); word++)
            yield return (row == 0 ? first : second) / sizeof(ushort) + word;
    }
}
