using System.Security.Cryptography;
using System.Text.Json;
using SuperMetroid.Core;
using SuperMetroid.Core.Assets;
using SuperMetroid.AssetExtraction.Frontend.Menus;
using SuperMetroid.Core.Frontend.Options;
using SuperMetroid.Core.Frontend.Options.Artwork;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Map.Artwork;
using SuperMetroid.Core.Spatial;
using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.AssetExtraction.Frontend.Map;

namespace SuperMetroid.AssetExtraction.Frontend.Options.Artwork;

/// <summary>
/// The installed options presentation as the import pipeline reads it (#1463): beside the pages, labels, highlight
/// regions and anchors the game binds, the heading and cursor compositions the options sheet is composed from.
/// </summary>
public sealed class GameOptionsSourcePresentation : GameOptionsPresentation
{
    /// <summary>Each menu layout's heading sprite.</summary>
    internal readonly Dictionary<GameOptionsPage, SpriteComposition> headingSprites;
    /// <summary>The missile cursor's sprite by animation frame.</summary>
    internal readonly SpriteComposition[] cursorSprites;

    /// <summary>Compiled tiles of each named options page.</summary>
    private readonly Dictionary<GameOptionsTilemap, BackgroundTile[]> pages;
    /// <summary>The common backdrop page.</summary>
    private readonly BackgroundTile[] background;
    /// <summary>Edited controller-button glyphs, keyed by button; a button absent here draws its native glyph.</summary>
    internal readonly Dictionary<ControllerButton, BackgroundTile[]> controllerLabels;
    /// <summary>Edited controller-label anchors by action, or null for the native destinations.</summary>
    internal readonly Position16<TilemapCells>[]? controllerLabelAnchors;
    /// <summary>Edited primary-language highlight regions, or null for the native regions.</summary>
    internal readonly GameOptionsLanguageRegionDocument[]? languageRegions;
    /// <summary>Edited special-setting visuals, or null for the native cells.</summary>
    internal readonly Dictionary<GameOptionsToggle, GameOptionsToggleVisualDocument>? specialToggles;

    /// <summary>Retains the compiled presentation, only the visual edits that depart from stock geometry, and its compositions.</summary>
    private GameOptionsSourcePresentation(GameOptionsPresentationDocument document, GameOptionsPages pages,
        GameOptionsSprites sprites, string contentIdentity)
        : base(document, contentIdentity)
    {
        this.pages = pages.Pages;
        background = pages.Background;
        controllerLabels = pages.ControllerLabels.Where(pair => pair.Value.Where((tile, cell) =>
            tile != GameOptionsPageCells.ControllerLabelTile(pair.Key, cell)).Any())
            .ToDictionary(pair => pair.Key, pair => pair.Value);
        controllerLabelAnchors = document.ControllerLabelAnchors.Where((point, index) =>
            point != GameOptionsPageCells.ControllerAnchor(index)).Any() ? document.ControllerLabelAnchors : null;
        languageRegions = document.LanguageRegions.Where((region, index) =>
            region.HighlightWhenJapanese != GameOptionsPageCells.LanguagePaletteRegion(index).HighlightWhenJapanese ||
            !region.Cells.SequenceEqual(GameOptionsPageCells.LanguageCells(index))).Any()
            ? document.LanguageRegions : null;
        Dictionary<GameOptionsToggle, GameOptionsToggleVisualDocument> toggles = GameOptionsPresentationDefinitions.Toggles.ToDictionary(
            toggle => toggle, toggle => document.SpecialToggles[GameOptionsNames.ToggleName(toggle)]);
        specialToggles = toggles.Any(pair =>
            !pair.Value.EnabledCells.SequenceEqual(GameOptionsPageCells.ToggleCells(pair.Key, GameOptionsToggleState.Enabled)) ||
            !pair.Value.DisabledCells.SequenceEqual(GameOptionsPageCells.ToggleCells(pair.Key, GameOptionsToggleState.Disabled)))
            ? toggles : null;
        SelectedPalette = document.SelectedPalette;
        UnselectedPalette = document.UnselectedPalette;
        headingSprites = sprites.Headings;
        cursorSprites = sprites.Cursors;
    }

    /// <summary>BG palette index 0-7 applied to the currently chosen language and special-toggle text regions.</summary>
    public int SelectedPalette { get; }
    /// <summary>BG palette index 0-7 applied to the other language and toggle choices; differs from <see cref="SelectedPalette"/>.</summary>
    public int UnselectedPalette { get; }

    /// <summary>Returns an independent mutable copy of one compiled foreground page.</summary>
    internal BackgroundTile[] CreatePage(GameOptionsTilemap tilemap) => (BackgroundTile[])pages[tilemap].Clone();

