using SuperMetroid.Core.Frontend.Options;
using SuperMetroid.Core.Input;

namespace SuperMetroid.AssetExtraction.Frontend.Options.Artwork;

/// <summary>
/// Names the options pages' special settings and controller actions by their row on the page, as a check counts them: a
/// row past the page's names a setting or action the game does not have.
/// </summary>
internal static class GameOptionsPageCellsAccess
{
    /// <summary>The setting or action a row past the page's names: one the game does not have.</summary>
    private const int Undefined = 99;

    /// <summary>The special setting on <paramref name="row"/> of the special page, or an undefined one past its rows.</summary>
    internal static GameOptionsSpecialRow SpecialRowAt(int row) =>
        row >= 0 && row < GameOptionsPageRows.Special.Count ? GameOptionsPageRows.Special[row] : (GameOptionsSpecialRow)Undefined;

    /// <summary>The controller action on <paramref name="row"/> of the controller page, or an undefined one past its rows.</summary>
    internal static ControllerAction ControllerActionAt(int row) =>
        row >= 0 && row < GameOptionsPageRows.ControllerActions.Count ? GameOptionsPageRows.ControllerActions[row] : (ControllerAction)Undefined;

    /// <summary>Lays out the special settings by row.</summary>
    extension(GameOptionsPageCells)
    {
        /// <summary>The layout of the special setting on <paramref name="setting"/>'s row.</summary>
        internal static GameOptionsToggleLayout ToggleLayout(int setting) => GameOptionsPageCells.ToggleLayout(SpecialRowAt(setting));
    }
}
