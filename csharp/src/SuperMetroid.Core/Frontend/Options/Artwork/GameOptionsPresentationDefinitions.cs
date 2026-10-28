using SuperMetroid.Core.Input;
using SuperMetroid.Core.Frontend.Menus.Artwork;
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
    /// <summary>Number of row-major tile cells in each 32-by-32 options page.</summary>
    public const int PageCellCount = GameOptionsDefinitions.MenuTilemapWidth * GameOptionsDefinitions.MenuTilemapHeight;
    /// <summary>Height in characters of the atlas addressed by authored background cells.</summary>
    public const int CharacterRows = 32;
    /// <summary>Width in tile cells of a controller-button label patch: three.</summary>
    public const int ControllerLabelWidth = GameOptionsDefinitions.ControllerLabels.WidthInTiles;
    /// <summary>Height in tile cells of a controller-button label patch: two.</summary>
    public const int ControllerLabelHeight = GameOptionsDefinitions.ControllerLabels.HeightInTiles;
    /// <summary>Six row-major tile cells required for each controller-button glyph patch.</summary>
    public const int ControllerLabelCellCount = ControllerLabelWidth * ControllerLabelHeight;
    /// <summary>Size of one controller-label glyph patch in tile cells.</summary>
    public static Extent16<TilemapCells> ControllerLabelSize => new(ControllerLabelWidth, ControllerLabelHeight);

    /// <summary>The foreground option pages in document order: primary, then English and Japanese controller and special pages.</summary>
    public static IReadOnlyList<GameOptionsTilemap> Tilemaps { get; } = Enum.GetValues<GameOptionsTilemap>();
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
    private static readonly string[] cursorSpriteNames = ["Cursor.0", "Cursor.1", "Cursor.2", "Cursor.3"];

    /// <summary>The JSON name of a foreground page.</summary>
    internal static readonly IReadOnlyDictionary<GameOptionsTilemap, string> TilemapNames =
        new Dictionary<GameOptionsTilemap, string>
        {
            [GameOptionsTilemap.Primary] = "Primary",
            [GameOptionsTilemap.ControllerEnglish] = "Controller.English",
            [GameOptionsTilemap.ControllerJapanese] = "Controller.Japanese",
            [GameOptionsTilemap.SpecialEnglish] = "Special.English",
            [GameOptionsTilemap.SpecialJapanese] = "Special.Japanese",
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

    /// <summary>Every frame of the options sheet: each page's heading, then each cursor frame.</summary>
    public static IEnumerable<string> SheetFrames =>
        MenuPages.Select(HeadingSpriteName).Concat(Enumerable.Range(0, CursorFrameCount).Select(CursorSpriteName));

    /// <summary>The menu heading a menu layout draws.</summary>
    internal static readonly IReadOnlyDictionary<GameOptionsPage, MenuHeading> Headings =
        new Dictionary<GameOptionsPage, MenuHeading>
        {
            [GameOptionsPage.Primary] = MenuHeading.OptionsPrimary,
            [GameOptionsPage.Controller] = MenuHeading.OptionsController,
            [GameOptionsPage.Special] = MenuHeading.OptionsSpecial,
        };

    /// <summary>The JSON name of a looping missile-cursor frame's sprite, for frames 0-3.</summary>
    public static string CursorSpriteName(int frame) =>
        Ensure.Element(cursorSpriteNames, frame);

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

    /// <summary>The assignable button a native selector names.</summary>
    /// <param name="index">Zero-based index 0-6 in X, A, B, Select, Y, L, R order.</param>
    /// <exception cref="ArgumentOutOfRangeException">The index is outside 0-6.</exception>
    public static ControllerButton ControllerLabelButton(int index) =>
        Ensure.Element(ControllerLabelButtons, index);

    /// <summary>$82:F665 ButtonTilemaps_A: upper-left A glyph; right half mirrors it.</summary>
    internal const int AButtonTile = 0x90;
    /// <summary>$82:F671 ButtonTilemaps_B: upper-left B glyph; right half is next in atlas.</summary>
    internal const int BButtonTile = 0x91;
    /// <summary>$82:F659 ButtonTilemaps_X: diagonal half; opposite half flips both axes.</summary>
    internal const int XButtonTile = 0x93;
    /// <summary>$82:F689 ButtonTilemaps_Y: upper-left Y glyph; right half mirrors it.</summary>
    internal const int YButtonTile = 0x94;
    /// <summary>$82:F67D ButtonTilemaps_Select: three consecutive tiles per atlas row.</summary>
    internal const int SelectButtonTile = 0x95;
    /// <summary>$82:F695/F6A1 ButtonTilemaps_L/R: common shoulder outline corner.</summary>
    internal const int ShoulderCornerTile = 0x9a;
    /// <summary>$82:F697 ButtonTilemaps_L: central L glyph; lower half follows one atlas row.</summary>
    internal const int LeftShoulderTile = 0x9b;
    /// <summary>$82:F6A3 ButtonTilemaps_R: central R glyph; lower half follows one atlas row.</summary>
    internal const int RightShoulderTile = 0x9c;
    /// <summary>$82:F65D and sibling two-column labels pad the third column with tile $0F.</summary>
    internal const int ButtonPaddingTile = 0x0f;
    /// <summary>$82:F659-F6AC glyph upper/lower halves occupy adjacent 16-character atlas rows.</summary>
    internal const int ButtonAtlasRowStride = 16;
    /// <summary>Returns the number of selection-row anchors required by a menu layout.</summary>
    internal static readonly IReadOnlyDictionary<GameOptionsPage, int> CursorCounts =
        new Dictionary<GameOptionsPage, int>
        {
            [GameOptionsPage.Primary] = GameOptionsDefinitions.Rows.PrimaryCount,
            [GameOptionsPage.Controller] = GameOptionsDefinitions.Rows.ControllerCount,
            [GameOptionsPage.Special] = GameOptionsDefinitions.Rows.SpecialCount,
        };
}
