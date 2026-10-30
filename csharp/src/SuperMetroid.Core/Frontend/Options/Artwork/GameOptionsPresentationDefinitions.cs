using SuperMetroid.Core.Input;
namespace SuperMetroid.Core.Frontend.Options.Artwork;

/// <summary>Schema names and geometry for <c>options-menu.json</c>.</summary>
public static class GameOptionsPresentationDefinitions
{
    /// <summary>Supported options-menu presentation JSON schema version.</summary>
    public const int Version = 1;
    /// <summary>Extracted JSON filename for options artwork, highlight regions, sprite compositions, and anchors.</summary>
    public const string FileName = "options-menu.json";
    /// <summary>The JSON name of the common BG2 background page, separate from the foreground option pages.</summary>
    public const string BackgroundPageName = "Background";

    /// <summary>The foreground option pages in document order: primary, then English and Japanese controller and special pages.</summary>
    public static IReadOnlyList<GameOptionsScreen> Tilemaps { get; } = Enum.GetValues<GameOptionsScreen>();
    /// <summary>The three menu layouts headings and cursors are anchored by, in primary, controller and special order.</summary>
    internal static IReadOnlyList<GameOptionsPage> MenuPages { get; } = Enum.GetValues<GameOptionsPage>();
    /// <summary>The assignable buttons whose glyphs the controller page shows, in native selector order: X, A, B, Select, Y, L, R.</summary>
    public static IReadOnlyList<ControllerButton> ControllerLabelButtons { get; } =
        [ControllerButton.X, ControllerButton.A, ControllerButton.B, ControllerButton.Select, ControllerButton.Y, ControllerButton.L, ControllerButton.R];
    /// <summary>The two special settings, icon cancel then moonwalk.</summary>
    public static IReadOnlyList<GameOptionsToggle> Toggles { get; } = Enum.GetValues<GameOptionsToggle>();
    /// <summary>The missile cursor's looping animation frames.</summary>
    public const int CursorFrameCount = 4;
    /// <summary>The JSON names of the cursor frame sprites, by frame.</summary>
    internal static readonly string[] CursorSpriteNames = ["Cursor.0", "Cursor.1", "Cursor.2", "Cursor.3"];

    /// <summary>The JSON name of a foreground page.</summary>
    internal static readonly IReadOnlyDictionary<GameOptionsScreen, string> TilemapNames =
        new Dictionary<GameOptionsScreen, string>
        {
            [GameOptionsScreen.Primary] = "Primary",
            [GameOptionsScreen.ControllerEnglish] = "Controller.English",
            [GameOptionsScreen.ControllerJapanese] = "Controller.Japanese",
            [GameOptionsScreen.SpecialEnglish] = "Special.English",
            [GameOptionsScreen.SpecialJapanese] = "Special.Japanese",
        };

    /// <summary>The JSON name of a menu layout, which keys its heading and cursor anchors.</summary>
    internal static readonly IReadOnlyDictionary<GameOptionsPage, string> MenuNames =
        new Dictionary<GameOptionsPage, string>
        {
            [GameOptionsPage.Primary] = "Primary",
            [GameOptionsPage.Controller] = "Controller",
            [GameOptionsPage.Special] = "Special",
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
