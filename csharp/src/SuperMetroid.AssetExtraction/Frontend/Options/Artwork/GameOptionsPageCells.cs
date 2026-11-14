using SuperMetroid.Core.Frontend.Options;
using SuperMetroid.Core.Spatial;
using SuperMetroid.AssetExtraction.Artwork;
using static SuperMetroid.AssetExtraction.Frontend.Options.Artwork.GameOptionsPageDefinitions;

namespace SuperMetroid.AssetExtraction.Frontend.Options.Artwork;

/// <summary>
/// Lays out the options pages' dynamic cells as the cartridge does (<see cref="GameOptionsPageDefinitions"/>, #1467): each
/// controller label's box, the language-label rows and the toggle choice boxes the cartridge recolours, which the import
/// writes into the installed presentation. The game never addresses a page cell.
/// </summary>
public static class GameOptionsPageCells
{
    /// <summary>The page cell of each controller-action label's upper-left cell, action 0..6, each three rows below the last.</summary>
    /// <exception cref="ArgumentOutOfRangeException">The action is outside the seven assignable rows.</exception>
    public static Position16<CharacterCells> ControllerLabelAnchor(int action)
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
        if ((uint)index >= LanguageRegions.RowCount) throw new ArgumentOutOfRangeException(nameof(index));
        int language = index / 2;
        int row = index % 2;
        int rowBytes = GameOptionsPageDefinitions.MenuTilemapWidth * sizeof(ushort);
        return new(LanguageRegions.FirstByteOffset + rowBytes * (row + LanguageRegions.LanguageRowSpacing * language),
            language == 0 ? LanguageRegions.EnglishByteCount : LanguageRegions.JapaneseByteCount,
            HighlightWhenJapanese: language != 0);
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
        int rowStride = GameOptionsPageDefinitions.MenuTilemapWidth * sizeof(ushort);
        int origin = SpecialToggles.FirstByteOffset + SpecialToggles.SettingRowSpacing * rowStride * setting;
        int choiceStride = SpecialToggles.ChoiceCellSpacing * sizeof(ushort);
        return new(origin, origin + rowStride, origin + choiceStride, origin + rowStride + choiceStride);
    }
}
