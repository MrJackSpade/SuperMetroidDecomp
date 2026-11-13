using SuperMetroid.AssetExtraction.Areas.Ceres;
using SuperMetroid.AssetExtraction.Frontend;
using System.Runtime.CompilerServices;
using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.AssetExtraction.Frontend.Intro;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Frontend.CeresCinematics;
using SuperMetroid.Core.Frontend.Intro;
using SuperMetroid.Core.Frontend.Intro.Artwork;
using SuperMetroid.AssetExtraction.Frontend.Intro.Artwork;
using SuperMetroid.AssetExtraction.Frontend.Menus;
using SuperMetroid.AssetExtraction.Fx.Artwork;

namespace SuperMetroid.TestSupport.RepositoryInstallation.Verification;

/// <summary>
/// Verification access (#1392): draws a fixture's cinematic artwork as the game draws an installation's, from sheets that
/// artwork composes over the installed intro OBJ page and colours, registered so OAM comparisons still apply to them.
/// </summary>
internal static class CinematicSheetFixtures
{
    /// <summary>Installed sheets with an intro sheet already composed, by the intro artwork's content identity (#1532).</summary>
    private static readonly System.Collections.Concurrent.ConcurrentDictionary<string, InstalledSpriteSheets> intros = new(StringComparer.Ordinal);

    /// <summary>Each artwork object's content identity, computed once per object.</summary>
    private static readonly ConditionalWeakTable<object, string> identities = new();

    /// <summary>The content identity of <paramref name="intro"/>, which covers every artwork its sheets compose from.</summary>
    private static string Identity(ImportIntroCinematicArtwork intro) =>
        identities.GetValue(intro, key => ((ImportIntroCinematicArtwork)key).ContentIdentity);

    /// <summary>The content identity of <paramref name="flight"/>, which covers every artwork its sheets compose from.</summary>
    private static string Identity(CeresFlightCharacters flight) =>
        identities.GetValue(flight, key => ((CeresFlightCharacters)key).ContentIdentity);

    /// <summary>Drawers already composed, by presentation.</summary>
    private static readonly ConditionalWeakTable<object, CinematicSpriteSheet> drawers = new();

    /// <summary>Ceres flight sheets already composed, by the flight artwork's content identity (#1532).</summary>
    private static readonly System.Collections.Concurrent.ConcurrentDictionary<string, SpriteSheet> flights = new(StringComparer.Ordinal);

    /// <summary>The Ceres flight sheet <paramref name="flight"/> composes.</summary>
    internal static SpriteSheet CeresFlight(CeresFlightCharacters flight)
    {
        ArgumentNullException.ThrowIfNull(flight);
        return Identity(flight) == Identity(InstalledFixtures.IntroArtwork.FlightCharacters)
            ? InstalledFixtures.SpriteSheets.Get(SpriteSheetId.CeresFlightSprites)
            : flights.GetOrAdd(Identity(flight), _ => Load(IntroSpriteSheetComposer.CeresFlightSources(flight, InstalledFixtures.IntroArtwork.FlightPalette),
                $"ceres-flight-fixture-{RuntimeHelpers.GetHashCode(flight):X8}"));
    }

    extension(CeresDestructionCinematicState self)
    {
        /// <summary>Binds the catalog of <paramref name="intro"/> and the sheets it composes, as the game rebinds both.</summary>
        internal void BindFixtureDestructionArtwork(ImportIntroCinematicArtwork intro)
        {
            self.BindArtwork(intro.BoundCatalog);
            self.BindSpriteSheets(WithIntro(intro));
        }
    }

    extension(IntroCeresFlightState self)
    {
        /// <summary>Binds the catalog of <paramref name="flight"/> and the sheets it composes, as the game rebinds both.</summary>
        internal void BindFixtureFlightArtwork(CeresFlightCharacters flight)
        {
            self.BindArtwork(flight.BoundCatalog);
            self.BindSheets(WithFlight(flight));
        }
    }

    /// <summary>Installed sheets with the Ceres flight's sprites, caption pictures and floor composed, by the flight artwork's content identity.</summary>
    private static readonly System.Collections.Concurrent.ConcurrentDictionary<string, InstalledSpriteSheets> flightSheets = new(StringComparer.Ordinal);

