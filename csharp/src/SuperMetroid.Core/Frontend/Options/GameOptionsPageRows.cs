using SuperMetroid.Core.Frontend.Menus;
using SuperMetroid.Core.Input;

namespace SuperMetroid.Core.Frontend.Options;

/// <summary>
/// Each options page's rows from the top (#2206, #2225): the order the cursor moves through and the order of each page's
/// installed cursor anchors, the controller page's action labels included.
/// </summary>
internal static class GameOptionsPageRows
{
    /// <summary>The primary page's rows from the top.</summary>
    internal static readonly MenuRowOrder<GameOptionsPrimaryRow> Primary = new(
    [
        GameOptionsPrimaryRow.StartGame, GameOptionsPrimaryRow.EnglishText, GameOptionsPrimaryRow.JapaneseText,
        GameOptionsPrimaryRow.ControllerSettings, GameOptionsPrimaryRow.SpecialSettings,
    ]);

    /// <summary>The controller page's action rows from the top: the one place the menu's action order lives.</summary>
    internal static readonly MenuRowOrder<ControllerAction> ControllerActions = new(
    [
        ControllerAction.Shoot, ControllerAction.Jump, ControllerAction.Dash, ControllerAction.ItemSelect,
        ControllerAction.ItemCancel, ControllerAction.AimUp, ControllerAction.AimDown,
    ]);

    /// <summary>The controller page's rows from the top: a row assigning each action, then Exit and Reset.</summary>
    internal static readonly MenuRowOrder<GameOptionsControllerRow> Controller = new(
    [
        .. ControllerActions.Select(action => new GameOptionsControllerRow.AssignsAction(action)),
        GameOptionsControllerRow.Exit.Instance, GameOptionsControllerRow.Reset.Instance,
    ]);

    /// <summary>The special page's rows from the top.</summary>
    internal static readonly MenuRowOrder<GameOptionsSpecialRow> Special = new(
        [GameOptionsSpecialRow.IconCancel, GameOptionsSpecialRow.Moonwalk, GameOptionsSpecialRow.Exit]);
}
