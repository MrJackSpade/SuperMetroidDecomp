using SuperMetroid.AssetExtraction.Frontend.Title.Artwork;
using System.Runtime.CompilerServices;
using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.AssetExtraction.Frontend;
using SuperMetroid.AssetExtraction.Frontend.Map;
using SuperMetroid.AssetExtraction.Frontend.Map.Artwork;
using SuperMetroid.AssetExtraction.Frontend.Pause;
using SuperMetroid.AssetExtraction.Hud;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Frontend.Artwork;
using SuperMetroid.Core.Frontend.FileSelect.Artwork;
using SuperMetroid.Core.Frontend.Map.Artwork;
using SuperMetroid.Core.Frontend.Options.Artwork;
using SuperMetroid.Core.Frontend.Pause.Artwork;
using SuperMetroid.Core.Frontend.Title.Artwork;
using SuperMetroid.Core.Progression;
using SuperMetroid.AssetExtraction.Frontend.FileSelect.Artwork;
using SuperMetroid.AssetExtraction.Frontend.Options.Artwork;

namespace SuperMetroid.TestSupport.RepositoryInstallation.Verification;

/// <summary>
/// Verification access (#1392): the sprite sheets a fixture's menus draw from. The installation's menu presentation
/// draws from the installed sheets; a fixture's own presentation gets its menu sheets composed from it, as an
/// installation of that presentation would hold, registered so OAM comparisons still apply to them.
/// </summary>
internal static class MenuSheetFixtures
{
    /// <summary>Sheets already composed, by presentation.</summary>
    private static readonly ConditionalWeakTable<AreaMapPresentation, InstalledSpriteSheets> composed = new();

    /// <summary>The installed sheets with the menu sheets <paramref name="maps"/> composes.</summary>
    internal static InstalledSpriteSheets For(AreaMapPresentation maps)
    {
        ArgumentNullException.ThrowIfNull(maps);
        return maps.ContentIdentity == InstalledFixtures.Maps.ContentIdentity ? InstalledFixtures.SpriteSheets : composed.GetValue(maps, Compose);
    }

    /// <summary>File-select sheets already composed, by presentation.</summary>
    private static readonly ConditionalWeakTable<FileSelectPresentation, SpriteSheet> fileSelects = new();

    /// <summary>The file-select sheet <paramref name="fileSelect"/> composes over the installation's menus.</summary>
    internal static SpriteSheet FileSelect(FileSelectPresentation fileSelect)
    {
        ArgumentNullException.ThrowIfNull(fileSelect);
        return FileSelectSourcePresentation.Of(fileSelect).ContentIdentity == FileSelectSourcePresentation.Of(InstalledFixtures.Maps.FileSelect).ContentIdentity
            ? InstalledFixtures.SpriteSheets.Get(SpriteSheetId.FileSelect)
            : fileSelects.GetValue(fileSelect, key => Load(SpriteSheetId.FileSelect, FileSelectSheetComposer.Sources(key, InstalledFixtures.Maps),
                $"presentation-{RuntimeHelpers.GetHashCode(key):X8}"));
    }

    /// <summary>Options sheets already composed, by presentation.</summary>
    private static readonly ConditionalWeakTable<GameOptionsPresentation, SpriteSheet> options = new();

    /// <summary>The options sheet <paramref name="presentation"/> composes over the installation's menus.</summary>
    internal static SpriteSheet GameOptions(GameOptionsPresentation presentation)
    {
        ArgumentNullException.ThrowIfNull(presentation);
        return GameOptionsSourcePresentation.Of(presentation).ContentIdentity == GameOptionsSourcePresentation.Of(InstalledFixtures.Maps.GameOptions).ContentIdentity
            ? InstalledFixtures.SpriteSheets.Get(SpriteSheetId.GameOptions)
            : options.GetValue(presentation, key => Load(SpriteSheetId.GameOptions, GameOptionsSheetComposer.Sources(key, InstalledFixtures.Maps),
                $"presentation-{RuntimeHelpers.GetHashCode(key):X8}"));
    }