    /// <summary>The installed sheets with the Ceres flight sheet, caption pictures and floor <paramref name="flight"/> composes.</summary>
    internal static InstalledSpriteSheets WithFlight(CeresFlightCharacters flight)
    {
        ArgumentNullException.ThrowIfNull(flight);
        return Identity(flight) == Identity(InstalledFixtures.IntroArtwork.FlightCharacters) ? InstalledFixtures.SpriteSheets
            : flightSheets.GetOrAdd(Identity(flight), _ =>
            {
                CeresFlightCharacters key = flight;
                var sheets = SpriteSheetDefinitions.Sheets.ToDictionary(id => id, InstalledFixtures.SpriteSheets.Get);
                sheets[SpriteSheetId.CeresFlightSprites] = CeresFlight(key);
                // The caption is painted from the flight's characters and palette, so edited flight art recomposes it.
                sheets[SpriteSheetId.CeresCinematicPictures] = LoadTiles(SpriteSheetId.CeresCinematicPictures,
                    CeresCinematicPictureSheetComposer.Compose(key, InstalledFixtures.IntroArtwork), $"flight-{RuntimeHelpers.GetHashCode(key):X8}");
                sheets[SpriteSheetId.CeresFloor] = LoadTiles(SpriteSheetId.CeresFloor,
                    FrontendFloorSheetComposer.Ceres(key, InstalledFixtures.IntroArtwork.CeresDestructionMaps,
                        InstalledFixtures.IntroArtwork.FlightPalette, InstalledFixtures.ImportMaps.RoomPaletteFx), $"flight-{RuntimeHelpers.GetHashCode(key):X8}");
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
        return ReferenceEquals(paletteFx, InstalledFixtures.ImportMaps.RoomPaletteFx)
            ? InstalledFixtures.SpriteSheets.Get(SpriteSheetId.CeresFloor)
            : LoadTiles(SpriteSheetId.CeresFloor, FrontendFloorSheetComposer.Ceres(InstalledFixtures.IntroArtwork, paletteFx),
                $"engine-{RuntimeHelpers.GetHashCode(paletteFx):X8}");
    }

    /// <summary>Loads one composed fixture tile sheet under a name of its own.</summary>
    private static SpriteSheet LoadTiles(SpriteSheetId id, ComposedSpriteSheet tiles, string fixture) =>
        SpriteSheetFileConstants.Load(tiles.Manifest,
            $"{SpriteSheetDefinitions.FileNames[id]}-fixture-{fixture}", tiles.Size, tiles.Pixels, tiles.PaletteIndices);

    /// <summary>The installed sheets with the intro sheet <paramref name="intro"/> composes.</summary>
    internal static InstalledSpriteSheets WithIntro(ImportIntroCinematicArtwork intro)
    {
        ArgumentNullException.ThrowIfNull(intro);
        return Identity(intro) == Identity(InstalledFixtures.IntroArtwork) ? InstalledFixtures.SpriteSheets
            : intros.GetOrAdd(Identity(intro), _ =>
            {
                ImportIntroCinematicArtwork key = intro;
                var sheets = SpriteSheetDefinitions.Sheets.ToDictionary(id => id, InstalledFixtures.SpriteSheets.Get);
                sheets[SpriteSheetId.IntroSprites] = Load(IntroSpriteSheetComposer.Sources(key), $"intro-fixture-{RuntimeHelpers.GetHashCode(key):X8}");
                sheets[SpriteSheetId.CeresFlightSprites] = CeresFlight(key.FlightCharacters);
                sheets[SpriteSheetId.CeresDestructionSprites] = Load(IntroSpriteSheetComposer.CeresDestructionSources(key), $"ceres-destruction-fixture-{RuntimeHelpers.GetHashCode(key):X8}");
                sheets[SpriteSheetId.ZebesRevealSprites] = Load(IntroSpriteSheetComposer.ZebesRevealSources(key), $"zebes-reveal-fixture-{RuntimeHelpers.GetHashCode(key):X8}");
                // The cinematics' background tiles are pre-coloured from the intro's characters and palettes (#1392).
                var tileSources = new ScreenTileSources(() => InstalledFixtures.ImportMaps, () => key,
                    () => InstalledFixtures.EndingObjects, () => InstalledFixtures.EndingColours,
                    () => InstalledFixtures.EndingFloors);
                sheets[SpriteSheetId.IntroNarrationPictures] = LoadTiles(SpriteSheetId.IntroNarrationPictures,
                    IntroNarrationPictureSheetComposer.Compose(key, InstalledFixtures.ImportMaps.IntroFont), $"intro-{RuntimeHelpers.GetHashCode(key):X8}");
                sheets[SpriteSheetId.IntroScenePictures] = LoadTiles(SpriteSheetId.IntroScenePictures,
                    IntroScenePictureSheetComposer.Compose(key), $"intro-{RuntimeHelpers.GetHashCode(key):X8}");
                sheets[SpriteSheetId.CeresCinematicPictures] = LoadTiles(SpriteSheetId.CeresCinematicPictures,
                    CeresCinematicPictureSheetComposer.Compose(key), $"intro-{RuntimeHelpers.GetHashCode(key):X8}");
                sheets[SpriteSheetId.CeresFloor] = LoadTiles(SpriteSheetId.CeresFloor,
                    FrontendFloorSheetComposer.Ceres(key, InstalledFixtures.ImportMaps.RoomPaletteFx), $"intro-{RuntimeHelpers.GetHashCode(key):X8}");
                return new InstalledSpriteSheets(sheets);
            });
    }

    extension(IntroCinematicState self)
    {
        /// <summary>Binds the catalog of <paramref name="intro"/> and the sheets with the intro sheet it composes, as the game rebinds both.</summary>
        internal void BindFixtureIntroArtwork(ImportIntroCinematicArtwork intro)
        {
            self.BindCharacterArtwork(intro.BoundCatalog);
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
            return new CinematicSpriteSheet(Load(DrawerSources(frames),
                $"cinematic-fixture-{frames.Family}-{RuntimeHelpers.GetHashCode(key):X8}"), frames.Family);
        });
    }

    /// <summary>
    /// A sheet of any cinematic family's <paramref name="frames"/> over the installed intro OBJ page in the opening's colours
    /// and shades: an opening family's as the intro sheet composes it, in its colouring's sprite set (#1513); a Ceres or
    /// ending family's with its palette-inheriting parts in <see cref="FixtureSet"/>, as the opening defines no colouring for
    /// it. The OAM comparisons such a sheet serves read each draw's palette row from the draw's drawn shade
    /// (<see cref="DrawnShadeRows"/>), never from the picture's inks.
    /// </summary>
    private static SpriteSheetSources DrawerSources(ICinematicSpriteFrames frames)
    {
        ImportIntroCinematicArtwork intro = InstalledFixtures.IntroArtwork;
        if (frames.Family is CinematicSpriteFamily.IntroDiscoveryActor or CinematicSpriteFamily.IntroEggEffect or CinematicSpriteFamily.IntroRinka
            or CinematicSpriteFamily.IntroScientist or CinematicSpriteFamily.IntroMotherBrain or CinematicSpriteFamily.IntroMotherBrainExplosion
            or CinematicSpriteFamily.IntroCaret or CinematicSpriteFamily.IntroNextPageArrow)
        {
            return IntroSpriteSheetComposer.Sources([frames], intro);
        }
        ScreenObjectSizes sizes = ObjectSelectionCodec.Sizes(MenuRenderDefinitions.ObjectSelection);
        RgbColor[] sprites = [.. intro.Palette.SpriteColours];
        return new SpriteSheetSources([.. IntroSpriteSheetComposer.Frames([frames], IntroObjectPage.Of(intro), sizes, (_, _) => FixtureSet)], sprites, sizes,
            PartsKeepOwnRows: true) { Shades = IntroSheetShades.Sprites(sprites) };
    }

    /// <summary>The sprite set a fixture sheet pictures a Ceres or ending family's palette-inheriting parts in: the first.</summary>
    private const int FixtureSet = 0;

    /// <summary>Composes, loads and registers one fixture sheet under a name of its own.</summary>
    internal static SpriteSheet Load(SpriteSheetSources sources, string name)
    {
        ComposedSpriteSheet composed = SpriteSheetComposer.Compose(sources, 0, 0);
        SpriteSheet sheet = SpriteSheetFileConstants.Load(composed.Manifest, name,
            composed.Size, composed.Pixels, composed.PaletteIndices);
        SheetFrameSources.Register(sheet, sources);
        return sheet;
    }
}
