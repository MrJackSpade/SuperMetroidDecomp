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
    private readonly MapLabelPoint[] controllerLabelAnchors;
    private readonly GameOptionsLanguageRegionDocument[] languageRegions;
    private readonly Dictionary<string, GameOptionsToggleVisualDocument> specialToggles;
    private readonly Dictionary<string, SpriteComposition> sprites;
    private readonly Dictionary<string, MapLabelPoint> headingAnchors;
    private readonly Dictionary<string, MapLabelPoint[]> cursorAnchors;

    private GameOptionsPresentation(
        Dictionary<string, byte[]> pages,
        Dictionary<string, ushort[]> controllerLabels,
        Dictionary<string, SpriteComposition> sprites,
        GameOptionsPresentationDocument document,
        string contentIdentity)
    {
        this.pages = pages;
        this.controllerLabels = controllerLabels;
        this.sprites = sprites;
        controllerLabelAnchors = document.ControllerLabelAnchors;
        languageRegions = document.LanguageRegions;
        specialToggles = document.SpecialToggles;
        headingAnchors = document.HeadingAnchors;
        cursorAnchors = document.CursorAnchors;
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
        foreach (GameOptionsLanguageRegionDocument region in languageRegions)
        {
            bool selected = japanese == region.HighlightWhenJapanese;
            ApplyPalette(primaryPage, region.Cells,
                selected ? SelectedPalette : UnselectedPalette);
        }
    }

    internal void ApplyControllerLabel(Span<byte> page, int action, int button)
    {
        if ((uint)action >= controllerLabelAnchors.Length)
            throw new ArgumentOutOfRangeException(nameof(action));
        string label = GameOptionsPresentationDefinitions.ControllerLabelName(button);
        ushort[] cells = controllerLabels[label];
        MapLabelPoint anchor = controllerLabelAnchors[action];
        for (int row = 0; row < GameOptionsPresentationDefinitions.ControllerLabelHeight; row++)
        for (int column = 0; column < GameOptionsPresentationDefinitions.ControllerLabelWidth; column++)
        {
            int destination = (anchor.Y + row) * GameOptionsRomData.MenuTilemapWidth +
                anchor.X + column;
            BinaryPrimitives.WriteUInt16LittleEndian(
                page.Slice(destination * sizeof(ushort)),
                cells[row * GameOptionsPresentationDefinitions.ControllerLabelWidth + column]);
        }
    }

    internal void ApplySpecialToggle(Span<byte> page, string name, bool enabled)
    {
        if (!specialToggles.TryGetValue(name, out GameOptionsToggleVisualDocument? toggle))
            throw new InvalidDataException($"Unknown options toggle {name}.");
        ApplyPalette(page, toggle.EnabledCells,
            enabled ? SelectedPalette : UnselectedPalette);
        ApplyPalette(page, toggle.DisabledCells,
            enabled ? UnselectedPalette : SelectedPalette);
    }

    internal MapLabelPoint CursorPosition(string page, int selectedItem) =>
        cursorAnchors.TryGetValue(page, out MapLabelPoint[]? points) &&
        (uint)selectedItem < points.Length
            ? points[selectedItem]
            : throw new InvalidDataException(
                $"Options cursor {page}[{selectedItem}] is not authored.");

    internal void DrawHeading(OamBuffer oam, string page, int verticalScroll)
    {
        if (!headingAnchors.TryGetValue(page, out MapLabelPoint? point))
            throw new InvalidDataException($"Options heading {page} is not authored.");
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
            document = JsonSerializer.Deserialize<GameOptionsPresentationDocument>(
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
            sprites.Add(name, MenuSpriteCompiler.Compile(document.Sprites[name], $"options {name}"));
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

    private static void ApplyPalette(Span<byte> page, int[] cells, int palette)
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

    public static int CursorCount(string page) => page switch
    {
        PrimaryMenu => GameOptionsRomData.Rows.PrimaryCount,
        ControllerMenu => GameOptionsRomData.Rows.ControllerCount,
        SpecialMenu => GameOptionsRomData.Rows.SpecialCount,
        _ => throw new ArgumentOutOfRangeException(nameof(page)),
    };
}