    /// <summary>Map sprite sheets already composed, by catalog.</summary>
    private static readonly ConditionalWeakTable<MapSpriteCompositions, SpriteSheet> mapSprites = new();

    /// <summary>The map sprite sheet <paramref name="catalog"/> composes, coloured with the installation's menus.</summary>
    internal static SpriteSheet MapSprites(MapSpriteCompositions catalog)
    {
        ArgumentNullException.ThrowIfNull(catalog);
        return ReferenceEquals(catalog, InstalledAreaMapPresentation.SpritesOf(InstalledFixtures.Maps))
            ? InstalledFixtures.SpriteSheets.Get(SpriteSheetId.MapSprites)
            : mapSprites.GetValue(catalog, key => Load(SpriteSheetId.MapSprites, MapSpriteSheetComposer.Sources(key, InstalledFixtures.Maps),
                $"catalog-{RuntimeHelpers.GetHashCode(key):X8}"));
    }

    /// <summary>Pause sheets already composed, by selector presentation.</summary>
    private static readonly ConditionalWeakTable<PauseSelectorPresentation, SpriteSheet> pauseSprites = new();

    /// <summary>The pause sheet <paramref name="selectors"/> composes beside the installation's reserve strip.</summary>
    internal static SpriteSheet PauseSprites(PauseSelectorPresentation selectors)
    {
        ArgumentNullException.ThrowIfNull(selectors);
        return ReferenceEquals(selectors, InstalledFixtures.Maps.PauseSelectors)
            ? InstalledFixtures.SpriteSheets.Get(SpriteSheetId.PauseSprites)
            : pauseSprites.GetValue(selectors, key => Load(SpriteSheetId.PauseSprites,
                PauseSheetComposer.Sources(key, InstalledFixtures.Maps.PauseReserveTanks, InstalledFixtures.Maps),
                $"selectors-{RuntimeHelpers.GetHashCode(key):X8}"));
    }

    /// <summary>Pause sheets already composed, by reserve-strip presentation.</summary>
    private static readonly ConditionalWeakTable<PauseReserveTankPresentation, SpriteSheet> pauseTanks = new();

    /// <summary>The pause sheet <paramref name="tanks"/> composes beside the installation's selectors.</summary>
    internal static SpriteSheet PauseSprites(PauseReserveTankPresentation tanks)
    {
        ArgumentNullException.ThrowIfNull(tanks);
        return ReferenceEquals(tanks, InstalledFixtures.Maps.PauseReserveTanks)
            ? InstalledFixtures.SpriteSheets.Get(SpriteSheetId.PauseSprites)
            : pauseTanks.GetValue(tanks, key => Load(SpriteSheetId.PauseSprites,
                PauseSheetComposer.Sources(InstalledFixtures.Maps.PauseSelectors, key, InstalledFixtures.Maps),
                $"tanks-{RuntimeHelpers.GetHashCode(key):X8}"));
    }

    /// <summary>Installed sheets with a title sheet already composed, by title graphics.</summary>
    private static readonly ConditionalWeakTable<TitleGraphicsPresentation, InstalledSpriteSheets> titles = new();

    /// <summary>
    /// The installed sheets with the title sheet <paramref name="graphics"/> composes, coloured with the installation's
    /// title palette; a fixture's edited colours reach the screen through the live-colour path.
    /// </summary>
    internal static InstalledSpriteSheets WithTitle(TitleGraphicsPresentation graphics)
    {
        ArgumentNullException.ThrowIfNull(graphics);
        return ReferenceEquals(graphics, InstalledFixtures.Maps.TitleGraphics) || ShowsStockTitle(graphics) ? InstalledFixtures.SpriteSheets
            : titles.GetValue(graphics, ComposedTitle);
    }

