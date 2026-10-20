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

    /// <summary>The foreground option pages in document order: primary, then English and Japanese controller and special pages.</summary>
    public static IReadOnlyList<GameOptionsTilemap> Tilemaps { get; } = Enum.GetValues<GameOptionsTilemap>();
    /// <summary>The three menu layouts headings and cursors are anchored by, in primary, controller and special order.</summary>
    internal static IReadOnlyList<GameOptionsPage> MenuPages { get; } = Enum.GetValues<GameOptionsPage>();
    /// <summary>The assignable buttons whose glyphs the controller page shows, in native selector order: X, A, B, Select, Y, L, R.</summary>
    public static IReadOnlyList<SnesButton> ControllerLabelButtons { get; } =
        [SnesButton.X, SnesButton.A, SnesButton.B, SnesButton.Select, SnesButton.Y, SnesButton.L, SnesButton.R];
    /// <summary>The two special settings, icon cancel then moonwalk.</summary>
    public static IReadOnlyList<GameOptionsToggle> Toggles { get; } = Enum.GetValues<GameOptionsToggle>();
    /// <summary>The missile cursor's looping animation frames.</summary>
    public const int CursorFrameCount = 4;
    /// <summary>The JSON names of the cursor frame sprites, by frame.</summary>
    private static readonly string[] cursorSpriteNames = ["Cursor.0", "Cursor.1", "Cursor.2", "Cursor.3"];

    /// <summary>The JSON name of a foreground page.</summary>
    public static string TilemapName(GameOptionsTilemap tilemap) => tilemap switch
    {
        GameOptionsTilemap.Primary => "Primary",
        GameOptionsTilemap.ControllerEnglish => "Controller.English",
        GameOptionsTilemap.ControllerJapanese => "Controller.Japanese",
        GameOptionsTilemap.SpecialEnglish => "Special.English",
        GameOptionsTilemap.SpecialJapanese => "Special.Japanese",
        _ => throw new ArgumentOutOfRangeException(nameof(tilemap)),
    };

    /// <summary>The JSON name of a menu layout, which keys its heading and cursor anchors.</summary>
    internal static string MenuName(GameOptionsPage page) => page switch
    {
        GameOptionsPage.Primary => "Primary",
        GameOptionsPage.Controller => "Controller",
        GameOptionsPage.Special => "Special",
        _ => throw new ArgumentOutOfRangeException(nameof(page)),
    };

    /// <summary>The JSON name of a menu layout's heading sprite.</summary>
    internal static string HeadingSpriteName(GameOptionsPage page) => page switch
    {
        GameOptionsPage.Primary => "Heading.Primary",
        GameOptionsPage.Controller => "Heading.Controller",
        GameOptionsPage.Special => "Heading.Special",
        _ => throw new ArgumentOutOfRangeException(nameof(page)),
    };

    /// <summary>The menu heading a menu layout draws.</summary>
    internal static MenuHeading Heading(GameOptionsPage page) => page switch
    {
        GameOptionsPage.Primary => MenuHeading.OptionsPrimary,
        GameOptionsPage.Controller => MenuHeading.OptionsController,
        GameOptionsPage.Special => MenuHeading.OptionsSpecial,
        _ => throw new ArgumentOutOfRangeException(nameof(page)),
    };

    /// <summary>The JSON name of a looping missile-cursor frame's sprite, for frames 0-3.</summary>
    public static string CursorSpriteName(int frame) =>
        (uint)frame < cursorSpriteNames.Length ? cursorSpriteNames[frame] : throw new ArgumentOutOfRangeException(nameof(frame));

    /// <summary>The JSON name of an assignable button's glyph.</summary>
    public static string ControllerLabelName(SnesButton button) => button switch
    {
        SnesButton.X => "X",
        SnesButton.A => "A",
        SnesButton.B => "B",
        SnesButton.Select => "Select",
        SnesButton.Y => "Y",
        SnesButton.L => "L",
        SnesButton.R => "R",
        SnesButton.None or SnesButton.Right or SnesButton.Left or SnesButton.Down or SnesButton.Up or SnesButton.Start =>
            throw new ArgumentOutOfRangeException(nameof(button), $"{button} is not an assignable button."),
        _ => throw new ArgumentOutOfRangeException(nameof(button)),
    };

    /// <summary>The JSON name of a special setting's visual.</summary>
    public static string ToggleName(GameOptionsToggle toggle) => toggle switch
    {
        GameOptionsToggle.IconCancel => "IconCancel",
        GameOptionsToggle.Moonwalk => "Moonwalk",
        _ => throw new ArgumentOutOfRangeException(nameof(toggle)),
    };

    /// <summary>The assignable button a native selector names.</summary>
    /// <param name="index">Zero-based index 0-6 in X, A, B, Select, Y, L, R order.</param>
    /// <exception cref="ArgumentOutOfRangeException">The index is outside 0-6.</exception>
    public static SnesButton ControllerLabelButton(int index) =>
        (uint)index < ControllerLabelButtons.Count ? ControllerLabelButtons[index] : throw new ArgumentOutOfRangeException(nameof(index));

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
    internal static ushort ControllerLabelWord(SnesButton label, int cell)
    {
        if ((uint)cell >= ControllerLabelCellCount) throw new ArgumentOutOfRangeException(nameof(cell));
        int row = cell / ControllerLabelWidth;
        int column = cell % ControllerLabelWidth;
        int tile;
        SnesTileFlipFlags flips = SnesTileFlipFlags.None;
        switch (label)
        {
            case SnesButton.A:
            case SnesButton.Y:
                tile = column == 2 ? ButtonPaddingTile : (label == SnesButton.A ? AButtonTile : YButtonTile) + row * ButtonAtlasRowStride;
                if (column == 1) flips = SnesTileFlipFlags.Horizontal;
                break;
            case SnesButton.X:
                tile = column == 2 ? ButtonPaddingTile : XButtonTile + (row ^ column) * ButtonAtlasRowStride;
                if (column == 1) flips = SnesTileFlipFlags.Horizontal | SnesTileFlipFlags.Vertical;
                break;
            case SnesButton.B:
                tile = column == 2 ? ButtonPaddingTile : BButtonTile + column + row * ButtonAtlasRowStride;
                break;
            case SnesButton.Select:
                tile = SelectButtonTile + column + row * ButtonAtlasRowStride;
                break;
            case SnesButton.L:
            case SnesButton.R:
                tile = column == 1 ? (label == SnesButton.L ? LeftShoulderTile : RightShoulderTile) + row * ButtonAtlasRowStride : ShoulderCornerTile;
                if (column != 1)
                    flips = (row == 1 ? SnesTileFlipFlags.Vertical : SnesTileFlipFlags.None)
                        | (column == 2 ? SnesTileFlipFlags.Horizontal : SnesTileFlipFlags.None);
                break;
            case SnesButton.None:
            case SnesButton.Right:
            case SnesButton.Left:
            case SnesButton.Down:
            case SnesButton.Up:
            case SnesButton.Start:
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
    internal static Position16<OamPixels> CursorAnchor(GameOptionsPage page, int row)
    {
        if ((uint)row >= CursorCount(page))
            throw new InvalidDataException($"Options cursor {page}[{row}] is not authored.");
        return page switch
        {
            GameOptionsPage.Primary => new(GameOptionsRomData.Cursors.PrimaryX, GameOptionsRomData.Cursors.PrimaryY(row)),
            GameOptionsPage.Controller => new(GameOptionsRomData.Cursors.ControllerX, GameOptionsRomData.Cursors.ControllerY(row)),
            GameOptionsPage.Special => new(GameOptionsRomData.Cursors.SpecialX, GameOptionsRomData.Cursors.SpecialY(row)),
            _ => throw new ArgumentOutOfRangeException(nameof(page)),
        };
    }

    /// <summary>$82:F34B/F353/F35B and F369 named page heading setup anchors.</summary>
    internal static Position16<OamPixels> HeadingAnchor(GameOptionsPage page) =>
        new(GameOptionsRomData.Spritemaps.HeadingX(page), GameOptionsRomData.Spritemaps.HeadingY);

    /// <summary>$82:EDF2..EE51 language highlight calls cover consecutive tile words in two text rows.</summary>
    internal static IEnumerable<int> LanguageCells(int index)
    {
        var region = GameOptionsRomData.LanguagePaletteRegion(index);
        for (int word = 0; word < region.ByteCount / sizeof(ushort); word++)
            yield return region.ByteOffset / sizeof(ushort) + word;
    }

    /// <summary>$82:F149..F158 special-setting choice boxes: two rows of six words.</summary>
    internal static IEnumerable<int> ToggleCells(GameOptionsToggle toggle, GameOptionsToggleState state)
    {
        var layout = toggle switch
        {
            GameOptionsToggle.IconCancel => GameOptionsRomData.SpecialToggles.IconCancel,
            GameOptionsToggle.Moonwalk => GameOptionsRomData.SpecialToggles.Moonwalk,
            _ => throw new ArgumentOutOfRangeException(nameof(toggle)),
        };
        int first = state == GameOptionsToggleState.Enabled ? layout.EnabledTop : layout.DisabledTop;
        int second = state == GameOptionsToggleState.Enabled ? layout.EnabledBottom : layout.DisabledBottom;
        for (int row = 0; row < 2; row++)
        for (int word = 0; word < GameOptionsRomData.SpecialToggles.PaletteRegionByteCount / sizeof(ushort); word++)
            yield return (row == 0 ? first : second) / sizeof(ushort) + word;
    }
    /// <summary>Returns the number of selection-row anchors required by a menu layout.</summary>
    /// <returns>Five for primary, nine for controller including Exit and Reset, or three for special including Exit.</returns>
    internal static int CursorCount(GameOptionsPage page) => page switch
    {
        GameOptionsPage.Primary => GameOptionsRomData.Rows.PrimaryCount,
        GameOptionsPage.Controller => GameOptionsRomData.Rows.ControllerCount,
        GameOptionsPage.Special => GameOptionsRomData.Rows.SpecialCount,
        _ => throw new ArgumentOutOfRangeException(nameof(page)),
    };
}
