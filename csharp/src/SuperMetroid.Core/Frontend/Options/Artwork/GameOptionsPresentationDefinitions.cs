using SuperMetroid.Core.Input;
namespace SuperMetroid.Core.Frontend.Options.Artwork;

/// <summary>The options menu's pages, controller labels and the names its pieces are pictured under.</summary>
public static class GameOptionsPresentationDefinitions
{
    /// <summary>The assignable buttons whose glyphs the controller page shows, in selector order: X, A, B, Select, Y, L, R.</summary>
    public static IReadOnlyList<ControllerButton> ControllerLabelButtons { get; } =
        [ControllerButton.X, ControllerButton.A, ControllerButton.B, ControllerButton.Select, ControllerButton.Y, ControllerButton.L, ControllerButton.R];
    /// <summary>The JSON names of the cursor frame sprites, by frame.</summary>
    internal static readonly string[] CursorSpriteNames = ["Cursor.0", "Cursor.1", "Cursor.2", "Cursor.3"];

    /// <summary>The name each options page's pictures are filed under.</summary>
    internal static readonly IReadOnlyDictionary<GameOptionsScreen, string> PageNames =
        new Dictionary<GameOptionsScreen, string>
        {
            [GameOptionsScreen.Primary] = "Primary",
            [GameOptionsScreen.ControllerEnglish] = "Controller.English",
            [GameOptionsScreen.ControllerJapanese] = "Controller.Japanese",
            [GameOptionsScreen.SpecialEnglish] = "Special.English",
            [GameOptionsScreen.SpecialJapanese] = "Special.Japanese",
        };

    /// <summary>The JSON name of a menu layout's heading sprite.</summary>
    internal static readonly IReadOnlyDictionary<GameOptionsPage, string> HeadingSpriteNames =
        new Dictionary<GameOptionsPage, string>
        {
            [GameOptionsPage.Primary] = "Heading.Primary",
            [GameOptionsPage.Controller] = "Heading.Controller",
            [GameOptionsPage.Special] = "Heading.Special",
        };


    /// <summary>The JSON name of an assignable button's glyph.</summary>
    internal static readonly IReadOnlyDictionary<ControllerButton, string> ControllerLabelNames =
        new Dictionary<ControllerButton, string>
        {
            [ControllerButton.X] = "X",
            [ControllerButton.A] = "A",
            [ControllerButton.B] = "B",
            [ControllerButton.Select] = "Select",
            [ControllerButton.Y] = "Y",
            [ControllerButton.L] = "L",
            [ControllerButton.R] = "R",
        };

    /// <summary>The JSON name of a special setting's visual.</summary>
    internal static readonly IReadOnlyDictionary<GameOptionsToggle, string> ToggleNames =
        new Dictionary<GameOptionsToggle, string>
        {
            [GameOptionsToggle.IconCancel] = "IconCancel",
            [GameOptionsToggle.Moonwalk] = "Moonwalk",
        };

    /// <summary>Returns the number of selection-row anchors required by a menu layout.</summary>
    internal static readonly IReadOnlyDictionary<GameOptionsPage, int> CursorCounts =
        new Dictionary<GameOptionsPage, int>
        {
            [GameOptionsPage.Primary] = GameOptionsDefinitions.Rows.PrimaryCount,
            [GameOptionsPage.Controller] = GameOptionsDefinitions.Rows.ControllerCount,
            [GameOptionsPage.Special] = GameOptionsDefinitions.Rows.SpecialCount,
        };
}
