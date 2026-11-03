using SuperMetroid.AssetExtraction.Areas.Ceres;
using SuperMetroid.AssetExtraction.Frontend;
using System.Runtime.CompilerServices;
using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.AssetExtraction.Frontend.Intro;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Frontend.CeresCinematics;
using SuperMetroid.Core.Frontend.CeresCinematics.Artwork;
using SuperMetroid.Core.Frontend.Intro;
using SuperMetroid.Core.Frontend.Intro.Artwork;
using SuperMetroid.AssetExtraction.Frontend.Intro.Artwork;
using SuperMetroid.Core.Fx.Artwork;

namespace SuperMetroid.TestSupport.RepositoryInstallation.Verification;

/// <summary>
/// Verification access (#1392): draws a fixture's cinematic artwork as the game draws an installation's, from sheets that
/// artwork composes over the installed intro OBJ page and colours, registered so OAM comparisons still apply to them.
/// </summary>
internal static class CinematicSheetFixtures
{
    /// <summary>Installed sheets with an intro sheet already composed, by intro artwork.</summary>
    private static readonly ConditionalWeakTable<IntroCinematicArtwork, InstalledSpriteSheets> intros = new();

    /// <summary>Drawers already composed, by presentation.</summary>
    private static readonly ConditionalWeakTable<object, CinematicSpriteSheet> drawers = new();

    /// <summary>Ceres flight sheets already composed, by flight artwork.</summary>
    private static readonly ConditionalWeakTable<CeresFlightArtwork, SpriteSheet> flights = new();

    /// <summary>The Ceres flight sheet <paramref name="flight"/> composes.</summary>
    internal static SpriteSheet CeresFlight(CeresFlightArtwork flight)
    {
        ArgumentNullException.ThrowIfNull(flight);
        return ReferenceEquals(flight, InstalledFixtures.IntroArtwork.CeresFlight)
            ? InstalledFixtures.SpriteSheets.Get(SpriteSheetId.CeresFlightSprites)
            : flights.GetValue(flight, key => Load(IntroSpriteSheetComposer.CeresFlightSources(key),
                $"ceres-flight-fixture-{RuntimeHelpers.GetHashCode(key):X8}"));
    }

    extension(CeresDestructionCinematicState self)
    {
        /// <summary>Binds <paramref name="intro"/> and the sheets it composes, as the game rebinds both.</summary>
        internal void BindFixtureDestructionArtwork(IntroCinematicArtwork intro)
        {
            self.BindArtwork(intro);
            self.BindSpriteSheets(WithIntro(intro));
        }
    }

    extension(IntroCeresFlightState self)
    {
        /// <summary>Binds <paramref name="flight"/> and the sheets it composes, as the game rebinds both.</summary>
        internal void BindFixtureFlightArtwork(CeresFlightArtwork flight)
        {
            self.BindArtwork(flight);
            self.BindSheets(WithFlight(flight));
        }
    }

    /// <summary>Installed sheets with the Ceres flight's sprites, caption pictures and floor composed, by flight artwork.</summary>
    private static readonly ConditionalWeakTable<CeresFlightArtwork, InstalledSpriteSheets> flightSheets = new();

    /// <summary>The installed sheets with the Ceres flight sheet, caption pictures and floor <paramref name="flight"/> composes.</summary>
    internal static InstalledSpriteSheets WithFlight(CeresFlightArtwork flight)
    {
        ArgumentNullException.ThrowIfNull(flight);
        return ReferenceEquals(flight, InstalledFixtures.IntroArtwork.CeresFlight) ? InstalledFixtures.SpriteSheets
            : flightSheets.GetValue(flight, key =>
            {
                var sheets = SpriteSheetDefinitions.Sheets.ToDictionary(id => id, InstalledFixtures.SpriteSheets.Get);
                sheets[SpriteSheetId.CeresFlightSprites] = CeresFlight(key);
                // The caption is painted from the flight's characters and palette, so edited flight art recomposes it.
                sheets[SpriteSheetId.CeresCinematicPictures] = LoadTiles(SpriteSheetId.CeresCinematicPictures,
                    CeresCinematicPictureSheetComposer.Compose(key, InstalledFixtures.IntroArtwork, InstalledFixtures.Maps.RoomPaletteFx), $"flight-{RuntimeHelpers.GetHashCode(key):X8}");
                sheets[SpriteSheetId.CeresFloor] = LoadTiles(SpriteSheetId.CeresFloor,
                    FrontendFloorSheetComposer.Ceres(key, IntroCinematicSourceArtwork.Of(InstalledFixtures.IntroArtwork).CeresDestructionMaps, InstalledFixtures.Maps.RoomPaletteFx), $"flight-{RuntimeHelpers.GetHashCode(key):X8}");
                return new InstalledSpriteSheets(sheets);
            });
    }

    /// <summary>
    /// The installed Ceres floor sheet pre-coloured with the gunship engine flicker's records from <paramref name="paletteFx"/>
    /// (#1512), as an installation with those palette-effect colours composes it.
    /// </summary>
    internal static SpriteSheet CeresFloor(IPaletteFxColorSource paletteFx)
    {
        ArgumentNullException.ThrowIfNull(paletteFx);
        return ReferenceEquals(paletteFx, InstalledFixtures.Maps.RoomPaletteFx)
            ? InstalledFixtures.SpriteSheets.Get(SpriteSheetId.CeresFloor)
            : LoadTiles(SpriteSheetId.CeresFloor, FrontendFloorSheetComposer.Ceres(InstalledFixtures.IntroArtwork, paletteFx),
                $"engine-{RuntimeHelpers.GetHashCode(paletteFx):X8}");
    }

