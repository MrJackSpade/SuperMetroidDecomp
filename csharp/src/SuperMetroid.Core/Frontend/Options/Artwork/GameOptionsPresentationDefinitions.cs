namespace SuperMetroid.Core.Frontend.Options.Artwork;

/// <summary>Schema names and geometry for <c>options-menu.json</c>.</summary>
public static class GameOptionsPresentationDefinitions
{
    /// <summary>Supported options-menu presentation JSON schema version.</summary>
    public const int Version = 1;
    /// <summary>Extracted JSON filename for options artwork, highlight regions, sprite compositions, and anchors.</summary>
    public const string FileName = "options-menu.json";
    /// <summary>Common BG2 background page identity, separate from foreground option pages.</summary>
    public const string BackgroundPage = "Background";
    /// <summary>Primary foreground tilemap identity containing start, language, controller, and special-settings choices.</summary>
    public const string PrimaryPage = "Primary";
    /// <summary>English controller-settings foreground page identity.</summary>
    public const string ControllerEnglishPage = "Controller.English";
    /// <summary>Japanese controller-settings foreground page identity.</summary>
    public const string ControllerJapanesePage = "Controller.Japanese";
    /// <summary>English special-settings foreground page identity.</summary>
    public const string SpecialEnglishPage = "Special.English";
    /// <summary>Japanese special-settings foreground page identity.</summary>
    public const string SpecialJapanesePage = "Special.Japanese";
    /// <summary>Primary menu layout identity for heading and cursor anchors, shared across language choices.</summary>
    public const string PrimaryMenu = "Primary";
    /// <summary>Controller menu layout identity for heading and cursor anchors, independent of translated foreground pages.</summary>
    public const string ControllerMenu = "Controller";
    /// <summary>Special menu layout identity for heading and cursor anchors, independent of translated foreground pages.</summary>
    public const string SpecialMenu = "Special";
    /// <summary>Special-toggle visual identity for the icon-cancel enabled and disabled labels.</summary>
    public const string IconCancelToggle = "IconCancel";
    /// <summary>Special-toggle visual identity for the moonwalk enabled and disabled labels.</summary>
    public const string MoonwalkToggle = "Moonwalk";
    /// <summary>Number of row-major tile cells in each 32-by-32 options page.</summary>
    public const int PageCellCount = GameOptionsRomData.MenuTilemapWidth * GameOptionsRomData.MenuTilemapHeight;
    /// <summary>Height in characters of the atlas addressed by authored background cells.</summary>
    public const int CharacterRows = 32;
    /// <summary>Width in tile cells of a controller-button label patch: three.</summary>
    public const int ControllerLabelWidth = GameOptionsRomData.ControllerLabels.WidthInTiles;
    /// <summary>Height in tile cells of a controller-button label patch: two.</summary>
    public const int ControllerLabelHeight = GameOptionsRomData.ControllerLabels.HeightInTiles;
    /// <summary>Six row-major tile cells required for each controller-button glyph patch.</summary>
    public const int ControllerLabelCellCount = ControllerLabelWidth * ControllerLabelHeight;
    /// <summary>Size of one controller-label glyph patch in tile cells.</summary>
    public static Extent16<TilemapCells> ControllerLabelSize => new(ControllerLabelWidth, ControllerLabelHeight);

    /// <summary>Required page identities in the options document.</summary>
    private static readonly string[] pageNames =
        [BackgroundPage, PrimaryPage, ControllerEnglishPage, ControllerJapanesePage,
            SpecialEnglishPage, SpecialJapanesePage];
    /// <summary>Language-independent menu identities used for headings and cursors.</summary>
    private static readonly string[] menuPageNames =
        [PrimaryMenu, ControllerMenu, SpecialMenu];
    /// <summary>Assignable controller glyph identities in native selector order.</summary>
    private static readonly string[] controllerLabelNames =
        ["X", "A", "B", "Select", "Y", "L", "R"];
    /// <summary>Required visual identities for the two special settings.</summary>
    private static readonly string[] specialToggleNames =
        [IconCancelToggle, MoonwalkToggle];
    /// <summary>Required heading and animated cursor sprite identities.</summary>
    private static readonly string[] spriteNames =
        ["Heading.Primary", "Heading.Controller", "Heading.Special",
            "Cursor.0", "Cursor.1", "Cursor.2", "Cursor.3"];

    /// <summary>Read-only view of the six required page identities: background, primary, then English and Japanese controller and special pages.</summary>
    public static ReadOnlySpan<string> PageNames => pageNames;
    /// <summary>Read-only view of the three required heading/cursor layout identities in primary, controller, and special order.</summary>
    public static ReadOnlySpan<string> MenuPageNames => menuPageNames;
    /// <summary>Read-only view of assignable-button glyph identities in native selector order: X, A, B, Select, Y, L, R.</summary>
    public static ReadOnlySpan<string> ControllerLabelNames => controllerLabelNames;
    /// <summary>Read-only view of the required special-setting visual identities: icon cancel and moonwalk.</summary>
    public static ReadOnlySpan<string> SpecialToggleNames => specialToggleNames;
    /// <summary>Read-only view of seven required sprite identities: three menu headings followed by four cursor frames.</summary>
    public static ReadOnlySpan<string> SpriteNames => spriteNames;

