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

    /// <summary>The special page's rows from the top.</summary>
    internal static readonly GameOptionsSpecialRow[] SpecialRows =
        [GameOptionsSpecialRow.IconCancel, GameOptionsSpecialRow.Moonwalk, GameOptionsSpecialRow.Exit];
}
