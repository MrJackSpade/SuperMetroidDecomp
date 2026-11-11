using SuperMetroid.AssetExtraction.Frontend.Map;
using SuperMetroid.AssetExtraction.Installation;
using System.Collections.Concurrent;
using System.Runtime.CompilerServices;
using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.AssetExtraction.Frontend;
using SuperMetroid.AssetExtraction.Frontend.Ending;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Frontend.Ending;
using SuperMetroid.Core.Frontend.Ending.Artwork;
using SuperMetroid.Core.Frontend.Intro.Artwork;
using SuperMetroid.Core.Frontend.Map.Artwork;
using SuperMetroid.AssetExtraction;
using SuperMetroid.AssetExtraction.Frontend.Ending.Artwork;

namespace SuperMetroid.TestSupport.RepositoryInstallation.Verification;

/// <summary>
/// Verification access (#1392): binds the ending sheets a fixture's ending artwork composes over the installed palettes and
/// font, as the game binds an installation's sheets, registered so OAM comparisons still apply to them.
/// </summary>
internal static class EndingSheetFixtures
{
    /// <summary>The installed ending artwork's identity, whose sheets are the installed ones.</summary>
    private static readonly Lazy<string> stockIdentity = new(() => RepositoryInstallation.Installation.LoadEndingObjectArt().ContentIdentity);

    /// <summary>The ending's decoded colours the composed sheets are coloured with (#1544).</summary>
    private static readonly Lazy<EndingPaletteSet> palettes = new(() => InstalledFixtures.EndingColours);

    /// <summary>The ending's background picture sheets its object artwork composes (#1392, #1467).</summary>
    private static readonly SpriteSheetId[] EndingPictureSheets = [SpriteSheetId.EndingPostCreditsPictures,
        SpriteSheetId.EndingWavySamusPicture, SpriteSheetId.EndingExplosionPictures];

    /// <summary>Installed sheets with the ending sheets already composed, by ending artwork identity.</summary>
    private static readonly ConcurrentDictionary<string, InstalledSpriteSheets> composed = new();

    /// <summary>The installed sheets with the ending sheets <paramref name="objects"/> composes.</summary>
    internal static InstalledSpriteSheets WithEnding(EndingObjectSourceArtwork objects)
    {
        ArgumentNullException.ThrowIfNull(objects);
        string identity = objects.ContentIdentity;
        return identity == stockIdentity.Value ? InstalledFixtures.SpriteSheets
            : composed.GetOrAdd(identity, key => Compose(objects, palettes.Value, InstalledFixtures.ImportMaps, key[..8], withFloors: false));
    }

    /// <summary>Installed sheets with the ending sheets composed over edited maps, by map presentation.</summary>
    private static readonly ConditionalWeakTable<ImportAreaMapPresentation, InstalledSpriteSheets> mapSheets = new();

    /// <summary>
    /// The installed sheets with the ending sheets the installed ending artwork composes over <paramref name="maps"/>' font
    /// and palette-FX colours: the ending's sprites, pictures and floors are pre-coloured from them (#1512), so edited
    /// palette-FX colours show only through them.
    /// </summary>
    internal static InstalledSpriteSheets WithEndingMaps(ImportAreaMapPresentation maps)
    {
        ArgumentNullException.ThrowIfNull(maps);
        return maps.Game.ContentIdentity == InstalledFixtures.Maps.ContentIdentity ? InstalledFixtures.SpriteSheets : mapSheets.GetValue(maps,
            key => Compose(RepositoryInstallation.Installation.LoadEndingObjectArt(), palettes.Value, key,
                $"maps-{RuntimeHelpers.GetHashCode(key):X8}", withFloors: true));
    }

    /// <summary>The ending's Mode 7 floor sheets, which its palettes and palette-FX colours pre-colour (#1392, #1512).</summary>
    private static readonly SpriteSheetId[] EndingFloorSheets = [SpriteSheetId.EndingEscapeAFloor, SpriteSheetId.EndingEscapeBFloor,
        SpriteSheetId.EndingPlanetExplosionFloor, SpriteSheetId.EndingRewardIconFloor];