    /// <summary>Recolors English and Japanese text regions according to the selected language.</summary>
    internal void ApplyLanguage(Span<BackgroundTile> primaryPage, bool japanese)
    {
        for (int index = 0; index < GameOptionsDefinitions.LanguagePaletteRegionCount; index++)
            ApplyPalette(primaryPage, LanguageRegionCells(index), japanese == HighlightsWhenJapanese(index) ? SelectedPalette : UnselectedPalette);
    }

    /// <summary>The primary-page cells of language region <paramref name="index"/>.</summary>
    internal IReadOnlyCollection<int> LanguageRegionCells(int index) =>
        languageRegions is null ? [.. GameOptionsPageCells.LanguageCells(index)] : languageRegions[index].Cells;

    /// <summary>Whether language region <paramref name="index"/> takes the selected palette while the text is Japanese.</summary>
    private bool HighlightsWhenJapanese(int index) =>
        languageRegions?[index].HighlightWhenJapanese ?? GameOptionsPageCells.LanguagePaletteRegion(index).HighlightWhenJapanese;

    /// <summary>Writes the selected button glyph into one controller action's two-row label area.</summary>
    internal void ApplyControllerLabel(Span<BackgroundTile> page, int action, int button)
    {
        if ((uint)action >= GameOptionsDefinitions.Rows.ControllerActionCount)
            throw new ArgumentOutOfRangeException(nameof(action));
        ControllerButton label = GameOptionsNames.ControllerLabelButton(button);
        controllerLabels.TryGetValue(label, out BackgroundTile[]? cells);
        foreach (GameOptionsLabelCell cell in ControllerLabelCells(action))
            page[cell.PageCell] = cells is not null ? cells[cell.GlyphCell] : GameOptionsPageCells.ControllerLabelTile(label, cell.GlyphCell);
    }

    /// <summary>Each page cell of controller action <paramref name="action"/>'s label, with its place in the label's glyph.</summary>
    private IEnumerable<GameOptionsLabelCell> ControllerLabelCells(int action)
    {
        Position16<TilemapCells> anchor = controllerLabelAnchors?[action] ?? GameOptionsPageCells.ControllerAnchor(action);
        for (int row = 0; row < GameOptionsPageDefinitions.ControllerLabelHeight; row++)
        for (int column = 0; column < GameOptionsPageDefinitions.ControllerLabelWidth; column++)
            yield return new GameOptionsLabelCell((anchor.Y + row) * GameOptionsDefinitions.MenuTilemapWidth + anchor.X + column,
                row * GameOptionsPageDefinitions.ControllerLabelWidth + column);
    }

    /// <summary>Recolors the enabled and disabled labels for one special setting.</summary>
    internal void ApplySpecialToggle(Span<BackgroundTile> page, GameOptionsToggle name, bool enabled)
    {
        ApplyPalette(page, ToggleChoiceCells(name, GameOptionsToggleState.Enabled), enabled ? SelectedPalette : UnselectedPalette);
        ApplyPalette(page, ToggleChoiceCells(name, GameOptionsToggleState.Disabled), enabled ? UnselectedPalette : SelectedPalette);
    }

    /// <summary>The page cells of <paramref name="name"/>'s <paramref name="state"/> choice.</summary>
    private IReadOnlyCollection<int> ToggleChoiceCells(GameOptionsToggle name, GameOptionsToggleState state) => specialToggles is null
        ? [.. GameOptionsPageCells.ToggleCells(name, state)]
        : state == GameOptionsToggleState.Enabled ? specialToggles[name].EnabledCells : specialToggles[name].DisabledCells;

    /// <summary>Changes only the colour variant of selected tilemap cells, preserving their artwork and mirroring.</summary>
    private static void ApplyPalette(Span<BackgroundTile> page, IEnumerable<int> cells, int palette)
    {
        foreach (int cell in cells)
            page[cell] = page[cell] with { ColourVariant = palette };
    }

