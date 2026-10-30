using System.Runtime.CompilerServices;
using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.AssetExtraction.Frontend;
using SuperMetroid.AssetExtraction.Hud;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Frontend.Artwork;
using SuperMetroid.Core.Frontend.FileSelect.Artwork;
using SuperMetroid.Core.Frontend.Map.Artwork;
using SuperMetroid.Core.Frontend.Options.Artwork;
using SuperMetroid.Core.Frontend.Pause.Artwork;
using SuperMetroid.Core.Frontend.Title.Artwork;

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
        return fileSelect.ContentIdentity == InstalledFixtures.Maps.FileSelect.ContentIdentity
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
        return presentation.ContentIdentity == InstalledFixtures.Maps.GameOptions.ContentIdentity
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
        return ReferenceEquals(catalog, InstalledFixtures.Maps.Sprites)
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
        return ReferenceEquals(graphics, InstalledFixtures.Maps.TitleGraphics) ? InstalledFixtures.SpriteSheets
            : titles.GetValue(graphics, key =>
            {
                var sheets = SpriteSheetDefinitions.Sheets.ToDictionary(id => id, InstalledFixtures.SpriteSheets.Get);
                sheets[SpriteSheetId.Title] = Load(SpriteSheetId.Title, TitleSheetComposer.Sources(key, InstalledFixtures.Maps.TitlePalette),
                    $"title-{RuntimeHelpers.GetHashCode(key):X8}");
                // The title's Mode 7 characters are pre-coloured from these graphics (#1392).
                sheets[SpriteSheetId.TitleFloorTiles] = LoadTiles(SpriteSheetId.TitleFloorTiles,
                    ScreenTileSheetComposer.ComposeTitleMode7(key, InstalledFixtures.Maps.TitlePalette), $"title-{RuntimeHelpers.GetHashCode(key):X8}");
                sheets[SpriteSheetId.TitleBabyTiles] = LoadTiles(SpriteSheetId.TitleBabyTiles,
                    ScreenTileSheetComposer.ComposeTitleBaby(key, InstalledFixtures.Maps.TitlePalette), $"title-{RuntimeHelpers.GetHashCode(key):X8}");
                return new InstalledSpriteSheets(sheets);
            });
    }

    /// <summary>Loads one composed fixture tile sheet under a name of its own.</summary>
    private static SpriteSheet LoadTiles(SpriteSheetId id, ComposedSpriteSheet tiles, string fixture) =>
        SpriteSheet.Load(new MemoryStream(SpriteSheet.Write(tiles.Manifest), writable: false),
            $"{SpriteSheetDefinitions.FileNames[id]}-fixture-{fixture}", tiles.Size, tiles.Pixels, tiles.PaletteIndices, tiles.Priorities, tiles.PartGroups);

    /// <summary>Composes and registers a fixture presentation's menu sheets beside the installed others.</summary>
    private static InstalledSpriteSheets Compose(AreaMapPresentation maps)
    {
        var sheets = SpriteSheetDefinitions.Sheets.ToDictionary(id => id, InstalledFixtures.SpriteSheets.Get);
        foreach ((SpriteSheetId id, SpriteSheetSources sources) in new[]
                 {
                     (SpriteSheetId.GameOver, GameOverSheetComposer.Sources(maps)),
                     (SpriteSheetId.FileSelect, FileSelectSheetComposer.Sources(maps)),
                     (SpriteSheetId.GameOptions, GameOptionsSheetComposer.Sources(maps)),
                     (SpriteSheetId.MapSprites, MapSpriteSheetComposer.Sources(maps)),
                     (SpriteSheetId.PauseSprites, PauseSheetComposer.Sources(maps)),
                     (SpriteSheetId.Title, TitleSheetComposer.Sources(maps)),
                 })
            sheets[id] = Load(id, sources, $"maps-{RuntimeHelpers.GetHashCode(maps):X8}");
        // The menus' background tiles are pre-coloured from the maps' characters and palettes (#1392).
        foreach (SpriteSheetId id in ScreenTileSheetDefinitions.Depths.Keys)
        {
            ComposedSpriteSheet tiles = ScreenTileSheetComposer.Compose(id, new ScreenTileSources(() => maps, () => InstalledFixtures.IntroArtwork,
                RepositoryInstallation.Installation.LoadEndingObjectArt, RepositoryInstallation.Installation.LoadEndingPalettes,
                RepositoryInstallation.Installation.LoadEndingMode7Art));
            sheets[id] = SpriteSheet.Load(new MemoryStream(SpriteSheet.Write(tiles.Manifest), writable: false),
                $"{SpriteSheetDefinitions.FileNames[id]}-fixture-maps-{RuntimeHelpers.GetHashCode(maps):X8}", tiles.Size, tiles.Pixels,
                tiles.PaletteIndices, tiles.Priorities, tiles.PartGroups);
        }
        // Pause's HUD plane draws the HUD's images, pictured from the maps' HUD artwork and characters (#1466).
        sheets[SpriteSheetId.PauseHud] = LoadTiles(SpriteSheetId.PauseHud, HudImageSheet.ComposePause(maps), $"maps-{RuntimeHelpers.GetHashCode(maps):X8}");
        // The menus' pages and the pictures placed over them are pictured from the maps' presentation (#1467).
        sheets[SpriteSheetId.MenuPictures] = LoadTiles(SpriteSheetId.MenuPictures, MenuPictureSheet.Compose(maps), $"maps-{RuntimeHelpers.GetHashCode(maps):X8}");
        return new InstalledSpriteSheets(sheets);
    }

    /// <summary>Composes, loads and registers one fixture sheet under a name of its own.</summary>
    private static SpriteSheet Load(SpriteSheetId id, SpriteSheetSources sources, string fixture)
    {
        ComposedSpriteSheet sheet = SpriteSheetComposer.Compose(sources, 0, 0);
        // A name of its own keeps its frames apart from the installed sheet's in the frame-source registry.
        SpriteSheet loaded = SpriteSheet.Load(new MemoryStream(SpriteSheet.Write(sheet.Manifest), writable: false),
            $"{SpriteSheetDefinitions.FileNames[id]}-fixture-{fixture}", sheet.Size, sheet.Pixels, sheet.PaletteIndices, sheet.Priorities, sheet.PartGroups);
        SheetFrameSources.Register(loaded, sources);
        return loaded;
    }
}
