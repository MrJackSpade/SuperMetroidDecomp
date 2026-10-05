using System.Buffers.Binary;
using System.Security.Cryptography;
using System.Text.Json;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Assets;

/// <summary>
/// Immutable options-screen pages, dynamic label patches, highlight regions, actor
/// compositions and visual anchors. Navigation, binding swaps, toggles and fades remain
/// application mechanics.
/// </summary>
public sealed class GameOptionsPresentation
{
    private readonly Dictionary<string, byte[]> pages;
    private readonly Dictionary<string, ushort[]> controllerLabels;
    private readonly MapLabelPoint[]? controllerLabelAnchors;
    private readonly GameOptionsLanguageRegionDocument[]? languageRegions;
    private readonly Dictionary<string, GameOptionsToggleVisualDocument>? specialToggles;
    private readonly Dictionary<string, SpriteComposition> sprites;
    private readonly Dictionary<string, MapLabelPoint>? headingAnchors;
    private readonly Dictionary<string, MapLabelPoint[]>? cursorAnchors;

    private GameOptionsPresentation(
        Dictionary<string, byte[]> pages,
        Dictionary<string, ushort[]> controllerLabels,
        Dictionary<string, SpriteComposition> sprites,
        GameOptionsPresentationDocument document,
        string contentIdentity)
    {
        this.pages = pages;
        this.controllerLabels = controllerLabels.Where(pair => pair.Value.Where((word, cell) =>
            word != GameOptionsPresentationDefinitions.ControllerLabelWord(pair.Key, cell)).Any())
            .ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.Ordinal);
        this.sprites = sprites;
        controllerLabelAnchors = document.ControllerLabelAnchors.Where((point, index) =>
            point != GameOptionsPresentationDefinitions.ControllerAnchor(index)).Any() ? document.ControllerLabelAnchors : null;
        languageRegions = document.LanguageRegions.Where((region, index) =>
            region.HighlightWhenJapanese != GameOptionsRomData.LanguagePaletteRegion(index).HighlightWhenJapanese ||
            !region.Cells.SequenceEqual(GameOptionsPresentationDefinitions.LanguageCells(index))).Any()
            ? document.LanguageRegions : null;
        specialToggles = document.SpecialToggles.Any(pair =>
            !pair.Value.EnabledCells.SequenceEqual(GameOptionsPresentationDefinitions.ToggleCells(pair.Key, true)) ||
            !pair.Value.DisabledCells.SequenceEqual(GameOptionsPresentationDefinitions.ToggleCells(pair.Key, false)))
            ? document.SpecialToggles : null;
        headingAnchors = document.HeadingAnchors.Any(pair => pair.Value != GameOptionsPresentationDefinitions.HeadingAnchor(pair.Key))
            ? document.HeadingAnchors : null;
        cursorAnchors = document.CursorAnchors.Any(pair => pair.Value.Where((point, index) =>
            point != GameOptionsPresentationDefinitions.CursorAnchor(pair.Key, index)).Any()) ? document.CursorAnchors : null;
        HiddenCursor = document.HiddenCursor;
        SelectedPalette = document.SelectedPalette;
        UnselectedPalette = document.UnselectedPalette;
        CursorPalette = document.CursorPalette;
        CursorFrameDuration = document.CursorFrameDuration;
        ContentIdentity = contentIdentity;
    }

    public string ContentIdentity { get; }
    public MapLabelPoint HiddenCursor { get; }
    public int SelectedPalette { get; }
    public int UnselectedPalette { get; }
    public int CursorPalette { get; }
    public int CursorFrameDuration { get; }

    internal byte[] CreatePage(string name) =>
        pages.TryGetValue(name, out byte[]? page)
            ? (byte[])page.Clone()
            : throw new InvalidDataException($"Unknown options page {name}.");

    internal void LoadBackground(SnesVram vram) =>
        vram.LoadBytes(MenuPpuState.Bg2TilemapWord * sizeof(ushort),
            pages[GameOptionsPresentationDefinitions.BackgroundPage]);

    internal void ApplyLanguage(Span<byte> primaryPage, bool japanese)
    {
        if (languageRegions is null)
        {
            for (int index = 0; index < GameOptionsRomData.LanguagePaletteRegionCount; index++)
            {
                var region = GameOptionsRomData.LanguagePaletteRegion(index);
                ApplyPalette(primaryPage, GameOptionsPresentationDefinitions.LanguageCells(index),
                    japanese == region.HighlightWhenJapanese ? SelectedPalette : UnselectedPalette);
            }
            return;
        }
        foreach (GameOptionsLanguageRegionDocument region in languageRegions)
        {
            bool selected = japanese == region.HighlightWhenJapanese;
            ApplyPalette(primaryPage, region.Cells,
                selected ? SelectedPalette : UnselectedPalette);
        }
    }

    internal void ApplyControllerLabel(Span<byte> page, int action, int button)
    {
        if ((uint)action >= GameOptionsRomData.Rows.ControllerActionCount)
            throw new ArgumentOutOfRangeException(nameof(action));
        string label = GameOptionsPresentationDefinitions.ControllerLabelName(button);
        controllerLabels.TryGetValue(label, out ushort[]? cells);
        MapLabelPoint anchor = controllerLabelAnchors?[action] ?? GameOptionsPresentationDefinitions.ControllerAnchor(action);
        for (int row = 0; row < GameOptionsPresentationDefinitions.ControllerLabelHeight; row++)
        for (int column = 0; column < GameOptionsPresentationDefinitions.ControllerLabelWidth; column++)
        {
            int destination = (anchor.Y + row) * GameOptionsRomData.MenuTilemapWidth +
                anchor.X + column;
            BinaryPrimitives.WriteUInt16LittleEndian(
                page.Slice(destination * sizeof(ushort)),
                cells is not null ? cells[row * GameOptionsPresentationDefinitions.ControllerLabelWidth + column]
                    : GameOptionsPresentationDefinitions.ControllerLabelWord(label, row * GameOptionsPresentationDefinitions.ControllerLabelWidth + column));
        }
    }

    internal void ApplySpecialToggle(Span<byte> page, string name, bool enabled)
    {
        if (specialToggles is null)
        {
            ApplyPalette(page, GameOptionsPresentationDefinitions.ToggleCells(name, true),
                enabled ? SelectedPalette : UnselectedPalette);
            ApplyPalette(page, GameOptionsPresentationDefinitions.ToggleCells(name, false),
                enabled ? UnselectedPalette : SelectedPalette);
            return;
        }
        if (!specialToggles.TryGetValue(name, out GameOptionsToggleVisualDocument? toggle))
            throw new InvalidDataException($"Unknown options toggle {name}.");
        ApplyPalette(page, toggle.EnabledCells,
            enabled ? SelectedPalette : UnselectedPalette);
        ApplyPalette(page, toggle.DisabledCells,
            enabled ? UnselectedPalette : SelectedPalette);
    }

    internal MapLabelPoint CursorPosition(string page, int selectedItem)
    {
        if (cursorAnchors is null)
            return GameOptionsPresentationDefinitions.CursorAnchor(page, selectedItem);
        return cursorAnchors.TryGetValue(page, out MapLabelPoint[]? points) &&
            (uint)selectedItem < points.Length
            ? points[selectedItem]
            : throw new InvalidDataException($"Options cursor {page}[{selectedItem}] is not authored.");
    }
    internal void DrawHeading(OamBuffer oam, string page, int verticalScroll)
    {
        MapLabelPoint point = headingAnchors is null
            ? GameOptionsPresentationDefinitions.HeadingAnchor(page)
            : headingAnchors.TryGetValue(page, out MapLabelPoint? edited) ? edited
                : throw new InvalidDataException($"Options heading {page} is not authored.");
        sprites[GameOptionsPresentationDefinitions.HeadingFrameName(page)].DrawOnScreen(
            oam, checked((ushort)point.X), unchecked((ushort)(point.Y - verticalScroll)),
            PaletteBits(CursorPalette));
    }

    internal void DrawCursor(OamBuffer oam, int frame, MapLabelPoint point)
    {
        sprites[GameOptionsPresentationDefinitions.CursorFrameName(frame)].DrawOnScreen(
            oam, checked((ushort)point.X), checked((ushort)point.Y), PaletteBits(CursorPalette));
    }

    public static GameOptionsPresentation Load(Stream source)
    {
        byte[] bytes;
        using (var copy = new MemoryStream())
        {
            source.CopyTo(copy);
            bytes = copy.ToArray();
        }

        GameOptionsPresentationDocument document;
        try
        {
            document = JsonAssetDocument.Read<GameOptionsPresentationDocument>(
                bytes, MapPresentationFormat.JsonOptions) ??
                throw new InvalidDataException("Options presentation document is null.");
        }
        catch (JsonException error)
        {
            throw new InvalidDataException("Invalid options presentation JSON.", error);
        }

        if (document.Version != GameOptionsPresentationDefinitions.Version)
            throw new InvalidDataException(
                $"Options presentation version must be {GameOptionsPresentationDefinitions.Version}.");
        ValidateExactKeys(document.Pages, GameOptionsPresentationDefinitions.PageNames,
            "options pages");
        ValidateExactKeys(document.ControllerLabels,
            GameOptionsPresentationDefinitions.ControllerLabelNames,
            "options controller labels");
        ValidateExactKeys(document.SpecialToggles,
            GameOptionsPresentationDefinitions.SpecialToggleNames,
            "options special toggles");
        ValidateExactKeys(document.Sprites, GameOptionsPresentationDefinitions.SpriteNames,
            "options sprites");
        ValidateExactKeys(document.HeadingAnchors,
            GameOptionsPresentationDefinitions.MenuPageNames,
            "options heading anchors");
        ValidateExactKeys(document.CursorAnchors,
            GameOptionsPresentationDefinitions.MenuPageNames,
            "options cursor anchors");
        if (document.ControllerLabelAnchors is null ||
            document.ControllerLabelAnchors.Length != GameOptionsRomData.Rows.ControllerActionCount)
            throw new InvalidDataException("Options require seven controller-label anchors.");
        if (document.LanguageRegions is null || document.LanguageRegions.Length != 4)
            throw new InvalidDataException("Options require four primary-language highlight regions.");
        ValidatePalette(document.SelectedPalette, nameof(document.SelectedPalette));
        ValidatePalette(document.UnselectedPalette, nameof(document.UnselectedPalette));
        ValidatePalette(document.CursorPalette, nameof(document.CursorPalette));
        if (document.SelectedPalette == document.UnselectedPalette)
            throw new InvalidDataException("Options selected and unselected palettes must differ.");
        if (document.CursorFrameDuration is < 1 or > ushort.MaxValue)
            throw new InvalidDataException("Options cursor frame duration must be 1..65535.");
        ValidatePoint(document.HiddenCursor, "hidden cursor", allowOffscreenX: true);

        var pages = new Dictionary<string, byte[]>(StringComparer.Ordinal);
        foreach (string name in GameOptionsPresentationDefinitions.PageNames)
        {
            MapPresentationCell[] cells = document.Pages[name];
            if (cells is null || cells.Length != GameOptionsPresentationDefinitions.PageCellCount)
                throw new InvalidDataException($"Options page {name} requires 1024 cells.");
            pages.Add(name, CompileCells(cells, $"options page {name}"));
        }

        var labels = new Dictionary<string, ushort[]>(StringComparer.Ordinal);
        foreach (string name in GameOptionsPresentationDefinitions.ControllerLabelNames)
        {
            MapPresentationCell[] cells = document.ControllerLabels[name];
            if (cells is null || cells.Length != GameOptionsPresentationDefinitions.ControllerLabelCellCount)
                throw new InvalidDataException($"Options controller label {name} requires six cells.");
            byte[] compiled = CompileCells(cells, $"options controller label {name}");
            var words = new ushort[cells.Length];
            for (int index = 0; index < words.Length; index++)
                words[index] = BinaryPrimitives.ReadUInt16LittleEndian(
                    compiled.AsSpan(index * sizeof(ushort)));
            labels.Add(name, words);
        }

        foreach (MapLabelPoint? anchor in document.ControllerLabelAnchors)
            ValidateTilePoint(anchor, "controller-label anchor",
                GameOptionsPresentationDefinitions.ControllerLabelWidth,
                GameOptionsPresentationDefinitions.ControllerLabelHeight);
        var claimedLanguageCells = new HashSet<int>();
        foreach (GameOptionsLanguageRegionDocument? region in document.LanguageRegions)
        {
            if (region is null || region.Cells is null || region.Cells.Length == 0)
                throw new InvalidDataException("Each options language region requires cells.");
            ValidateCellSet(region.Cells, claimedLanguageCells, "language region");
        }
        foreach (string name in GameOptionsPresentationDefinitions.SpecialToggleNames)
        {
            GameOptionsToggleVisualDocument? toggle = document.SpecialToggles[name];
            if (toggle is null || toggle.EnabledCells is null || toggle.DisabledCells is null ||
                toggle.EnabledCells.Length == 0 || toggle.DisabledCells.Length == 0)
                throw new InvalidDataException($"Options toggle {name} requires enabled and disabled cells.");
            var claimed = new HashSet<int>();
            ValidateCellSet(toggle.EnabledCells, claimed, $"{name} enabled");
            ValidateCellSet(toggle.DisabledCells, claimed, $"{name} disabled");
        }

        var sprites = new Dictionary<string, SpriteComposition>(StringComparer.Ordinal);
        foreach (string name in GameOptionsPresentationDefinitions.SpriteNames)
            sprites.Add(name, MenuCursorParts.CalculateIfMatching(name,
                MenuSpriteCompiler.Compile(document.Sprites[name], $"options {name}")));
        foreach (string page in GameOptionsPresentationDefinitions.MenuPageNames)
        {
            ValidatePoint(document.HeadingAnchors[page], $"{page} heading", allowOffscreenX: false);
            int expected = GameOptionsPresentationDefinitions.CursorCount(page);
            MapLabelPoint[]? points = document.CursorAnchors[page];
            if (points is null || points.Length != expected)
                throw new InvalidDataException($"Options {page} requires {expected} cursor anchors.");
            foreach (MapLabelPoint? point in points)
                ValidatePoint(point, $"{page} cursor", allowOffscreenX: false);
        }

        return new(pages, labels, sprites, document,
            Convert.ToHexString(SHA256.HashData(bytes)));
    }

    public static void Write(Stream output, GameOptionsPresentationDocument document)
    {
        byte[] bytes = JsonSerializer.SerializeToUtf8Bytes(document, MapPresentationFormat.JsonOptions);
        _ = Load(new MemoryStream(bytes, writable: false));
        output.Write(bytes);
    }

    private static byte[] CompileCells(MapPresentationCell[] cells, string owner)
    {
        var bytes = new byte[cells.Length * sizeof(ushort)];
        for (int index = 0; index < cells.Length; index++)
        {
            MapPresentationCell? cell = cells[index];
            if (cell is null ||
                (uint)cell.TileColumn >= MapTileAtlasFormat.TileColumns ||
                (uint)cell.TileRow >= GameOptionsPresentationDefinitions.CharacterRows ||
                (uint)cell.Palette >= MapPresentationFormat.PaletteCount)
                throw new InvalidDataException(
                    $"{owner} cell {index} has an invalid atlas coordinate or palette.");
            ushort word = checked((ushort)(
                cell.TileRow * MapTileAtlasFormat.TileColumns + cell.TileColumn |
                cell.Palette << MapPresentationFormat.PaletteShift |
                (cell.Priority ? MapPresentationFormat.PriorityBit : 0) |
                (cell.FlipX ? MapPresentationFormat.FlipXBit : 0) |
                (cell.FlipY ? MapPresentationFormat.FlipYBit : 0)));
            BinaryPrimitives.WriteUInt16LittleEndian(bytes.AsSpan(index * sizeof(ushort)), word);
        }
        return bytes;
    }

    private static void ApplyPalette(Span<byte> page, IEnumerable<int> cells, int palette)
    {
        foreach (int cell in cells)
        {
            int offset = cell * sizeof(ushort);
            var word = new SnesBgTilemapWord(
                BinaryPrimitives.ReadUInt16LittleEndian(page[offset..]));
            BinaryPrimitives.WriteUInt16LittleEndian(page[offset..],
                word.WithPaletteIndex(palette).Raw);
        }
    }

    private static void ValidateExactKeys<T>(Dictionary<string, T>? values,
        ReadOnlySpan<string> expected, string owner)
    {
        if (values is null || values.Count != expected.Length)
            throw new InvalidDataException($"{owner} requires exactly {expected.Length} named entries.");
        foreach (string name in expected)
            if (!values.ContainsKey(name))
                throw new InvalidDataException($"{owner} is missing {name}.");
    }

    private static void ValidateCellSet(int[] cells, HashSet<int> claimed, string owner)
    {
        foreach (int cell in cells)
            if ((uint)cell >= GameOptionsPresentationDefinitions.PageCellCount || !claimed.Add(cell))
                throw new InvalidDataException($"Options {owner} contains an invalid or duplicate cell {cell}.");
    }

    private static void ValidatePoint(MapLabelPoint? point, string owner, bool allowOffscreenX)
    {
        int maximumX = allowOffscreenX ? 0x1ff : byte.MaxValue;
        if (point is null || point.X < 0 || point.X > maximumX || (uint)point.Y > byte.MaxValue)
            throw new InvalidDataException(
                $"Options {owner} requires X=0..{maximumX} and Y=0..255.");
    }

    private static void ValidateTilePoint(MapLabelPoint? point, string owner, int width, int height)
    {
        if (point is null || point.X < 0 || point.Y < 0 ||
            point.X + width > GameOptionsRomData.MenuTilemapWidth ||
            point.Y + height > GameOptionsRomData.MenuTilemapHeight)
            throw new InvalidDataException($"Options {owner} escapes the 32x32 page.");
    }

    private static void ValidatePalette(int palette, string owner)
    {
        if ((uint)palette >= MapPresentationFormat.PaletteCount)
            throw new InvalidDataException($"Options {owner} must be 0..7.");
    }

    private static ushort PaletteBits(int index) =>
        SnesObjAttributeWord.Create(0, index, 0).PaletteBits;
}