    /// <summary>
    /// Every picture of the options screen the menus' sheet holds (#1467): the backdrop page; each page less its dynamic
    /// cells; each language row as the primary page shows it in either language; each action's label naming each
    /// assignable button; and each toggle's two choices on each special page with either selected.
    /// </summary>
    internal IEnumerable<ScreenPictureSubject> Subjects()
    {
        int width = GameOptionsDefinitions.MenuTilemapWidth;
        yield return ScreenPictureSubject.Page(GameOptionsNames.BackgroundFrame, background, width);
        foreach (GameOptionsTilemap tilemap in GameOptionsPresentationDefinitions.Tilemaps)
            yield return ScreenPictureSubject.Page(GameOptionsNames.PageFrame(tilemap), pages[tilemap], width, DynamicCells(tilemap));
        foreach (bool japanese in new[] { false, true })
        {
            BackgroundTile[] primary = CreatePage(GameOptionsTilemap.Primary);
            ApplyLanguage(primary, japanese);
            for (int region = 0; region < GameOptionsDefinitions.LanguagePaletteRegionCount; region++)
                yield return ScreenPictureSubject.Cells(GameOptionsNames.LanguageFrame(region, japanese), primary, width, LanguageRegionCells(region));
        }
        for (int action = 0; action < GameOptionsDefinitions.Rows.ControllerActionCount; action++)
        {
            int[] cells = [.. ControllerLabelCells(action).Select(cell => cell.PageCell)];
            for (int button = 0; button < GameOptionsPresentationDefinitions.ControllerLabelButtons.Count; button++)
            {
                BackgroundTile[] controller = CreatePage(GameOptionsTilemap.ControllerEnglish);
                ApplyControllerLabel(controller, action, button);
                yield return ScreenPictureSubject.Cells(GameOptionsNames.ControllerLabelFrame(action, GameOptionsNames.ControllerLabelButton(button)),
                    controller, width, cells);
            }
        }
        foreach (GameOptionsTilemap special in new[] { GameOptionsTilemap.SpecialEnglish, GameOptionsTilemap.SpecialJapanese })
        foreach (GameOptionsToggle toggle in GameOptionsPresentationDefinitions.Toggles)
        foreach (bool enabled in new[] { false, true })
        {
            BackgroundTile[] page = CreatePage(special);
            ApplySpecialToggle(page, toggle, enabled);
            int[] cells = [.. ToggleChoiceCells(toggle, GameOptionsToggleState.Enabled), .. ToggleChoiceCells(toggle, GameOptionsToggleState.Disabled)];
            yield return ScreenPictureSubject.Cells(GameOptionsNames.ToggleFrame(special, toggle, enabled), page, width, cells);
        }
    }

    /// <summary>The cells of <paramref name="tilemap"/> its pictures over it show instead of the page's own picture.</summary>
    private IEnumerable<int> DynamicCells(GameOptionsTilemap tilemap) => tilemap switch
    {
        GameOptionsTilemap.Primary => Enumerable.Range(0, GameOptionsDefinitions.LanguagePaletteRegionCount).SelectMany(LanguageRegionCells),
        GameOptionsTilemap.ControllerEnglish or GameOptionsTilemap.ControllerJapanese =>
            Enumerable.Range(0, GameOptionsDefinitions.Rows.ControllerActionCount).SelectMany(action => ControllerLabelCells(action).Select(cell => cell.PageCell)),
        GameOptionsTilemap.SpecialEnglish or GameOptionsTilemap.SpecialJapanese => GameOptionsPresentationDefinitions.Toggles.SelectMany(toggle =>
            ToggleChoiceCells(toggle, GameOptionsToggleState.Enabled).Concat(ToggleChoiceCells(toggle, GameOptionsToggleState.Disabled))),
        _ => throw new ArgumentOutOfRangeException(nameof(tilemap), tilemap, "Undefined options page."),
    };

    /// <summary>Reads a complete options-menu JSON document and compiles validated pages, controller labels, highlights, and sprite layouts.</summary>
    /// <param name="source">Readable stream consumed from its current position to the end and left open.</param>
    /// <returns>A presentation owning independently parsed layout and compiled artwork; stock-matching values are reconstructed from definitions.</returns>
    /// <exception cref="InvalidDataException">The JSON, version, required identities, cells, anchors, palette selectors, or cursor duration are invalid.</exception>
    public static GameOptionsSourcePresentation Load(Stream source)
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
        ValidateExactKeys(document.Pages, [GameOptionsPresentationDefinitions.BackgroundPageName,
                .. GameOptionsPresentationDefinitions.Tilemaps.Select(GameOptionsNames.TilemapName)],
            "options pages");
        ValidateExactKeys(document.ControllerLabels,
            [.. GameOptionsPresentationDefinitions.ControllerLabelButtons.Select(GameOptionsNames.ControllerLabelName)],
            "options controller labels");
        ValidateExactKeys(document.SpecialToggles,
            [.. GameOptionsPresentationDefinitions.Toggles.Select(GameOptionsNames.ToggleName)],
            "options special toggles");
        ValidateExactKeys(document.Sprites,
            [.. GameOptionsPresentationDefinitions.MenuPages.Select(GameOptionsNames.HeadingSpriteName),
                .. Enumerable.Range(0, GameOptionsPresentationDefinitions.CursorFrameCount).Select(GameOptionsNames.CursorSpriteName)],
            "options sprites");
        string[] menuNames = [.. GameOptionsPresentationDefinitions.MenuPages.Select(GameOptionsNames.MenuName)];
        ValidateExactKeys(document.HeadingAnchors, menuNames, "options heading anchors");
        ValidateExactKeys(document.CursorAnchors, menuNames, "options cursor anchors");
        if (document.ControllerLabelAnchors is null ||
            document.ControllerLabelAnchors.Length != GameOptionsDefinitions.Rows.ControllerActionCount)
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
        ValidatePoint(document.HiddenCursor, "hidden cursor", offscreenX: OffscreenX.Allowed);

