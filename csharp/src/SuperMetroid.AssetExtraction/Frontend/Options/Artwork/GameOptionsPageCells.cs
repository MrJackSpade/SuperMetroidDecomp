using SuperMetroid.Core.Frontend.Options;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Spatial;
using SuperMetroid.AssetExtraction.Artwork;
using static SuperMetroid.AssetExtraction.Frontend.Options.Artwork.GameOptionsPageDefinitions;

namespace SuperMetroid.AssetExtraction.Frontend.Options.Artwork;

/// <summary>
/// Lays out the options pages' dynamic cells as the cartridge does (<see cref="GameOptionsPageDefinitions"/>, #1467): each
/// controller label's box, the language-label rows and the toggle choice boxes the cartridge recolours, which the import
/// pictures from its import data; the installation holds the pictures. The game never addresses a page cell.
/// </summary>
public static class GameOptionsPageCells
{

    /// <summary>One of a language label's two rows.</summary>
    public enum GameOptionsLabelRow
    {
        /// <summary>The label's upper row.</summary>
        Upper,

        /// <summary>The label's lower row.</summary>
        Lower,
    }

    /// <summary>
    /// The page cell of <paramref name="action"/>'s label's upper-left cell: each action's label three rows below the label of
    /// the action listed above it (<see cref="GameOptionsPageRows.ControllerActions"/>).
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">The action is not one of the seven assignable rows.</exception>
    public static Position16<CharacterCells> ControllerLabelAnchor(ControllerAction action)
    {
        if (!GameOptionsPageRows.ControllerActions.Contains(action)) throw new ArgumentOutOfRangeException(nameof(action), action, "Not an assignable action.");
        int rowsAbove = GameOptionsPageRows.ControllerActions.PlaceOf(action);
        return new(ControllerLabels.AnchorColumn, checked((ushort)(ControllerLabels.FirstAnchorRow + ControllerLabels.AnchorRowSpacing * rowsAbove)));
    }

    /// <summary>
    /// The primary page's four language-label rows, as the installed regions list them: the English label's upper and lower
    /// rows, then the Japanese label's.
    /// </summary>
    public static IReadOnlyList<GameOptionsLanguagePaletteRegion> LanguagePaletteRegions { get; } =
    [
        LanguagePaletteRegion(TextLanguage.English, GameOptionsLabelRow.Upper),
        LanguagePaletteRegion(TextLanguage.English, GameOptionsLabelRow.Lower),
        LanguagePaletteRegion(TextLanguage.Japanese, GameOptionsLabelRow.Upper),
        LanguagePaletteRegion(TextLanguage.Japanese, GameOptionsLabelRow.Lower),
    ];

    /// <summary>
    /// The <paramref name="row"/> row of <paramref name="language"/>'s label on the primary page; the Japanese label's take the
    /// selected palette when Japanese is active.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">The language or the row is undefined.</exception>
    private static GameOptionsLanguagePaletteRegion LanguagePaletteRegion(GameOptionsLabelLanguage language, GameOptionsLabelRow row)
    {
        int rowsBelowLabelTop = row switch
        {
            GameOptionsLabelRow.Upper => 0,
            GameOptionsLabelRow.Lower => 1,
            _ => throw new ArgumentOutOfRangeException(nameof(row), row, "Undefined label row."),
        };
        int labelsAbove = language switch
        {
            TextLanguage.English => 0,
            TextLanguage.Japanese => 1,
            _ => throw new ArgumentOutOfRangeException(nameof(language), language, "Undefined label language."),
        };
        int byteCount = language == TextLanguage.Japanese ? LanguageRegions.JapaneseByteCount : LanguageRegions.EnglishByteCount;
        int rowBytes = GameOptionsPageDefinitions.MenuTilemapWidth * sizeof(ushort);
        return new(LanguageRegions.FirstByteOffset + rowBytes * (rowsBelowLabelTop + LanguageRegions.LanguageRowSpacing * labelsAbove),
            byteCount, language);
    }

    /// <summary>The Icon Cancel toggle's choice boxes.</summary>
    public static GameOptionsToggleLayout IconCancel => ToggleLayout(GameOptionsSpecialRow.IconCancel);

    /// <summary>The Moonwalk toggle's choice boxes, six tilemap rows below Icon Cancel's.</summary>
    public static GameOptionsToggleLayout Moonwalk => ToggleLayout(GameOptionsSpecialRow.Moonwalk);

    /// <summary>
    /// The two-row choice boxes of the toggle row <paramref name="toggle"/>: each toggle's boxes six tilemap rows below the
    /// boxes of the toggle listed above it on the special page, and their choices seven cells apart. Exit has no boxes.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">The row is not a toggle.</exception>
    private static GameOptionsToggleLayout ToggleLayout(GameOptionsSpecialRow toggle)
    {
        if (toggle is not (GameOptionsSpecialRow.IconCancel or GameOptionsSpecialRow.Moonwalk))
            throw new ArgumentOutOfRangeException(nameof(toggle), toggle, "Not a toggle row.");
        int rowsAbove = GameOptionsPageRows.Special.PlaceOf(toggle);
        int rowStride = GameOptionsPageDefinitions.MenuTilemapWidth * sizeof(ushort);
        int origin = SpecialToggles.FirstByteOffset + SpecialToggles.SettingRowSpacing * rowStride * rowsAbove;
        int choiceStride = SpecialToggles.ChoiceCellSpacing * sizeof(ushort);
        return new(origin, origin + rowStride, origin + choiceStride, origin + rowStride + choiceStride);
    }
}