public sealed record GameOptionsPresentationDocument
{
    public required int Version { get; init; }
    public required Dictionary<string, MapPresentationCell[]> Pages { get; init; }
    public required Dictionary<string, MapPresentationCell[]> ControllerLabels { get; init; }
    public required MapLabelPoint[] ControllerLabelAnchors { get; init; }
    public required GameOptionsLanguageRegionDocument[] LanguageRegions { get; init; }
    public required Dictionary<string, GameOptionsToggleVisualDocument> SpecialToggles { get; init; }
    public required Dictionary<string, SpriteVisualPart[]> Sprites { get; init; }
    public required Dictionary<string, MapLabelPoint> HeadingAnchors { get; init; }
    public required Dictionary<string, MapLabelPoint[]> CursorAnchors { get; init; }
    public required MapLabelPoint HiddenCursor { get; init; }
    public required int SelectedPalette { get; init; }
    public required int UnselectedPalette { get; init; }
    public required int CursorPalette { get; init; }
    public required int CursorFrameDuration { get; init; }
}

public sealed record GameOptionsLanguageRegionDocument
{
    public required int[] Cells { get; init; }
    public required bool HighlightWhenJapanese { get; init; }
}

public sealed record GameOptionsToggleVisualDocument
{
    public required int[] EnabledCells { get; init; }
    public required int[] DisabledCells { get; init; }
}