    /// <summary>
    /// Title graphics whose sheets were composed, with those sheets (#1532): graphics read again from the same source hold
    /// the same content, so they draw from the sheets already composed for that content instead of composing them again.
    /// </summary>
    private static readonly List<(SuperMetroid.AssetExtraction.Frontend.Title.TitleGraphicsCharacters Graphics, InstalledSpriteSheets Sheets)> composedTitles = [];

    /// <summary>
    /// The installed sheets with the title and title floor sheets <paramref name="graphics"/> composes: those already composed
    /// for the same content, or composed now.
    /// </summary>
    private static InstalledSpriteSheets ComposedTitle(TitleGraphicsPresentation graphics)
    {
        if (graphics is not SuperMetroid.AssetExtraction.Frontend.Title.TitleGraphicsCharacters characters)
            return ComposeTitle(graphics);
        lock (composedTitles)
        {
            foreach ((SuperMetroid.AssetExtraction.Frontend.Title.TitleGraphicsCharacters composed, InstalledSpriteSheets sheets) in composedTitles)
                if (SameTitleContent(characters, composed))
                    return sheets;
            InstalledSpriteSheets made = ComposeTitle(graphics);
            composedTitles.Add((characters, made));
            return made;
        }
    }

    /// <summary>Composes the installed sheets with the title and title floor sheets <paramref name="graphics"/> composes.</summary>
    private static InstalledSpriteSheets ComposeTitle(TitleGraphicsPresentation graphics)
    {
        var sheets = SpriteSheetDefinitions.Sheets.ToDictionary(id => id, InstalledFixtures.SpriteSheets.Get);
        sheets[SpriteSheetId.Title] = Load(SpriteSheetId.Title, TitleSheetComposer.Sources(graphics, InstalledAreaMapPresentation.TitlePaletteOf(InstalledFixtures.Maps)),
            $"title-{RuntimeHelpers.GetHashCode(graphics):X8}");
        // The title's floor is composed from these graphics (#1392, #1467).
        sheets[SpriteSheetId.TitleFloor] = LoadTiles(SpriteSheetId.TitleFloor,
            FrontendFloorSheetComposer.Title(graphics, InstalledAreaMapPresentation.TitlePaletteOf(InstalledFixtures.Maps)), $"title-{RuntimeHelpers.GetHashCode(graphics):X8}");
        return new InstalledSpriteSheets(sheets);
    }

    /// <summary>Installed sheets with a title sheet already composed, by title palette.</summary>
    private static readonly ConditionalWeakTable<TitlePalettePresentation, InstalledSpriteSheets> titlePalettes = new();

    /// <summary>
    /// Whether <paramref name="graphics"/> holds the installation's title graphics: the same content, read again (#1532),
    /// whose sheets are the installed sheets.
    /// </summary>
    private static bool ShowsStockTitle(TitleGraphicsPresentation graphics) =>
        graphics is SuperMetroid.AssetExtraction.Frontend.Title.TitleGraphicsCharacters characters
        && SameTitleContent(characters, InstalledFixtures.Maps.TitleGraphics as SuperMetroid.AssetExtraction.Frontend.Title.TitleGraphicsCharacters
            ?? throw new InvalidOperationException("The installation's title graphics are not installed title characters."));

    /// <summary>
    /// Whether two title graphics hold the same content: the floor map, the floor, object and Baby characters, and every
    /// title composition's parts.
    /// </summary>
    private static bool SameTitleContent(SuperMetroid.AssetExtraction.Frontend.Title.TitleGraphicsCharacters first,
        SuperMetroid.AssetExtraction.Frontend.Title.TitleGraphicsCharacters second) =>
        first.FloorMap.SequenceEqual(second.FloorMap) && first.FloorCharacters.SequenceEqual(second.FloorCharacters)
        && first.BabyCharacters.SequenceEqual(second.BabyCharacters) && first.ObjectPage.Characters.SequenceEqual(second.ObjectPage.Characters)
        && Enum.GetValues<SuperMetroid.Core.Frontend.Title.TitleSprite>().All(sprite =>
            first.Frame(sprite, sprite.ToString()).Parts.SequenceEqual(second.Frame(sprite, sprite.ToString()).Parts));