    /// <summary>Returns the glyph identity for a native assignable-button selector.</summary>
    /// <param name="index">Zero-based index 0-6 in X, A, B, Select, Y, L, R order.</param>
    /// <returns>The controller-label dictionary key.</returns>
    /// <exception cref="ArgumentOutOfRangeException">The index is outside 0-6.</exception>
    public static string ControllerLabelName(int index) =>
        (uint)index < controllerLabelNames.Length
            ? controllerLabelNames[index]
            : throw new ArgumentOutOfRangeException(nameof(index));

    /// <summary>Returns the sprite composition identity for a looping missile-cursor frame.</summary>
    /// <param name="index">Zero-based animation frame 0-3.</param>
    /// <returns><c>Cursor.</c> followed by the frame number.</returns>
    /// <exception cref="ArgumentOutOfRangeException">The frame is outside 0-3.</exception>
    public static string CursorFrameName(int index) => index switch
    {
        0 => "Cursor.0",
        1 => "Cursor.1",
        2 => "Cursor.2",
        3 => "Cursor.3",
        _ => throw new ArgumentOutOfRangeException(nameof(index)),
    };

    /// <summary>Maps a language-independent menu layout identity to its heading sprite composition.</summary>
    /// <param name="page">The primary, controller, or special menu identity; translated page names are not accepted.</param>
    /// <returns>The menu identity prefixed with <c>Heading.</c>.</returns>
    /// <exception cref="ArgumentOutOfRangeException">The value is not one of the three menu identities.</exception>
    public static string HeadingFrameName(string page) => page switch
    {
        PrimaryMenu => "Heading.Primary",
        ControllerMenu => "Heading.Controller",
        SpecialMenu => "Heading.Special",
        _ => throw new ArgumentOutOfRangeException(nameof(page)),
    };

    /// <summary>$82:F665 ButtonTilemaps_A: upper-left A glyph; right half mirrors it.</summary>
    private const int AButtonTile = 0x90;
    /// <summary>$82:F671 ButtonTilemaps_B: upper-left B glyph; right half is next in atlas.</summary>
    private const int BButtonTile = 0x91;
    /// <summary>$82:F659 ButtonTilemaps_X: diagonal half; opposite half flips both axes.</summary>
    private const int XButtonTile = 0x93;
    /// <summary>$82:F689 ButtonTilemaps_Y: upper-left Y glyph; right half mirrors it.</summary>
    private const int YButtonTile = 0x94;
    /// <summary>$82:F67D ButtonTilemaps_Select: three consecutive tiles per atlas row.</summary>
    private const int SelectButtonTile = 0x95;
    /// <summary>$82:F695/F6A1 ButtonTilemaps_L/R: common shoulder outline corner.</summary>
    private const int ShoulderCornerTile = 0x9a;
    /// <summary>$82:F697 ButtonTilemaps_L: central L glyph; lower half follows one atlas row.</summary>
    private const int LeftShoulderTile = 0x9b;
    /// <summary>$82:F6A3 ButtonTilemaps_R: central R glyph; lower half follows one atlas row.</summary>
    private const int RightShoulderTile = 0x9c;
    /// <summary>$82:F65D and sibling two-column labels pad the third column with tile $0F.</summary>
    private const int ButtonPaddingTile = 0x0f;
    /// <summary>$82:F659-F6AC glyph upper/lower halves occupy adjacent 16-character atlas rows.</summary>
    private const int ButtonAtlasRowStride = 16;

