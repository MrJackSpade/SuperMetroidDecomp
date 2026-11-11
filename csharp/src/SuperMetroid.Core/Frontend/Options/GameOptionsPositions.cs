using SuperMetroid.Core.Input;
using static SuperMetroid.Core.Frontend.Options.GameOptionsLayout;

namespace SuperMetroid.Core.Frontend.Options;

/// <summary>
/// Classifies the options screen's rows (<see cref="GameOptionsLayout"/>); the pages' labels, language highlights and
/// toggle boxes are the import's, which pictures them (#1467), and the cursor's and headings' anchors are installed (#1615).
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

    /// <summary>The primary page's row at cursor position <paramref name="row"/>, or null past the page's last row.</summary>
    internal static GameOptionsPrimaryRow? PrimaryRowAt(int row) => (uint)row < (uint)PrimaryRows.Length ? PrimaryRows[row] : null;

    /// <summary>The action the controller page's cursor position <paramref name="row"/> binds, or null on Exit, Reset or past the page.</summary>
    internal static ControllerAction? ControllerActionAt(int row) =>
        (uint)row < (uint)ControllerActionRows.Length ? ControllerActionRows[row] : null;

    /// <summary>The special page's row at cursor position <paramref name="row"/>, or null past the page's last row.</summary>
    internal static GameOptionsSpecialRow? SpecialRowAt(int row) => (uint)row < (uint)SpecialRows.Length ? SpecialRows[row] : null;
}