        var pages = GameOptionsPresentationDefinitions.Tilemaps.ToDictionary(tilemap => tilemap,
            tilemap => CompilePage(document, GameOptionsNames.TilemapName(tilemap)));
        BackgroundTile[] background = CompilePage(document, GameOptionsPresentationDefinitions.BackgroundPageName);

        var labels = new Dictionary<ControllerButton, BackgroundTile[]>();
        foreach (ControllerButton button in GameOptionsPresentationDefinitions.ControllerLabelButtons)
        {
            string name = GameOptionsNames.ControllerLabelName(button);
            MapPresentationCell[] cells = document.ControllerLabels[name];
            if (cells is null || cells.Length != GameOptionsPageDefinitions.ControllerLabelCellCount)
                throw new InvalidDataException($"Options controller label {name} requires six cells.");
            labels.Add(button, CompileCells(cells, $"options controller label {name}"));
        }

        foreach (Position16<TilemapCells> anchor in document.ControllerLabelAnchors)
            ValidateTilePoint(anchor, "controller-label anchor", GameOptionsPageDefinitions.ControllerLabelSize);
        var claimedLanguageCells = new HashSet<int>();
        foreach (GameOptionsLanguageRegionDocument? region in document.LanguageRegions)
        {
            if (region is null || region.Cells is null || region.Cells.Length == 0)
                throw new InvalidDataException("Each options language region requires cells.");
            ValidateCellSet(region.Cells, claimedLanguageCells, "language region");
        }
        foreach (GameOptionsToggle option in GameOptionsPresentationDefinitions.Toggles)
        {
            string name = GameOptionsNames.ToggleName(option);
            GameOptionsToggleVisualDocument? toggle = document.SpecialToggles[name];
            if (toggle is null || toggle.EnabledCells is null || toggle.DisabledCells is null ||
                toggle.EnabledCells.Length == 0 || toggle.DisabledCells.Length == 0)
                throw new InvalidDataException($"Options toggle {name} requires enabled and disabled cells.");
            var claimed = new HashSet<int>();
            ValidateCellSet(toggle.EnabledCells, claimed, $"{name} enabled");
            ValidateCellSet(toggle.DisabledCells, claimed, $"{name} disabled");
        }

        var headingSprites = GameOptionsPresentationDefinitions.MenuPages.ToDictionary(page => page,
            page => MenuHeadingBorder.CalculateIfMatching(Ensure.Listed(GameOptionsSpriteDefinitions.Headings, page),
                MenuBorderParts.CalculateIfMatching(CompileSprite(document, GameOptionsNames.HeadingSpriteName(page)))));
        SpriteComposition[] cursorSprites = [.. Enumerable.Range(0, GameOptionsPresentationDefinitions.CursorFrameCount)
            .Select(frame => MenuCursorParts.CalculateIfMatching(frame,
                CompileSprite(document, GameOptionsNames.CursorSpriteName(frame))))];
        foreach (GameOptionsPage page in GameOptionsPresentationDefinitions.MenuPages)
        {
            string name = GameOptionsNames.MenuName(page);
            ValidatePoint(document.HeadingAnchors[name], $"{name} heading", offscreenX: OffscreenX.Rejected);
            int expected = GameOptionsNames.CursorCount(page);
            Position16<SpritePixels>[]? points = document.CursorAnchors[name];
            if (points is null || points.Length != expected)
                throw new InvalidDataException($"Options {name} requires {expected} cursor anchors.");
            foreach (Position16<SpritePixels> point in points)
                ValidatePoint(point, $"{name} cursor", offscreenX: OffscreenX.Rejected);
        }