/// <summary>Schema names and geometry for <c>options-menu.json</c>.</summary>
public static class GameOptionsPresentationDefinitions
{
    public const int Version = 1;
    public const string FileName = "options-menu.json";
    public const string BackgroundPage = "Background";
    public const string PrimaryPage = "Primary";
    public const string ControllerEnglishPage = "Controller.English";
    public const string ControllerJapanesePage = "Controller.Japanese";
    public const string SpecialEnglishPage = "Special.English";
    public const string SpecialJapanesePage = "Special.Japanese";
    public const string PrimaryMenu = "Primary";
    public const string ControllerMenu = "Controller";
    public const string SpecialMenu = "Special";
    public const string IconCancelToggle = "IconCancel";
    public const string MoonwalkToggle = "Moonwalk";
    public const int PageCellCount = GameOptionsRomData.MenuTilemapWidth * GameOptionsRomData.MenuTilemapHeight;
    public const int CharacterRows = 32;
    public const int ControllerLabelWidth = GameOptionsRomData.ControllerLabels.WidthInTiles;
    public const int ControllerLabelHeight = GameOptionsRomData.ControllerLabels.HeightInTiles;
    public const int ControllerLabelCellCount = ControllerLabelWidth * ControllerLabelHeight;

    private static readonly string[] pageNames =
        [BackgroundPage, PrimaryPage, ControllerEnglishPage, ControllerJapanesePage,
            SpecialEnglishPage, SpecialJapanesePage];
    private static readonly string[] menuPageNames =
        [PrimaryMenu, ControllerMenu, SpecialMenu];
    private static readonly string[] controllerLabelNames =
        ["X", "A", "B", "Select", "Y", "L", "R"];
    private static readonly string[] specialToggleNames =
        [IconCancelToggle, MoonwalkToggle];
    private static readonly string[] spriteNames =
        ["Heading.Primary", "Heading.Controller", "Heading.Special",
            "Cursor.0", "Cursor.1", "Cursor.2", "Cursor.3"];

