using System.Security.Cryptography;
using System.Text.Json;
using SuperMetroid.Core;
using SuperMetroid.AssetExtraction.Frontend.Menus;
using SuperMetroid.Core.Frontend.Options;
using SuperMetroid.Core.Frontend.Options.Artwork;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Spatial;
using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.AssetExtraction.Frontend.Map;
using SuperMetroid.AssetExtraction.Map.Artwork;

namespace SuperMetroid.AssetExtraction.Frontend.Options.Artwork;

/// <summary>
/// The options presentation as the import pipeline reads it (#1463, #1638), from the extracted artwork document in the
/// component's import data: the anchors (<see cref="Presentation"/>), which the installed layout holds for the game
/// (<see cref="GameOptionsLayoutFile"/>), and the pages, labels, highlight regions and the heading and cursor compositions
/// the options sheets are composed from, which the game never receives (#1616).
/// </summary>
public sealed class GameOptionsSourcePresentation
{
    /// <summary>Each menu layout's heading sprite.</summary>
    internal readonly Dictionary<GameOptionsPage, SpriteComposition> headingSprites;
    /// <summary>The missile cursor's sprite by animation frame.</summary>
    internal readonly SpriteComposition[] cursorSprites;

    /// <summary>Compiled tiles of each named options page.</summary>
    private readonly Dictionary<GameOptionsScreen, BackgroundTile[]> pages;
    /// <summary>The common backdrop page.</summary>
    private readonly BackgroundTile[] background;
    /// <summary>The installed controller-button glyphs, keyed by every assignable button.</summary>
    internal readonly Dictionary<ControllerButton, BackgroundTile[]> controllerLabels;
    /// <summary>The installed controller-label anchors, one per action.</summary>
    internal readonly Position16<CharacterCells>[] controllerLabelAnchors;
    /// <summary>The installed primary-language highlight regions.</summary>
    private readonly GameOptionsLanguageRegionDocument[] languageRegions;
    /// <summary>The installed special-setting choice cells, keyed by toggle.</summary>
    private readonly Dictionary<GameOptionsToggle, GameOptionsToggleVisualDocument> specialToggles;

    /// <summary>The heading and cursor anchors and the cursor's timing the game binds.</summary>
    public GameOptionsPresentation Presentation { get; }

    /// <summary>Uppercase SHA-256 digest of the loaded JSON bytes, identifying selected artwork and layout including their encoding.</summary>
    public string ContentIdentity { get; }

    /// <summary>OBJ palette index 0-7 the missile cursor and menu-heading compositions are composed in.</summary>
    public int CursorPalette { get; }