    /// <summary>Loads one composed fixture tile sheet under a name of its own.</summary>
    private static SpriteSheet LoadTiles(SpriteSheetId id, ComposedSpriteSheet tiles, string fixture) =>
        SpriteSheetFile.Load(new MemoryStream(SpriteSheetManifestWriter.Write(tiles.Manifest), writable: false),
            $"{SpriteSheetDefinitions.FileNames[id]}-fixture-{fixture}", tiles.Size, tiles.Pixels, tiles.PaletteIndices, tiles.Priorities);

    /// <summary>The installed sheets with the intro sheet <paramref name="intro"/> composes.</summary>
    internal static InstalledSpriteSheets WithIntro(IntroCinematicArtwork intro)
    {
        ArgumentNullException.ThrowIfNull(intro);
        return ReferenceEquals(intro, InstalledFixtures.IntroArtwork) ? InstalledFixtures.SpriteSheets
            : intros.GetValue(intro, key =>
            {
                var sheets = SpriteSheetDefinitions.Sheets.ToDictionary(id => id, InstalledFixtures.SpriteSheets.Get);
                sheets[SpriteSheetId.IntroSprites] = Load(IntroSpriteSheetComposer.Sources(key), $"intro-fixture-{RuntimeHelpers.GetHashCode(key):X8}");
                sheets[SpriteSheetId.CeresFlightSprites] = CeresFlight(key.CeresFlight);
                sheets[SpriteSheetId.CeresDestructionSprites] = Load(IntroSpriteSheetComposer.CeresDestructionSources(key), $"ceres-destruction-fixture-{RuntimeHelpers.GetHashCode(key):X8}");
                sheets[SpriteSheetId.ZebesRevealSprites] = Load(IntroSpriteSheetComposer.ZebesRevealSources(key), $"zebes-reveal-fixture-{RuntimeHelpers.GetHashCode(key):X8}");
                // The cinematics' background tiles are pre-coloured from the intro's characters and palettes (#1392).
                var tileSources = new ScreenTileSources(() => InstalledFixtures.Maps, () => key,
                    RepositoryInstallation.Installation.LoadEndingObjectArt, RepositoryInstallation.Installation.LoadEndingPalettes,
                    RepositoryInstallation.Installation.LoadEndingMode7Art);
                sheets[SpriteSheetId.IntroNarrationPictures] = LoadTiles(SpriteSheetId.IntroNarrationPictures,
                    IntroNarrationPictureSheetComposer.Compose(key, InstalledFixtures.Maps.IntroFont), $"intro-{RuntimeHelpers.GetHashCode(key):X8}");
                sheets[SpriteSheetId.IntroScenePictures] = LoadTiles(SpriteSheetId.IntroScenePictures,
                    IntroScenePictureSheetComposer.Compose(key), $"intro-{RuntimeHelpers.GetHashCode(key):X8}");
                sheets[SpriteSheetId.CeresCinematicPictures] = LoadTiles(SpriteSheetId.CeresCinematicPictures,
                    CeresCinematicPictureSheetComposer.Compose(key, InstalledFixtures.Maps.RoomPaletteFx), $"intro-{RuntimeHelpers.GetHashCode(key):X8}");
                sheets[SpriteSheetId.CeresFloor] = LoadTiles(SpriteSheetId.CeresFloor,
                    FrontendFloorSheetComposer.Ceres(key, InstalledFixtures.Maps.RoomPaletteFx), $"intro-{RuntimeHelpers.GetHashCode(key):X8}");
                return new InstalledSpriteSheets(sheets);
            });
    }

    extension(IntroCinematicState self)
    {
        /// <summary>Binds <paramref name="intro"/> and the sheets with the intro sheet it composes, as the game rebinds both.</summary>
        internal void BindFixtureIntroArtwork(IntroCinematicArtwork intro)
        {
            self.BindCharacterArtwork(intro);
            self.BindSpriteSheets(WithIntro(intro));
        }
    }

    /// <summary>Draws <paramref name="presentation"/>'s frames from a sheet it composes.</summary>
    internal static CinematicSpriteSheet Drawer(ICinematicSpriteFrames presentation)
    {
        ArgumentNullException.ThrowIfNull(presentation);
        return drawers.GetValue(presentation, key =>
        {
            var frames = (ICinematicSpriteFrames)key;
            return new CinematicSpriteSheet(Load(IntroSpriteSheetComposer.Sources([frames], InstalledFixtures.IntroArtwork),
                $"cinematic-fixture-{frames.Family}-{RuntimeHelpers.GetHashCode(key):X8}"), frames.Family);
        });
    }

    /// <summary>Composes, loads and registers one fixture sheet under a name of its own.</summary>
    internal static SpriteSheet Load(SpriteSheetSources sources, string name)
    {
        ComposedSpriteSheet composed = SpriteSheetComposer.Compose(sources, 0, 0);
        SpriteSheet sheet = SpriteSheetFile.Load(new MemoryStream(SpriteSheetManifestWriter.Write(composed.Manifest), writable: false), name,
            composed.Size, composed.Pixels, composed.PaletteIndices, composed.Priorities);
        SheetFrameSources.Register(sheet, sources);
        return sheet;
    }
}