    public static ReadOnlySpan<string> PageNames => pageNames;
    public static ReadOnlySpan<string> MenuPageNames => menuPageNames;
    public static ReadOnlySpan<string> ControllerLabelNames => controllerLabelNames;
    public static ReadOnlySpan<string> SpecialToggleNames => specialToggleNames;
    public static ReadOnlySpan<string> SpriteNames => spriteNames;

    public static string ControllerLabelName(int index) =>
        (uint)index < controllerLabelNames.Length
            ? controllerLabelNames[index]
            : throw new ArgumentOutOfRangeException(nameof(index));

    public static string CursorFrameName(int index) => index switch
    {
        0 => "Cursor.0",
        1 => "Cursor.1",
        2 => "Cursor.2",
        3 => "Cursor.3",
        _ => throw new ArgumentOutOfRangeException(nameof(index)),
    };

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
    internal static MapLabelPoint ControllerAnchor(int action)
    {
        int word = GameOptionsRomData.ControllerLabels.Destination(action) / sizeof(ushort);
        return new(word % GameOptionsRomData.MenuTilemapWidth, word / GameOptionsRomData.MenuTilemapWidth);
    }

    /// <summary>$82:F307/F31B/F33F selection missile rows, including controller scroll displacement.</summary>
    internal static MapLabelPoint CursorAnchor(string page, int row)
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
    internal static MapLabelPoint HeadingAnchor(string page)
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
    public static int CursorCount(string page) => page switch
    {
        PrimaryMenu => GameOptionsRomData.Rows.PrimaryCount,
        ControllerMenu => GameOptionsRomData.Rows.ControllerCount,
        SpecialMenu => GameOptionsRomData.Rows.SpecialCount,
        _ => throw new ArgumentOutOfRangeException(nameof(page)),
    };
}