    /// <summary>Retains the compiled presentation, its complete installed geometry, and its compositions.</summary>
    private GameOptionsSourcePresentation(GameOptionsPresentationDocument document, GameOptionsPresentation presentation, GameOptionsPages pages,
        GameOptionsSprites sprites, string contentIdentity)
    {
        Presentation = presentation;
        ContentIdentity = contentIdentity;
        CursorPalette = document.CursorPalette;
        this.pages = pages.Pages;
        background = pages.Background;
        controllerLabels = pages.ControllerLabels;
        controllerLabelAnchors = [.. document.ControllerLabelAnchors];
        languageRegions = [.. document.LanguageRegions];
        specialToggles = GameOptionsPresentationFormat.Toggles.ToDictionary(
            toggle => toggle, toggle => document.SpecialToggles[GameOptionsNames.ToggleName(toggle)]);
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
    internal BackgroundTile[] CreatePage(GameOptionsScreen tilemap) => (BackgroundTile[])pages[tilemap].Clone();

    /// <summary>Recolors English and Japanese text regions according to the selected language.</summary>
    internal void ApplyLanguage(Span<BackgroundTile> primaryPage, bool japanese)
    {
        for (int index = 0; index < GameOptionsDefinitions.LanguagePaletteRegionCount; index++)
            ApplyPalette(primaryPage, LanguageRegionCells(index), japanese == HighlightsWhenJapanese(index) ? SelectedPalette : UnselectedPalette);
    }

    /// <summary>The primary-page cells of language region <paramref name="index"/>.</summary>
    internal IReadOnlyCollection<int> LanguageRegionCells(int index) => languageRegions[index].Cells;

    /// <summary>Whether language region <paramref name="index"/> takes the selected palette while the text is Japanese.</summary>
    private bool HighlightsWhenJapanese(int index) => languageRegions[index].HighlightWhenJapanese;

    /// <summary>Writes the selected button glyph into one controller action's two-row label area.</summary>
    internal void ApplyControllerLabel(Span<BackgroundTile> page, int action, int button)
    {
        if ((uint)action >= GameOptionsDefinitions.Rows.ControllerActionCount)
            throw new ArgumentOutOfRangeException(nameof(action));
        ControllerButton label = GameOptionsNames.ControllerLabelButton(button);
        BackgroundTile[] cells = controllerLabels[label];
        foreach (GameOptionsLabelCell cell in ControllerLabelCells(action))
            page[cell.PageCell] = cells[cell.GlyphCell];
    }

    /// <summary>Each page cell of controller action <paramref name="action"/>'s label, with its place in the label's glyph.</summary>
    private IEnumerable<GameOptionsLabelCell> ControllerLabelCells(int action)
    {
        Position16<CharacterCells> anchor = controllerLabelAnchors[action];
        for (int row = 0; row < GameOptionsPageDefinitions.ControllerLabelHeight; row++)
        for (int column = 0; column < GameOptionsPageDefinitions.ControllerLabelWidth; column++)
            yield return new GameOptionsLabelCell((anchor.Y + row) * GameOptionsPageDefinitions.MenuTilemapWidth + anchor.X + column,
                row * GameOptionsPageDefinitions.ControllerLabelWidth + column);
    }

    /// <summary>Recolors the enabled and disabled labels for one special setting.</summary>
    internal void ApplySpecialToggle(Span<BackgroundTile> page, GameOptionsToggle name, bool enabled)
    {
        ApplyPalette(page, ToggleChoiceCells(name, GameOptionsToggleState.Enabled), enabled ? SelectedPalette : UnselectedPalette);
        ApplyPalette(page, ToggleChoiceCells(name, GameOptionsToggleState.Disabled), enabled ? UnselectedPalette : SelectedPalette);
    }

    /// <summary>The page cells of <paramref name="name"/>'s <paramref name="state"/> choice.</summary>
    private int[] ToggleChoiceCells(GameOptionsToggle name, GameOptionsToggleState state) =>
        state == GameOptionsToggleState.Enabled ? specialToggles[name].EnabledCells : specialToggles[name].DisabledCells;

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
        int width = GameOptionsPageDefinitions.MenuTilemapWidth;
        yield return ScreenPictureSubject.Page(GameOptionsNames.BackgroundFrame, background, width);
        foreach (GameOptionsScreen tilemap in GameOptionsPresentationFormat.Tilemaps)
            yield return ScreenPictureSubject.Page(GameOptionsNames.PageFrame(tilemap), pages[tilemap], width, DynamicCells(tilemap));
        foreach (bool japanese in new[] { false, true })
        {
            BackgroundTile[] primary = CreatePage(GameOptionsScreen.Primary);
            ApplyLanguage(primary, japanese);
            for (int region = 0; region < GameOptionsDefinitions.LanguagePaletteRegionCount; region++)
                yield return ScreenPictureSubject.Cells(GameOptionsNames.LanguageFrame(region, japanese), primary, width, LanguageRegionCells(region));
        }
        for (int action = 0; action < GameOptionsDefinitions.Rows.ControllerActionCount; action++)
        {
            int[] cells = [.. ControllerLabelCells(action).Select(cell => cell.PageCell)];
            for (int button = 0; button < GameOptionsPresentationDefinitions.ControllerLabelButtons.Count; button++)
            {
                BackgroundTile[] controller = CreatePage(GameOptionsScreen.ControllerEnglish);
                ApplyControllerLabel(controller, action, button);
                yield return ScreenPictureSubject.Cells(GameOptionsNames.ControllerLabelFrame(action, GameOptionsNames.ControllerLabelButton(button)),
                    controller, width, cells);
            }
        }
        foreach (GameOptionsScreen special in new[] { GameOptionsScreen.SpecialEnglish, GameOptionsScreen.SpecialJapanese })
        foreach (GameOptionsToggle toggle in GameOptionsPresentationFormat.Toggles)
        foreach (bool enabled in new[] { false, true })
        {
            BackgroundTile[] page = CreatePage(special);
            ApplySpecialToggle(page, toggle, enabled);
            int[] cells = [.. ToggleChoiceCells(toggle, GameOptionsToggleState.Enabled), .. ToggleChoiceCells(toggle, GameOptionsToggleState.Disabled)];
            yield return ScreenPictureSubject.Cells(GameOptionsNames.ToggleFrame(special, toggle, enabled), page, width, cells);
        }
    }