    /// <summary>
    /// The installed sheets with the title and title floor sheets the installation's title graphics compose under
    /// <paramref name="palette"/> (#1512): a fixture's edited title colours reach the screen through the shades the import
    /// pre-colours its sheets in.
    /// </summary>
    internal static InstalledSpriteSheets WithTitle(TitlePalettePresentation palette)
    {
        ArgumentNullException.ThrowIfNull(palette);
        return ReferenceEquals(palette, InstalledAreaMapPresentation.TitlePaletteOf(InstalledFixtures.Maps)) ? InstalledFixtures.SpriteSheets
            : titlePalettes.GetValue(palette, key =>
            {
                TitleGraphicsPresentation graphics = InstalledFixtures.Maps.TitleGraphics;
                var sheets = SpriteSheetDefinitions.Sheets.ToDictionary(id => id, InstalledFixtures.SpriteSheets.Get);
                sheets[SpriteSheetId.Title] = Load(SpriteSheetId.Title, TitleSheetComposer.Sources(graphics, key),
                    $"title-palette-{RuntimeHelpers.GetHashCode(key):X8}");
                sheets[SpriteSheetId.TitleFloor] = LoadTiles(SpriteSheetId.TitleFloor,
                    FrontendFloorSheetComposer.Title(graphics, key), $"title-palette-{RuntimeHelpers.GetHashCode(key):X8}");
                return new InstalledSpriteSheets(sheets);
            });
    }

    /// <summary>Loads one composed fixture tile sheet under a name of its own.</summary>
    private static SpriteSheet LoadTiles(SpriteSheetId id, ComposedSpriteSheet tiles, string fixture) =>
        SpriteSheetFileConstants.Load(new MemoryStream(SpriteSheetManifestWriter.Write(tiles.Manifest), writable: false),
            $"{SpriteSheetDefinitions.FileNames[id]}-fixture-{fixture}", tiles.Size, tiles.Pixels, tiles.PaletteIndices, tiles.FrontTiles);

    /// <summary>File-select map sheets already composed, by presentation.</summary>
    private static readonly ConditionalWeakTable<AreaMapPresentation, InstalledSpriteSheets> fileSelectMaps = new();

    /// <summary>
    /// The sheets the file-select map draws from (#1532): the installed sheets for the installation's presentation, otherwise
    /// only the sheets <paramref name="maps"/> composes for the file-select map's area and room screens: the map sprites, the
    /// menu and room-map pictures and each area's world-map pages. A screen that asks for any other sheet fails, so a fixture
    /// presentation's map screens never compose the pause, title and other menus' sheets they never draw.
    /// </summary>
    internal static InstalledSpriteSheets ForFileSelectMap(AreaMapPresentation maps)
    {
        ArgumentNullException.ThrowIfNull(maps);
        return maps.ContentIdentity == InstalledFixtures.Maps.ContentIdentity ? InstalledFixtures.SpriteSheets
            : fileSelectMaps.GetValue(maps, key =>
            {
                var sheets = new Dictionary<SpriteSheetId, SpriteSheet>();
                ComposeFileSelectMapSheets(key, sheets, $"file-select-map-{RuntimeHelpers.GetHashCode(key):X8}");
                return new InstalledSpriteSheets(sheets);
            });
    }

