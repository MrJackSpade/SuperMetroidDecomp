using System.Buffers.Binary;
using System.Security.Cryptography;
using System.Text.Json;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Frontend.Menus;
using SuperMetroid.Core.Frontend.Menus.Artwork;
using SuperMetroid.Core.Map.Artwork;

namespace SuperMetroid.Core.Frontend.Options.Artwork;

/// <summary>
/// Immutable options-screen pages, dynamic label patches, highlight regions, actor
/// compositions and visual anchors. Navigation, binding swaps, toggles and fades remain
/// application mechanics.
/// </summary>
public sealed class GameOptionsPresentation
{
    /// <summary>Compiled BG tilemap bytes for each named options page.</summary>
    private readonly Dictionary<string, byte[]> pages;
    /// <summary>Controller glyph cells retained only when they differ from stock definitions.</summary>
    internal readonly Dictionary<string, ushort[]> controllerLabels;
    /// <summary>Edited controller glyph origins, or null when all stock anchors apply.</summary>
    internal readonly Position16<TilemapCells>[]? controllerLabelAnchors;
    /// <summary>Edited language highlight regions, or null for the stock layout.</summary>
    internal readonly GameOptionsLanguageRegionDocument[]? languageRegions;
    /// <summary>Edited toggle highlight cells, or null for stock cell regions.</summary>
    internal readonly Dictionary<string, GameOptionsToggleVisualDocument>? specialToggles;
    /// <summary>Compiled heading and cursor sprite compositions by authored identity.</summary>
    internal readonly Dictionary<string, SpriteComposition> sprites;
    /// <summary>Edited heading origins, or null for native positions.</summary>
    internal readonly Dictionary<string, Position16<OamPixels>>? headingAnchors;
    /// <summary>Edited cursor origins, or null for native positions.</summary>
    internal readonly Dictionary<string, Position16<OamPixels>[]>? cursorAnchors;

    /// <summary>Retains compiled pages and only visual edits that depart from stock geometry.</summary>
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

    /// <summary>Uppercase SHA-256 digest of the loaded JSON bytes, identifying selected artwork and layout including their encoding.</summary>
    public string ContentIdentity { get; }
    /// <summary>Cursor origin used during phases without a cursor table, in screen pixels with X=0-511 and Y=0-255; the actor remains in OAM.</summary>
    public Position16<OamPixels> HiddenCursor { get; }
    /// <summary>BG palette index 0-7 applied to the currently chosen language and special-toggle text regions.</summary>
    public int SelectedPalette { get; }
    /// <summary>BG palette index 0-7 applied to the other language and toggle choices; differs from <see cref="SelectedPalette"/>.</summary>
    public int UnselectedPalette { get; }
    /// <summary>OBJ palette index 0-7 used for both the missile cursor and menu-heading sprite compositions.</summary>
    public int CursorPalette { get; }
    /// <summary>Menu updates per frame of the four-frame looping missile cursor, in the range 1-65535.</summary>
    public int CursorFrameDuration { get; }

    /// <summary>Returns an independent mutable copy of one compiled foreground or background page.</summary>
    internal byte[] CreatePage(string name) =>
        pages.TryGetValue(name, out byte[]? page)
            ? (byte[])page.Clone()
            : throw new InvalidDataException($"Unknown options page {name}.");

    /// <summary>Uploads the common BG2 background page into menu VRAM.</summary>
    internal void LoadBackground(SnesVram vram) =>
        new MenuVram(vram).Bg2Tilemap.Load(pages[GameOptionsPresentationDefinitions.BackgroundPage]);

    /// <summary>Recolors English and Japanese text regions according to the selected language.</summary>
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

    /// <summary>Writes the selected button glyph into one controller action's two-row label area.</summary>
    internal void ApplyControllerLabel(Span<byte> page, int action, int button)
    {
        if ((uint)action >= GameOptionsRomData.Rows.ControllerActionCount)
            throw new ArgumentOutOfRangeException(nameof(action));
        string label = GameOptionsPresentationDefinitions.ControllerLabelName(button);
        controllerLabels.TryGetValue(label, out ushort[]? cells);
        Position16<TilemapCells> anchor = controllerLabelAnchors?[action] ?? GameOptionsPresentationDefinitions.ControllerAnchor(action);
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

    /// <summary>Recolors the enabled and disabled labels for one special setting.</summary>
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

    /// <summary>Gets the cursor origin for an authored page and selection row.</summary>
    internal Position16<OamPixels> CursorPosition(string page, int selectedItem)
    {
        if (cursorAnchors is null)
            return GameOptionsPresentationDefinitions.CursorAnchor(page, selectedItem);
        return cursorAnchors.TryGetValue(page, out Position16<OamPixels>[]? points) &&
            (uint)selectedItem < points.Length
            ? points[selectedItem]
            : throw new InvalidDataException($"Options cursor {page}[{selectedItem}] is not authored.");
    }
    /// <summary>Draws a page heading at its authored origin after applying menu scroll.</summary>
    internal void DrawHeading(OamBuffer oam, string page, int verticalScroll)
    {
        Position16<OamPixels> point = headingAnchors is null
            ? GameOptionsPresentationDefinitions.HeadingAnchor(page)
            : headingAnchors.TryGetValue(page, out Position16<OamPixels> edited) ? edited
                : throw new InvalidDataException($"Options heading {page} is not authored.");
        // The authored X is validated on screen; scrolling the heading wraps its Y word.
        Position16<OamPixels> origin = new(checked((ushort)point.X), unchecked((ushort)(point.Y - verticalScroll)));
        sprites[GameOptionsPresentationDefinitions.HeadingFrameName(page)].DrawOnScreen(
            oam, origin,
            PaletteBits(CursorPalette));
    }

    /// <summary>Draws one frame of the missile cursor at the selected OAM position.</summary>
    internal void DrawCursor(OamBuffer oam, int frame, Position16<OamPixels> point)
    {
        sprites[GameOptionsPresentationDefinitions.CursorFrameName(frame)].DrawOnScreen(
            oam, point, PaletteBits(CursorPalette));
    }

    /// <summary>Reads a complete options-menu JSON document and compiles validated pages, controller labels, highlights, and sprite layouts.</summary>
    /// <param name="source">Readable stream consumed from its current position to the end and left open.</param>
    /// <returns>A presentation owning independently parsed layout and compiled artwork; stock-matching values are reconstructed from definitions.</returns>
    /// <exception cref="InvalidDataException">The JSON, version, required identities, cells, anchors, palette selectors, or cursor duration are invalid.</exception>
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
                bytes, ArtworkJson.Options) ??
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

