namespace SuperMetroid.Core.Frontend.Options;

/// <summary>Where the options screen's rows, cursors, labels, language regions, toggle boxes and headings are, from <see cref="GameOptionsDefinitions"/>.</summary>
public static class GameOptionsLayout
{
    /// <summary>Classifies a controller-page cursor row: assignable action rows precede Exit, then Reset.</summary>
    /// <exception cref="ArgumentOutOfRangeException">The row is outside the nine-row page.</exception>
    public static GameOptionsControllerRowKind ControllerRowKind(int row) =>
        row is < 0 or >= GameOptionsDefinitions.Rows.ControllerCount ? throw new ArgumentOutOfRangeException(nameof(row))
        : row < GameOptionsDefinitions.Rows.ControllerActionCount ? GameOptionsControllerRowKind.Action
        : row < GameOptionsDefinitions.Rows.ControllerReset ? GameOptionsControllerRowKind.Exit
        : GameOptionsControllerRowKind.Reset;

    /// <summary>The primary page's cursor row for selected row 0..4: evenly spaced, with the authored gap after row 1.</summary>
    /// <exception cref="ArgumentOutOfRangeException">The row is outside the five rows.</exception>
    public static ushort PrimaryY(int row)
    {
        if ((uint)row >= GameOptionsDefinitions.Rows.PrimaryCount) throw new ArgumentOutOfRangeException(nameof(row));
        int raise = row >= GameOptionsDefinitions.Cursors.PrimaryFirstRaisedRow ? GameOptionsDefinitions.Cursors.PrimaryLowerRowsRaise : 0;
        return (ushort)(GameOptionsDefinitions.Cursors.PrimaryFirstY + GameOptionsDefinitions.Cursors.PrimaryRowSpacing * row - raise);
    }

    /// <summary>The controller page's cursor row for selected row 0..8; Exit and Reset account for the page's scroll.</summary>
    /// <exception cref="ArgumentOutOfRangeException">The row is outside the nine rows.</exception>
    public static ushort ControllerY(int row)
    {
        if ((uint)row >= GameOptionsDefinitions.Rows.ControllerCount) throw new ArgumentOutOfRangeException(nameof(row));
        int scroll = row >= GameOptionsDefinitions.Rows.ControllerExit ? GameOptionsDefinitions.ControllerScrollLimit : 0;
        return (ushort)(GameOptionsDefinitions.Cursors.ControllerFirstY + GameOptionsDefinitions.Cursors.ControllerRowSpacing * row - scroll);
    }

    /// <summary>The special page's cursor row for selected row 0..2.</summary>
    /// <exception cref="ArgumentOutOfRangeException">The row is outside the three rows.</exception>
    public static ushort SpecialY(int row)
    {
        if ((uint)row >= GameOptionsDefinitions.Rows.SpecialCount) throw new ArgumentOutOfRangeException(nameof(row));
        return (ushort)(GameOptionsDefinitions.Cursors.SpecialFirstY + GameOptionsDefinitions.Cursors.SpecialRowSpacing * row);
    }

    /// <summary>The page cell of each controller-action label's upper-left cell, action 0..6, each three rows below the last.</summary>
    /// <exception cref="ArgumentOutOfRangeException">The action is outside the seven assignable rows.</exception>
    public static Position16<TilemapCells> ControllerLabelAnchor(int action)
    {
        if ((uint)action >= GameOptionsDefinitions.Rows.ControllerActionCount) throw new ArgumentOutOfRangeException(nameof(action));
        return new(GameOptionsDefinitions.ControllerLabels.AnchorColumn,
            checked((ushort)(GameOptionsDefinitions.ControllerLabels.FirstAnchorRow + GameOptionsDefinitions.ControllerLabels.AnchorRowSpacing * action)));
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
        return new(GameOptionsDefinitions.LanguageRegions.FirstByteOffset +
                rowBytes * (row + GameOptionsDefinitions.LanguageRegions.LanguageRowSpacing * language),
            language == 0 ? GameOptionsDefinitions.LanguageRegions.EnglishByteCount : GameOptionsDefinitions.LanguageRegions.JapaneseByteCount,
            HighlightWhenJapanese: language != 0);
    }

    /// <summary>The Icon Cancel toggle's choice boxes.</summary>
    public static GameOptionsToggleLayout IconCancel => ToggleLayout((int)GameOptionsSpecialRow.IconCancel);

    /// <summary>The Moonwalk toggle's choice boxes, six tilemap rows below Icon Cancel's.</summary>
    public static GameOptionsToggleLayout Moonwalk => ToggleLayout((int)GameOptionsSpecialRow.Moonwalk);

    /// <summary>
    /// The two-row choice boxes of Icon Cancel (0) or Moonwalk (1): the toggles are six tilemap rows apart and their
    /// choices seven cells apart. Exit has no boxes.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">The setting is not a toggle.</exception>
    public static GameOptionsToggleLayout ToggleLayout(int setting)
    {
        if ((uint)setting >= (int)GameOptionsSpecialRow.Exit) throw new ArgumentOutOfRangeException(nameof(setting));
        int rowStride = GameOptionsDefinitions.MenuTilemapWidth * sizeof(ushort);
        int origin = GameOptionsDefinitions.SpecialToggles.FirstByteOffset +
            GameOptionsDefinitions.SpecialToggles.SettingRowSpacing * rowStride * setting;
        int choiceStride = GameOptionsDefinitions.SpecialToggles.ChoiceCellSpacing * sizeof(ushort);
        return new(origin, origin + rowStride, origin + choiceStride,
            origin + rowStride + choiceStride);
    }

    /// <summary>The column of a page's heading.</summary>
    /// <exception cref="ArgumentOutOfRangeException">The page is not an options page.</exception>
    internal static ushort HeadingX(GameOptionsPage page) => page switch
    {
        GameOptionsPage.Primary => GameOptionsDefinitions.Headings.PrimaryX,
        GameOptionsPage.Controller => GameOptionsDefinitions.Headings.ControllerX,
        GameOptionsPage.Special => GameOptionsDefinitions.Headings.SpecialX,
        _ => throw new ArgumentOutOfRangeException(nameof(page)),
    };
}