    /// <summary>Builds the native button glyph from its letter, atlas rows and geometric reflections.</summary>
    internal static ushort ControllerLabelWord(string label, int cell)
    {
        if ((uint)cell >= ControllerLabelCellCount) throw new ArgumentOutOfRangeException(nameof(cell));
        int row = cell / ControllerLabelWidth;
        int column = cell % ControllerLabelWidth;
        int tile;
        SnesTileFlipFlags flips = SnesTileFlipFlags.None;
        switch (label)
        {
            case "A":
            case "Y":
                tile = column == 2 ? ButtonPaddingTile : (label == "A" ? AButtonTile : YButtonTile) + row * ButtonAtlasRowStride;
                if (column == 1) flips = SnesTileFlipFlags.Horizontal;
                break;
            case "X":
                tile = column == 2 ? ButtonPaddingTile : XButtonTile + (row ^ column) * ButtonAtlasRowStride;
                if (column == 1) flips = SnesTileFlipFlags.Horizontal | SnesTileFlipFlags.Vertical;
                break;
            case "B":
                tile = column == 2 ? ButtonPaddingTile : BButtonTile + column + row * ButtonAtlasRowStride;
                break;
            case "Select":
                tile = SelectButtonTile + column + row * ButtonAtlasRowStride;
                break;
            case "L":
            case "R":
                tile = column == 1 ? (label == "L" ? LeftShoulderTile : RightShoulderTile) + row * ButtonAtlasRowStride : ShoulderCornerTile;
                if (column != 1)
                    flips = (row == 1 ? SnesTileFlipFlags.Vertical : SnesTileFlipFlags.None)
                        | (column == 2 ? SnesTileFlipFlags.Horizontal : SnesTileFlipFlags.None);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(label));
        }
        return SnesBgTilemapWord.Create(tile, 0, false, flips).Raw;
    }
    /// <summary>$82:F639 controller label destinations: three tile rows per action.</summary>
    internal static Position16<TilemapCells> ControllerAnchor(int action)
    {
        int word = GameOptionsRomData.ControllerLabels.Destination(action) / sizeof(ushort);
        return new(checked((ushort)(word % GameOptionsRomData.MenuTilemapWidth)), checked((ushort)(word / GameOptionsRomData.MenuTilemapWidth)));
    }

    /// <summary>$82:F307/F31B/F33F selection missile rows, including controller scroll displacement.</summary>
    internal static Position16<OamPixels> CursorAnchor(string page, int row)
    {
        if (page is not (PrimaryMenu or ControllerMenu or SpecialMenu) || (uint)row >= CursorCount(page))
            throw new InvalidDataException($"Options cursor {page}[{row}] is not authored.");
        return page switch
        {
            PrimaryMenu => new(GameOptionsRomData.Cursors.PrimaryX, GameOptionsRomData.Cursors.PrimaryY(row)),
            ControllerMenu => new(GameOptionsRomData.Cursors.ControllerX, GameOptionsRomData.Cursors.ControllerY(row)),
            _ => new(GameOptionsRomData.Cursors.SpecialX, GameOptionsRomData.Cursors.SpecialY(row)),
        };
    }

    /// <summary>$82:F34B/F353/F35B and F369 named page heading setup anchors.</summary>
    internal static Position16<OamPixels> HeadingAnchor(string page)
    {
        GameOptionsPage selected = page switch
        {
            PrimaryMenu => GameOptionsPage.Primary,
            ControllerMenu => GameOptionsPage.Controller,
            SpecialMenu => GameOptionsPage.Special,
            _ => throw new InvalidDataException($"Options heading {page} is not authored."),
        };
        return new(GameOptionsRomData.Spritemaps.HeadingX(selected), GameOptionsRomData.Spritemaps.HeadingY);
    }

    /// <summary>$82:EDF2..EE51 language highlight calls cover consecutive tile words in two text rows.</summary>
    internal static IEnumerable<int> LanguageCells(int index)
    {
        var region = GameOptionsRomData.LanguagePaletteRegion(index);
        for (int word = 0; word < region.ByteCount / sizeof(ushort); word++)
            yield return region.ByteOffset / sizeof(ushort) + word;
    }

    /// <summary>$82:F149..F158 special-setting choice boxes: two rows of six words.</summary>
    internal static IEnumerable<int> ToggleCells(string name, bool enabled)
    {
        var layout = name switch
        {
            IconCancelToggle => GameOptionsRomData.SpecialToggles.IconCancel,
            MoonwalkToggle => GameOptionsRomData.SpecialToggles.Moonwalk,
            _ => throw new InvalidDataException($"Unknown options toggle {name}."),
        };
        int first = enabled ? layout.EnabledTop : layout.DisabledTop;
        int second = enabled ? layout.EnabledBottom : layout.DisabledBottom;
        for (int row = 0; row < 2; row++)
        for (int word = 0; word < GameOptionsRomData.SpecialToggles.PaletteRegionByteCount / sizeof(ushort); word++)
            yield return (row == 0 ? first : second) / sizeof(ushort) + word;
    }
    /// <summary>Returns the number of selection-row anchors required by a menu layout.</summary>
    /// <param name="page">The language-independent primary, controller, or special menu identity.</param>
    /// <returns>Five for primary, nine for controller including Exit and Reset, or three for special including Exit.</returns>
    /// <exception cref="ArgumentOutOfRangeException">The value is not one of the three menu identities.</exception>
    public static int CursorCount(string page) => page switch
    {
        PrimaryMenu => GameOptionsRomData.Rows.PrimaryCount,
        ControllerMenu => GameOptionsRomData.Rows.ControllerCount,
        SpecialMenu => GameOptionsRomData.Rows.SpecialCount,
        _ => throw new ArgumentOutOfRangeException(nameof(page)),
    };
}