    /// <summary>
    /// The name of every picture of the options screen the menus' sheet holds (#1467), in <see cref="Subjects"/> order: the
    /// names alone, which the game needs to check the installed sheet and which no installed document holds (#1638).
    /// </summary>
    internal static IEnumerable<string> SubjectNames()
    {
        yield return GameOptionsNames.BackgroundFrame;
        foreach (GameOptionsScreen tilemap in GameOptionsPresentationFormat.Tilemaps)
            yield return GameOptionsNames.PageFrame(tilemap);
        foreach (bool japanese in new[] { false, true })
            for (int region = 0; region < GameOptionsDefinitions.LanguagePaletteRegionCount; region++)
                yield return GameOptionsNames.LanguageFrame(region, japanese);
        for (int action = 0; action < GameOptionsDefinitions.Rows.ControllerActionCount; action++)
            for (int button = 0; button < GameOptionsPresentationDefinitions.ControllerLabelButtons.Count; button++)
                yield return GameOptionsNames.ControllerLabelFrame(action, GameOptionsNames.ControllerLabelButton(button));
        foreach (GameOptionsScreen special in new[] { GameOptionsScreen.SpecialEnglish, GameOptionsScreen.SpecialJapanese })
        foreach (GameOptionsToggle toggle in GameOptionsPresentationFormat.Toggles)
        foreach (bool enabled in new[] { false, true })
            yield return GameOptionsNames.ToggleFrame(special, toggle, enabled);
    }

    /// <summary>The cells of <paramref name="tilemap"/> its pictures over it show instead of the page's own picture.</summary>
    private IEnumerable<int> DynamicCells(GameOptionsScreen tilemap) => tilemap switch
    {
        GameOptionsScreen.Primary => Enumerable.Range(0, GameOptionsDefinitions.LanguagePaletteRegionCount).SelectMany(LanguageRegionCells),
        GameOptionsScreen.ControllerEnglish or GameOptionsScreen.ControllerJapanese =>
            Enumerable.Range(0, GameOptionsDefinitions.Rows.ControllerActionCount).SelectMany(action => ControllerLabelCells(action).Select(cell => cell.PageCell)),
        GameOptionsScreen.SpecialEnglish or GameOptionsScreen.SpecialJapanese => GameOptionsPresentationFormat.Toggles.SelectMany(toggle =>
            ToggleChoiceCells(toggle, GameOptionsToggleState.Enabled).Concat(ToggleChoiceCells(toggle, GameOptionsToggleState.Disabled))),
        _ => throw new ArgumentOutOfRangeException(nameof(tilemap), tilemap, "Undefined options page."),
    };

    /// <summary>Reads a complete options-menu JSON document and compiles validated pages, controller labels, highlights, and sprite layouts.</summary>
    /// <param name="source">Readable stream consumed from its current position to the end and left open.</param>
    /// <returns>A presentation owning its complete installed layout and compiled artwork.</returns>
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

        if (document.Version != GameOptionsPresentationFormat.Version)
            throw new InvalidDataException(
                $"Options presentation version must be {GameOptionsPresentationFormat.Version}.");
        ValidateExactKeys(document.Pages, [GameOptionsPresentationFormat.BackgroundPageName,
                .. GameOptionsPresentationFormat.Tilemaps.Select(GameOptionsNames.PageName)],
            "options pages");
        ValidateExactKeys(document.ControllerLabels,
            [.. GameOptionsPresentationDefinitions.ControllerLabelButtons.Select(GameOptionsNames.ControllerLabelName)],
            "options controller labels");
        ValidateExactKeys(document.SpecialToggles,
            [.. GameOptionsPresentationFormat.Toggles.Select(GameOptionsNames.ToggleName)],
            "options special toggles");
        ValidateExactKeys(document.Sprites,
            [.. GameOptionsPresentationFormat.MenuPages.Select(GameOptionsNames.HeadingSpriteName),
                .. Enumerable.Range(0, GameOptionsPresentationFormat.CursorFrameCount).Select(GameOptionsNames.CursorSpriteName)],
            "options sprites");
        GameOptionsPresentation presentation = GameOptionsLayoutFile.Presentation(document.HeadingAnchors, document.CursorAnchors,
            document.HiddenCursor, document.CursorFrameDuration);
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