    /// <summary>
    /// The installed sheets with the ending sheets <paramref name="objects"/> composes over <paramref name="colours"/> and
    /// <paramref name="maps"/>, named by <paramref name="tag"/>: its sprite and picture sheets, and its floors when
    /// <paramref name="withFloors"/>.
    /// </summary>
    private static InstalledSpriteSheets Compose(EndingObjectSourceArtwork objects, EndingPaletteSet colours, ImportAreaMapPresentation maps, string tag,
        bool withFloors)
    {
        var sheets = SpriteSheetDefinitions.Sheets.ToDictionary(id => id, InstalledFixtures.SpriteSheets.Get);
        foreach (SpriteSheetId sheet in EndingSpriteSheetsConstants.Sheets)
            sheets[sheet] = CinematicSheetFixtures.Load(EndingSpriteSheetComposer.Sources(sheet, objects, colours, maps.EndingFont, maps.RoomPaletteFx),
                $"{SpriteSheetDefinitions.FileNames[sheet]}-fixture-{tag}");
        // The ending's background pictures and floors are pre-coloured from these objects, colours and maps (#1392, #1467).
        var tileSources = new ScreenTileSources(() => maps, () => InstalledFixtures.IntroArtwork, () => objects, () => colours,
            RepositoryInstallation.Installation.LoadEndingMode7Art);
        foreach (SpriteSheetId sheet in EndingPictureSheets)
            sheets[sheet] = Loaded(sheet, EndingPictureSheetComposer.Compose(sheet, tileSources), tag);
        if (withFloors)
        {
            foreach (SpriteSheetId sheet in EndingFloorSheets)
                sheets[sheet] = Loaded(sheet, FrontendFloorSheetComposer.Compose(sheet, tileSources), tag);
        }
        return new InstalledSpriteSheets(sheets);
    }

    /// <summary>Loads composed tile sheet <paramref name="tiles"/> as sheet <paramref name="sheet"/>, named by <paramref name="tag"/>.</summary>
    private static SpriteSheet Loaded(SpriteSheetId sheet, ComposedSpriteSheet tiles, string tag) =>
        SpriteSheetFileConstants.Load(tiles.Manifest,
            $"{SpriteSheetDefinitions.FileNames[sheet]}-fixture-{tag}", tiles.Size, tiles.Pixels, tiles.PaletteIndices, tiles.FrontTiles);

    /// <summary>The installed sheets with the ending Mode 7 sheets edited <paramref name="mode7"/> art composes (#1392).</summary>
    internal static InstalledSpriteSheets WithEndingMode7(EndingFloorCharacters mode7)
    {
        ArgumentNullException.ThrowIfNull(mode7);
        var sheets = SpriteSheetDefinitions.Sheets.ToDictionary(id => id, InstalledFixtures.SpriteSheets.Get);
        var tileSources = new ScreenTileSources(() => InstalledFixtures.ImportMaps, () => InstalledFixtures.IntroArtwork,
            RepositoryInstallation.Installation.LoadEndingObjectArt, () => palettes.Value, () => mode7);
        foreach (SpriteSheetId sheet in EndingFloorSheets)
            sheets[sheet] = Loaded(sheet, FrontendFloorSheetComposer.Compose(sheet, tileSources), "mode7");
        return new InstalledSpriteSheets(sheets);
    }

    /// <summary>
    /// Draws <paramref name="family"/> from the sheet <paramref name="ending"/> draws <paramref name="objectSheet"/>'s page
    /// from now, as its own frames draw it.
    /// </summary>
    internal static CinematicSpriteSheet Drawer(EndingCreditsState ending, EndingObjectSheet objectSheet, CinematicSpriteFamily family)
    {
        ArgumentNullException.ThrowIfNull(ending);
        return new CinematicSpriteSheet(ending.InstalledArtwork.SpriteSheet(EndingSpriteSheets.For(objectSheet, ending.Stage.EndingReward)), family);
    }

    extension(EndingArtwork self)
    {
        /// <summary>Binds the ending sheets <paramref name="objects"/> composes, as the game binds an installation's sheets.</summary>
        internal void BindFixtureObjectArtwork(EndingObjectSourceArtwork objects) => self.BindSpriteSheets(WithEnding(objects));
    }
}