        return new(document, new(pages, background, labels), new(headingSprites, cursorSprites),
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

    /// <summary>The import-side presentation <paramref name="options"/> was read with.</summary>
    /// <exception cref="InvalidDataException">The options presentation was not read from installed artwork.</exception>
    public static GameOptionsSourcePresentation Of(GameOptionsPresentation options) => options as GameOptionsSourcePresentation
        ?? throw new InvalidDataException("The options presentation was not read from installed artwork.");

    /// <summary>Compiles the 1024-cell page the document names <paramref name="name"/>.</summary>
    private static BackgroundTile[] CompilePage(GameOptionsPresentationDocument document, string name)
    {
        MapPresentationCell[] cells = document.Pages[name];
        if (cells is null || cells.Length != GameOptionsPageDefinitions.PageCellCount)
            throw new InvalidDataException($"Options page {name} requires 1024 cells.");
        return CompileCells(cells, $"options page {name}");
    }

    /// <summary>Compiles the sprite the document names <paramref name="name"/>.</summary>
    private static SpriteComposition CompileSprite(GameOptionsPresentationDocument document, string name) =>
        MenuSpriteCompiler.Compile(document.Sprites[name], $"options {name}");

    /// <summary>Validates each authored atlas cell of <paramref name="owner"/> and places its tile.</summary>
    private static BackgroundTile[] CompileCells(MapPresentationCell[] cells, string owner)
    {
        var tiles = new BackgroundTile[cells.Length];
        for (int index = 0; index < cells.Length; index++)
        {
            MapPresentationCell? cell = cells[index];
            if (cell is null ||
                (uint)cell.TileColumn >= MapTileAtlasFormat.TileColumns ||
                (uint)cell.TileRow >= GameOptionsPageDefinitions.CharacterRows ||
                (uint)cell.Palette >= MapPresentationFormat.PaletteCount)
                throw new InvalidDataException(
                    $"{owner} cell {index} has an invalid atlas coordinate or palette.");
            tiles[index] = new BackgroundTile(cell.TileRow * MapTileAtlasFormat.TileColumns + cell.TileColumn, cell.Palette,
                cell.FlipX, cell.FlipY, cell.Priority);
        }
        return tiles;
    }

    /// <summary>Requires exactly the names prescribed for one options document collection.</summary>
    private static void ValidateExactKeys<T>(Dictionary<string, T>? values,
        string[] expected, string owner)
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
            if ((uint)cell >= GameOptionsPageDefinitions.PageCellCount || !claimed.Add(cell))
                throw new InvalidDataException($"Options {owner} contains an invalid or duplicate cell {cell}.");
    }

    /// <summary>Validates an OAM origin, allowing hidden cursor X positions when requested.</summary>
    private static void ValidatePoint(Position16<SpritePixels> point, string owner, OffscreenX offscreenX)
    {
        int maximumX = offscreenX == OffscreenX.Allowed ? SpritePart.PlaneWidth - 1 : byte.MaxValue;
        if (point.X < 0 || point.X > maximumX || (uint)point.Y > byte.MaxValue)
            throw new InvalidDataException(
                $"Options {owner} requires X=0..{maximumX} and Y=0..255.");
    }

    /// <summary>Requires a controller-label rectangle to fit the 32-by-32 page.</summary>
    private static void ValidateTilePoint(Position16<TilemapCells> point, string owner, Extent16<TilemapCells> size)
    {
        if (point.X < 0 || point.Y < 0 ||
            point.X + size.X > GameOptionsDefinitions.MenuTilemapWidth ||
            point.Y + size.Y > GameOptionsDefinitions.MenuTilemapHeight)
            throw new InvalidDataException($"Options {owner} escapes the 32x32 page.");
    }

    /// <summary>Requires an authored BG or OBJ palette index in the SNES range zero through seven.</summary>
    private static void ValidatePalette(int palette, string owner)
    {
        if ((uint)palette >= MapPresentationFormat.PaletteCount)
            throw new InvalidDataException($"Options {owner} must be 0..7.");
    }

    /// <summary>The compiled pages, common background and controller-button glyphs.</summary>
    /// <param name="Pages">Each named page's tiles.</param>
    /// <param name="Background">The common BG2 background's tiles.</param>
    /// <param name="ControllerLabels">Each assignable button's glyph.</param>
    private sealed record GameOptionsPages(Dictionary<GameOptionsTilemap, BackgroundTile[]> Pages, BackgroundTile[] Background,
        Dictionary<ControllerButton, BackgroundTile[]> ControllerLabels);

    /// <summary>The compiled heading and cursor compositions.</summary>
    /// <param name="Headings">Each menu layout's heading composition.</param>
    /// <param name="Cursors">The missile cursor's composition by animation frame.</param>
    private sealed record GameOptionsSprites(Dictionary<GameOptionsPage, SpriteComposition> Headings, SpriteComposition[] Cursors);
}