        foreach (Position16<TilemapCells> anchor in document.ControllerLabelAnchors)
            ValidateTilePoint(anchor, "controller-label anchor", GameOptionsPresentationDefinitions.ControllerLabelSize);
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
            sprites.Add(name, MenuHeadingBorderDefinitions.CalculateIfMatching(name, MenuBorderParts.CalculateIfMatching(name, MenuCursorParts.CalculateIfMatching(name,
                MenuSpriteCompiler.Compile(document.Sprites[name], $"options {name}")))));
        foreach (string page in GameOptionsPresentationDefinitions.MenuPageNames)
        {
            ValidatePoint(document.HeadingAnchors[page], $"{page} heading", allowOffscreenX: false);
            int expected = GameOptionsPresentationDefinitions.CursorCount(page);
            Position16<OamPixels>[]? points = document.CursorAnchors[page];
            if (points is null || points.Length != expected)
                throw new InvalidDataException($"Options {page} requires {expected} cursor anchors.");
            foreach (Position16<OamPixels> point in points)
                ValidatePoint(point, $"{page} cursor", allowOffscreenX: false);
        }

        return new(pages, labels, sprites, document,
            Convert.ToHexString(SHA256.HashData(bytes)));
    }

    /// <summary>Serializes an authored document, validating it with the loader before writing any output bytes.</summary>
    /// <param name="output">Writable stream receiving UTF-8 JSON at its current position; left open.</param>
    /// <param name="document">Authored artwork and layout, whose collections are not modified by serialization or validation.</param>
    /// <exception cref="InvalidDataException">The serialized document fails options-menu presentation validation.</exception>
    public static void Write(Stream output, GameOptionsPresentationDocument document)
    {
        byte[] bytes = JsonSerializer.SerializeToUtf8Bytes(document, ArtworkJson.Options);
        _ = Load(new MemoryStream(bytes, writable: false));
        output.Write(bytes);
    }

    /// <summary>Validates authored atlas cells and packs them into little-endian BG tilemap words.</summary>
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

    /// <summary>Changes only palette bits of selected tilemap cells, preserving their artwork and flips.</summary>
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

    /// <summary>Requires exactly the names prescribed for one options document collection.</summary>
    private static void ValidateExactKeys<T>(Dictionary<string, T>? values,
        ReadOnlySpan<string> expected, string owner)
    {
        if (values is null || values.Count != expected.Length)
            throw new InvalidDataException($"{owner} requires exactly {expected.Length} named entries.");
        foreach (string name in expected)
            if (!values.ContainsKey(name))
                throw new InvalidDataException($"{owner} is missing {name}.");
    }

    /// <summary>Rejects out-of-range or overlapping highlight cells.</summary>
    private static void ValidateCellSet(int[] cells, HashSet<int> claimed, string owner)
    {
        foreach (int cell in cells)
            if ((uint)cell >= GameOptionsPresentationDefinitions.PageCellCount || !claimed.Add(cell))
                throw new InvalidDataException($"Options {owner} contains an invalid or duplicate cell {cell}.");
    }

    /// <summary>Validates an OAM origin, allowing hidden cursor X positions when requested.</summary>
    private static void ValidatePoint(Position16<OamPixels> point, string owner, bool allowOffscreenX)
    {
        int maximumX = allowOffscreenX ? SnesSpritemapXWord.OffsetFieldMaximum : byte.MaxValue;
        if (point.X < 0 || point.X > maximumX || (uint)point.Y > byte.MaxValue)
            throw new InvalidDataException(
                $"Options {owner} requires X=0..{maximumX} and Y=0..255.");
    }

    /// <summary>Requires a controller-label rectangle to fit the 32-by-32 page.</summary>
    private static void ValidateTilePoint(Position16<TilemapCells> point, string owner, Extent16<TilemapCells> size)
    {
        if (point.X < 0 || point.Y < 0 ||
            point.X + size.X > GameOptionsRomData.MenuTilemapWidth ||
            point.Y + size.Y > GameOptionsRomData.MenuTilemapHeight)
            throw new InvalidDataException($"Options {owner} escapes the 32x32 page.");
    }

    /// <summary>Requires an authored BG or OBJ palette index in the SNES range zero through seven.</summary>
    private static void ValidatePalette(int palette, string owner)
    {
        if ((uint)palette >= MapPresentationFormat.PaletteCount)
            throw new InvalidDataException($"Options {owner} must be 0..7.");
    }

    /// <summary>Extracts OBJ palette attribute bits for a heading or cursor composition.</summary>
    private static ushort PaletteBits(int index) =>
        SnesObjAttributeWord.Create(0, index, 0).PaletteBits;
}
