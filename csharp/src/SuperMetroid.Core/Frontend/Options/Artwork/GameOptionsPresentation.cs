using SuperMetroid.Core.Input;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Frontend.Menus;

namespace SuperMetroid.Core.Frontend.Options.Artwork;

/// <summary>
/// Immutable options-screen pages, dynamic label patches, highlight regions and visual anchors the game binds. The
/// heading and cursor are drawn from the installed options sheet, whose compositions belong to the import pipeline that
/// reads the installed presentation (#1463). Navigation, binding swaps, toggles and fades remain application mechanics.
/// </summary>
public abstract class GameOptionsPresentation
{
    /// <summary>Compiled tiles of each named options page.</summary>
    private readonly Dictionary<GameOptionsTilemap, BackgroundTile[]> pages;
    /// <summary>The common BG2 background page.</summary>
    private readonly BackgroundTile[] background;
    /// <summary>Edited controller-button glyphs, keyed by button; a button absent here draws its native glyph.</summary>
    internal readonly Dictionary<ControllerButton, BackgroundTile[]> controllerLabels;
    /// <summary>Edited controller-label anchors by action, or null for the native destinations.</summary>
    internal readonly Position16<TilemapCells>[]? controllerLabelAnchors;
    /// <summary>Edited primary-language highlight regions, or null for the native regions.</summary>
    internal readonly GameOptionsLanguageRegionDocument[]? languageRegions;
    /// <summary>Edited special-setting visuals, or null for the native cells.</summary>
    internal readonly Dictionary<GameOptionsToggle, GameOptionsToggleVisualDocument>? specialToggles;
    /// <summary>Edited heading anchors by menu layout, or null for the native anchors.</summary>
    internal readonly Dictionary<GameOptionsPage, Position16<SpritePixels>>? headingAnchors;
    /// <summary>Edited cursor anchors by menu layout, or null for the native anchors.</summary>
    internal readonly Dictionary<GameOptionsPage, Position16<SpritePixels>[]>? cursorAnchors;

