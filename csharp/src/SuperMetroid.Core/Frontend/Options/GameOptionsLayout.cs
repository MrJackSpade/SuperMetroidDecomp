using SuperMetroid.Core.Input;

namespace SuperMetroid.Core.Frontend.Options;

/// <summary>Where the options screen's rows, cursors, labels, language regions, toggle boxes and headings are, from <see cref="GameOptionsDefinitions"/>.</summary>
public static class GameOptionsLayout
{
    /// <summary>The primary page's rows from the top.</summary>
    internal static readonly GameOptionsPrimaryRow[] PrimaryRows =
    [
        GameOptionsPrimaryRow.StartGame, GameOptionsPrimaryRow.EnglishText, GameOptionsPrimaryRow.JapaneseText,
        GameOptionsPrimaryRow.ControllerSettings, GameOptionsPrimaryRow.SpecialSettings,
    ];

    /// <summary>The controller page's action rows from the top: the one place the menu's action order lives.</summary>
    internal static readonly ControllerAction[] ControllerActionRows =
    [
        ControllerAction.Shoot, ControllerAction.Jump, ControllerAction.Dash, ControllerAction.ItemSelect,
        ControllerAction.ItemCancel, ControllerAction.AimUp, ControllerAction.AimDown,
    ];

    /// <summary>The special page's rows from the top.</summary>
    internal static readonly GameOptionsSpecialRow[] SpecialRows =
        [GameOptionsSpecialRow.IconCancel, GameOptionsSpecialRow.Moonwalk, GameOptionsSpecialRow.Exit];
}