    /// <summary>Composes and registers a fixture presentation's menu sheets beside the installed others.</summary>
    private static InstalledSpriteSheets Compose(AreaMapPresentation maps)
    {
        var sheets = SpriteSheetDefinitions.Sheets.ToDictionary(id => id, InstalledFixtures.SpriteSheets.Get);
        foreach ((SpriteSheetId id, SpriteSheetSources sources) in new[]
                 {
                     (SpriteSheetId.GameOver, GameOverSheetComposer.Sources(maps)),
                     (SpriteSheetId.FileSelect, FileSelectSheetComposer.Sources(maps)),
                     (SpriteSheetId.GameOptions, GameOptionsSheetComposer.Sources(maps)),
                     (SpriteSheetId.PauseSprites, PauseSheetComposer.Sources(maps)),
                     (SpriteSheetId.Title, TitleSheetComposer.Sources(maps)),
                 })
            sheets[id] = Load(id, sources, $"maps-{RuntimeHelpers.GetHashCode(maps):X8}");
        // Pause's HUD plane draws the HUD's images, pictured from the maps' HUD artwork and characters (#1466).
        sheets[SpriteSheetId.PauseHud] = LoadTiles(SpriteSheetId.PauseHud, HudImageSheet.ComposePause(maps), $"maps-{RuntimeHelpers.GetHashCode(maps):X8}");
        // The pause screen's map cells, backdrops, buttons and equipment page are pictured from the maps' presentation (#1467).
        sheets[SpriteSheetId.PausePictures] = LoadTiles(SpriteSheetId.PausePictures, PausePictureSheet.Compose(maps), $"maps-{RuntimeHelpers.GetHashCode(maps):X8}");
        ComposeFileSelectMapSheets(maps, sheets, $"maps-{RuntimeHelpers.GetHashCode(maps):X8}");
        return new InstalledSpriteSheets(sheets);
    }

    /// <summary>
    /// Composes and registers into <paramref name="sheets"/> the sheets <paramref name="maps"/> pictures the file-select map
    /// with: its map sprites, the menus' and room maps' pictures and each area's world-map pages, each named for
    /// <paramref name="fixture"/>.
    /// </summary>
    private static void ComposeFileSelectMapSheets(AreaMapPresentation maps, Dictionary<SpriteSheetId, SpriteSheet> sheets, string fixture)
    {
        sheets[SpriteSheetId.MapSprites] = Load(SpriteSheetId.MapSprites, MapSpriteSheetComposer.Sources(maps), fixture);
        // The menus' pages and the pictures placed over them are pictured from the maps' presentation (#1467).
        sheets[SpriteSheetId.MenuPictures] = LoadTiles(SpriteSheetId.MenuPictures, MenuPictureSheet.Compose(maps), fixture);
        // The room maps' cells and frames, and each area's world-map pages, are pictured from the maps' presentation (#1467).
        sheets[SpriteSheetId.RoomMapPictures] = LoadTiles(SpriteSheetId.RoomMapPictures, MapPictureSheets.ComposeRoomMap(maps), fixture);
        foreach (KeyValuePair<AreaId, SpriteSheetId> foreground in ScreenTileSheetDefinitions.AreaMapForegrounds)
            sheets[foreground.Value] = LoadTiles(foreground.Value, MapPictureSheets.ComposeWorldForeground(maps, foreground.Key), fixture);
        foreach (KeyValuePair<AreaId, SpriteSheetId> background in ScreenTileSheetDefinitions.AreaMapBackgrounds)
            sheets[background.Value] = LoadTiles(background.Value, MapPictureSheets.ComposeWorldBackground(maps, background.Key), fixture);
    }

    /// <summary>Composes, loads and registers one fixture sheet under a name of its own.</summary>
    private static SpriteSheet Load(SpriteSheetId id, SpriteSheetSources sources, string fixture)
    {
        ComposedSpriteSheet sheet = SpriteSheetComposer.Compose(sources, 0, 0);
        // A name of its own keeps its frames apart from the installed sheet's in the frame-source registry.
        SpriteSheet loaded = SpriteSheetFileConstants.Load(new MemoryStream(SpriteSheetManifestWriter.Write(sheet.Manifest), writable: false),
            $"{SpriteSheetDefinitions.FileNames[id]}-fixture-{fixture}", sheet.Size, sheet.Pixels, sheet.PaletteIndices, sheet.FrontTiles);
        SheetFrameSources.Register(loaded, sources);
        return loaded;
    }
}