    /// <summary>Retains compiled pages and only visual edits that depart from stock geometry.</summary>
    /// <param name="document">The validated layout.</param>
    /// <param name="pages">Each named page's compiled tiles.</param>
    /// <param name="background">The common BG2 background's compiled tiles.</param>
    /// <param name="controllerLabels">Each assignable button's compiled glyph.</param>
    /// <param name="contentIdentity">The identity of the selected presentation's bytes.</param>
    internal GameOptionsPresentation(
        GameOptionsLayoutDocument document,
        Dictionary<GameOptionsTilemap, BackgroundTile[]> pages,
        BackgroundTile[] background,
        Dictionary<ControllerButton, BackgroundTile[]> controllerLabels,
        string contentIdentity)
    {
        this.pages = pages;
        this.background = background;
        this.controllerLabels = controllerLabels.Where(pair => pair.Value.Where((tile, cell) =>
            tile != GameOptionsArtworkCells.ControllerLabelTile(pair.Key, cell)).Any())
            .ToDictionary(pair => pair.Key, pair => pair.Value);
        controllerLabelAnchors = document.ControllerLabelAnchors.Where((point, index) =>
            point != GameOptionsArtworkCells.ControllerAnchor(index)).Any() ? document.ControllerLabelAnchors : null;
        languageRegions = document.LanguageRegions.Where((region, index) =>
            region.HighlightWhenJapanese != GameOptionsPositions.LanguagePaletteRegion(index).HighlightWhenJapanese ||
            !region.Cells.SequenceEqual(GameOptionsArtworkCells.LanguageCells(index))).Any()
            ? document.LanguageRegions : null;
        Dictionary<GameOptionsToggle, GameOptionsToggleVisualDocument> toggles = GameOptionsPresentationDefinitions.Toggles.ToDictionary(
            toggle => toggle, toggle => document.SpecialToggles[GameOptionsNames.ToggleName(toggle)]);
        specialToggles = toggles.Any(pair =>
            !pair.Value.EnabledCells.SequenceEqual(GameOptionsArtworkCells.ToggleCells(pair.Key, GameOptionsToggleState.Enabled)) ||
            !pair.Value.DisabledCells.SequenceEqual(GameOptionsArtworkCells.ToggleCells(pair.Key, GameOptionsToggleState.Disabled)))
            ? toggles : null;
        Dictionary<GameOptionsPage, Position16<SpritePixels>> headings = GameOptionsPresentationDefinitions.MenuPages.ToDictionary(
            page => page, page => document.HeadingAnchors[GameOptionsNames.MenuName(page)]);
        headingAnchors = headings.Any(pair => pair.Value != GameOptionsArtworkCells.HeadingAnchor(pair.Key)) ? headings : null;
        Dictionary<GameOptionsPage, Position16<SpritePixels>[]> cursors = GameOptionsPresentationDefinitions.MenuPages.ToDictionary(
            page => page, page => document.CursorAnchors[GameOptionsNames.MenuName(page)]);
        cursorAnchors = cursors.Any(pair => pair.Value.Where((point, index) =>
            point != GameOptionsArtworkCells.CursorAnchor(pair.Key, index)).Any()) ? cursors : null;
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
    public Position16<SpritePixels> HiddenCursor { get; }
    /// <summary>BG palette index 0-7 applied to the currently chosen language and special-toggle text regions.</summary>
    public int SelectedPalette { get; }
    /// <summary>BG palette index 0-7 applied to the other language and toggle choices; differs from <see cref="SelectedPalette"/>.</summary>
    public int UnselectedPalette { get; }
    /// <summary>OBJ palette index 0-7 used for both the missile cursor and menu-heading sprite compositions.</summary>
    public int CursorPalette { get; }
    /// <summary>Menu updates per frame of the four-frame looping missile cursor, in the range 1-65535.</summary>
    public int CursorFrameDuration { get; }

    /// <summary>Returns an independent mutable copy of one compiled foreground or background page.</summary>
    internal BackgroundTile[] CreatePage(GameOptionsTilemap tilemap) => (BackgroundTile[])pages[tilemap].Clone();

    /// <summary>Copies the common BG2 background page into the menu's BG2 tilemap.</summary>
    internal void LoadBackground(MenuTilemaps tilemaps) =>
        MenuTilemaps.Load(tilemaps.Bg2, background);

    /// <summary>Recolors English and Japanese text regions according to the selected language.</summary>
    internal void ApplyLanguage(Span<BackgroundTile> primaryPage, bool japanese)
    {
        if (languageRegions is null)
        {
            for (int index = 0; index < GameOptionsDefinitions.LanguagePaletteRegionCount; index++)
            {
                var region = GameOptionsPositions.LanguagePaletteRegion(index);
                ApplyPalette(primaryPage, GameOptionsArtworkCells.LanguageCells(index),
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
    internal void ApplyControllerLabel(Span<BackgroundTile> page, int action, int button)
    {
        if ((uint)action >= GameOptionsDefinitions.Rows.ControllerActionCount)
            throw new ArgumentOutOfRangeException(nameof(action));
        ControllerButton label = GameOptionsNames.ControllerLabelButton(button);
        controllerLabels.TryGetValue(label, out BackgroundTile[]? cells);
        Position16<TilemapCells> anchor = controllerLabelAnchors?[action] ?? GameOptionsArtworkCells.ControllerAnchor(action);
        for (int row = 0; row < GameOptionsPresentationDefinitions.ControllerLabelHeight; row++)
        for (int column = 0; column < GameOptionsPresentationDefinitions.ControllerLabelWidth; column++)
        {
            int destination = (anchor.Y + row) * GameOptionsDefinitions.MenuTilemapWidth +
                anchor.X + column;
            page[destination] = cells is not null ? cells[row * GameOptionsPresentationDefinitions.ControllerLabelWidth + column]
                : GameOptionsArtworkCells.ControllerLabelTile(label, row * GameOptionsPresentationDefinitions.ControllerLabelWidth + column);
        }
    }

    /// <summary>Recolors the enabled and disabled labels for one special setting.</summary>
    internal void ApplySpecialToggle(Span<BackgroundTile> page, GameOptionsToggle name, bool enabled)
    {
        if (specialToggles is null)
        {
            ApplyPalette(page, GameOptionsArtworkCells.ToggleCells(name, GameOptionsToggleState.Enabled),
                enabled ? SelectedPalette : UnselectedPalette);
            ApplyPalette(page, GameOptionsArtworkCells.ToggleCells(name, GameOptionsToggleState.Disabled),
                enabled ? UnselectedPalette : SelectedPalette);
            return;
        }
        GameOptionsToggleVisualDocument toggle = specialToggles[name];
        ApplyPalette(page, toggle.EnabledCells,
            enabled ? SelectedPalette : UnselectedPalette);
        ApplyPalette(page, toggle.DisabledCells,
            enabled ? UnselectedPalette : SelectedPalette);
    }

    /// <summary>Gets the cursor origin for an authored page and selection row.</summary>
    internal Position16<SpritePixels> CursorPosition(GameOptionsPage page, int selectedItem)
    {
        if (!Enum.IsDefined(page))
            throw new ArgumentOutOfRangeException(nameof(page), page, "Options cursors exist only for the three menu pages.");
        if (cursorAnchors is null)
            return GameOptionsArtworkCells.CursorAnchor(page, selectedItem);
        Position16<SpritePixels>[] points = cursorAnchors[page];
        return (uint)selectedItem < points.Length
            ? points[selectedItem]
            : throw new InvalidDataException($"Options cursor {page}[{selectedItem}] is not authored.");
    }
    /// <summary>Draws a page heading at its authored origin after applying menu scroll.</summary>
    internal void DrawHeading(ScreenSprites sprites, SpriteSheet sheet, GameOptionsPage page, int verticalScroll)
    {
        Position16<SpritePixels> point = headingAnchors is null
            ? GameOptionsArtworkCells.HeadingAnchor(page)
            : headingAnchors[page];
        // The authored X is validated on screen; scrolling the heading wraps its Y word.
        Position16<SpritePixels> origin = new(checked(point.X), unchecked((ushort)(point.Y - verticalScroll)));
        sprites.AddImage(sheet.Frame(GameOptionsNames.HeadingSpriteName(page)), origin);
    }

    /// <summary>Draws one frame of the missile cursor at the selected position.</summary>
    internal static void DrawCursor(ScreenSprites sprites, SpriteSheet sheet, int frame, Position16<SpritePixels> point) =>
        sprites.AddImage(sheet.Frame(GameOptionsNames.CursorSpriteName(frame)), point);

    /// <summary>Changes only the colour variant of selected tilemap cells, preserving their artwork and mirroring.</summary>
    private static void ApplyPalette(Span<BackgroundTile> page, IEnumerable<int> cells, int palette)
    {
        foreach (int cell in cells)
            page[cell] = page[cell] with { ColourVariant = palette };
    }
}
