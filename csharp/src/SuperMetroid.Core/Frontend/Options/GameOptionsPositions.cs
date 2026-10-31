using static SuperMetroid.Core.Frontend.Options.GameOptionsLayout;

namespace SuperMetroid.Core.Frontend.Options;

/// <summary>
/// Places the options screen's rows, cursors and headings (<see cref="GameOptionsLayout"/>); the pages' labels,
/// language highlights and toggle boxes are the import's, which pictures them (#1467).
/// </summary>
public static class GameOptionsPositions
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
    public static int PrimaryY(int row)
    {
        if ((uint)row >= GameOptionsDefinitions.Rows.PrimaryCount) throw new ArgumentOutOfRangeException(nameof(row));
        int raise = row >= GameOptionsDefinitions.Cursors.PrimaryFirstRaisedRow ? GameOptionsDefinitions.Cursors.PrimaryLowerRowsRaise : 0;
        return CoordinateAxis.Wrap(GameOptionsDefinitions.Cursors.PrimaryFirstY + GameOptionsDefinitions.Cursors.PrimaryRowSpacing * row - raise);
    }

    /// <summary>The controller page's cursor row for selected row 0..8; Exit and Reset account for the page's scroll.</summary>
    /// <exception cref="ArgumentOutOfRangeException">The row is outside the nine rows.</exception>
    public static int ControllerY(int row)
    {
        if ((uint)row >= GameOptionsDefinitions.Rows.ControllerCount) throw new ArgumentOutOfRangeException(nameof(row));
        int scroll = row >= GameOptionsDefinitions.Rows.ControllerExit ? GameOptionsDefinitions.ControllerScrollLimit : 0;
        return CoordinateAxis.Wrap(GameOptionsDefinitions.Cursors.ControllerFirstY + GameOptionsDefinitions.Cursors.ControllerRowSpacing * row - scroll);
    }

    /// <summary>The special page's cursor row for selected row 0..2.</summary>
    /// <exception cref="ArgumentOutOfRangeException">The row is outside the three rows.</exception>
    public static int SpecialY(int row)
    {
        if ((uint)row >= GameOptionsDefinitions.Rows.SpecialCount) throw new ArgumentOutOfRangeException(nameof(row));
        return CoordinateAxis.Wrap(GameOptionsDefinitions.Cursors.SpecialFirstY + GameOptionsDefinitions.Cursors.SpecialRowSpacing * row);
    }

    /// <summary>The primary page's row at cursor position <paramref name="row"/>, or null past the page's last row.</summary>
    internal static GameOptionsPrimaryRow? PrimaryRowAt(int row) => (uint)row < (uint)PrimaryRows.Length ? PrimaryRows[row] : null;

    /// <summary>The special page's row at cursor position <paramref name="row"/>, or null past the page's last row.</summary>
    internal static GameOptionsSpecialRow? SpecialRowAt(int row) => (uint)row < (uint)SpecialRows.Length ? SpecialRows[row] : null;

    /// <summary>The column of a page's heading.</summary>
    /// <exception cref="ArgumentOutOfRangeException">The page is not an options page.</exception>
    internal static int HeadingX(GameOptionsPage page) => page switch
    {
        GameOptionsPage.Primary => GameOptionsDefinitions.Headings.PrimaryX,
        GameOptionsPage.Controller => GameOptionsDefinitions.Headings.ControllerX,
        GameOptionsPage.Special => GameOptionsDefinitions.Headings.SpecialX,
        _ => throw new ArgumentOutOfRangeException(nameof(page)),
    };
}