        var pages = GameOptionsPresentationFormat.Tilemaps.ToDictionary(tilemap => tilemap,
            tilemap => CompilePage(document, GameOptionsNames.PageName(tilemap)));
        BackgroundTile[] background = CompilePage(document, GameOptionsPresentationFormat.BackgroundPageName);

        var labels = new Dictionary<ControllerButton, BackgroundTile[]>();
        foreach (ControllerButton button in GameOptionsPresentationDefinitions.ControllerLabelButtons)
        {
            string name = GameOptionsNames.ControllerLabelName(button);
            MapPresentationCell[] cells = document.ControllerLabels[name];
            if (cells is null || cells.Length != GameOptionsPageDefinitions.ControllerLabelCellCount)
                throw new InvalidDataException($"Options controller label {name} requires six cells.");
            labels.Add(button, CompileCells(cells, $"options controller label {name}"));
        }

        foreach (Position16<CharacterCells> anchor in document.ControllerLabelAnchors)
            ValidateTilePoint(anchor, "controller-label anchor", GameOptionsPageDefinitions.ControllerLabelSize);
        var claimedLanguageCells = new HashSet<int>();
        foreach (GameOptionsLanguageRegionDocument? region in document.LanguageRegions)
        {
            if (region is null || region.Cells is null || region.Cells.Length == 0)
                throw new InvalidDataException("Each options language region requires cells.");
            ValidateCellSet(region.Cells, claimedLanguageCells, "language region");
        }
        foreach (GameOptionsToggle option in GameOptionsPresentationFormat.Toggles)
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

        var headingSprites = GameOptionsPresentationFormat.MenuPages.ToDictionary(page => page,
            page => CompileSprite(document, GameOptionsNames.HeadingSpriteName(page)));
        SpriteComposition[] cursorSprites = [.. Enumerable.Range(0, GameOptionsPresentationFormat.CursorFrameCount)
            .Select(frame => CompileSprite(document, GameOptionsNames.CursorSpriteName(frame)))];
        return new(document, presentation, new(pages, background, labels), new(headingSprites, cursorSprites),
            Convert.ToHexString(SHA256.HashData(bytes)));
    }

    /// <summary>Serializes an authored document to the output.</summary>
    /// <param name="output">Writable stream receiving UTF-8 JSON at its current position; left open.</param>
    /// <param name="document">Authored artwork and layout, whose collections are not modified by serialization.</param>
    public static void Write(Stream output, GameOptionsPresentationDocument document)
    {
        byte[] bytes = JsonSerializer.SerializeToUtf8Bytes(document, ArtworkJson.Options);
        output.Write(bytes);
    }

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

    /// <summary>Requires a controller-label rectangle to fit the 32-by-32 page.</summary>
    private static void ValidateTilePoint(Position16<CharacterCells> point, string owner, Extent16<CharacterCells> size)
    {
        if (point.X < 0 || point.Y < 0 ||
            point.X + size.X > GameOptionsPageDefinitions.MenuTilemapWidth ||
            point.Y + size.Y > GameOptionsPageDefinitions.MenuTilemapHeight)
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
    private sealed record GameOptionsPages(Dictionary<GameOptionsScreen, BackgroundTile[]> Pages, BackgroundTile[] Background,
        Dictionary<ControllerButton, BackgroundTile[]> ControllerLabels);

    /// <summary>The compiled heading and cursor compositions.</summary>
    /// <param name="Headings">Each menu layout's heading composition.</param>
    /// <param name="Cursors">The missile cursor's composition by animation frame.</param>
    private sealed record GameOptionsSprites(Dictionary<GameOptionsPage, SpriteComposition> Headings, SpriteComposition[] Cursors);
}
