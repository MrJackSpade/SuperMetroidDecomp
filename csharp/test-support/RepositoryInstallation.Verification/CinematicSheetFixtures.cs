using SuperMetroid.AssetExtraction.Frontend;
using System.Runtime.CompilerServices;
using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.AssetExtraction.Frontend.Intro;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Frontend.CeresCinematics;
using SuperMetroid.Core.Frontend.CeresCinematics.Artwork;
using SuperMetroid.Core.Frontend.Intro;
using SuperMetroid.Core.Frontend.Intro.Artwork;

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

    /// <summary>Installed sheets with the Ceres flight's sprites and caption tiles composed, by flight artwork.</summary>
    private static readonly ConditionalWeakTable<CeresFlightArtwork, InstalledSpriteSheets> flightSheets = new();

    /// <summary>The installed sheets with the Ceres flight sheet and caption tiles <paramref name="flight"/> composes.</summary>
    internal static InstalledSpriteSheets WithFlight(CeresFlightArtwork flight)
    {
        ArgumentNullException.ThrowIfNull(flight);
        return ReferenceEquals(flight, InstalledFixtures.IntroArtwork.CeresFlight) ? InstalledFixtures.SpriteSheets
            : flightSheets.GetValue(flight, key =>
            {
                var sheets = SpriteSheetDefinitions.Sheets.ToDictionary(id => id, InstalledFixtures.SpriteSheets.Get);
                sheets[SpriteSheetId.CeresFlightSprites] = CeresFlight(key);
                sheets[SpriteSheetId.CeresFlightObjectTiles] = LoadTiles(SpriteSheetId.CeresFlightObjectTiles, ScreenTileSheetComposer.ComposeFlightObjects(key),
                    $"flight-{RuntimeHelpers.GetHashCode(key):X8}");
                sheets[SpriteSheetId.CeresFlightFloorTiles] = LoadTiles(SpriteSheetId.CeresFlightFloorTiles, ScreenTileSheetComposer.ComposeFlightMode7(key),
                    $"flight-{RuntimeHelpers.GetHashCode(key):X8}");
                return new InstalledSpriteSheets(sheets);
            });
    }

    /// <summary>Loads one composed fixture tile sheet under a name of its own.</summary>
    private static SpriteSheet LoadTiles(SpriteSheetId id, ComposedSpriteSheet tiles, string fixture) =>
        SpriteSheet.Load(new MemoryStream(SpriteSheet.Write(tiles.Manifest), writable: false),
            $"{SpriteSheetDefinitions.FileNames[id]}-fixture-{fixture}", tiles.Size, tiles.Pixels, tiles.PaletteIndices, tiles.Priorities, tiles.PartRows);

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
                sheets[SpriteSheetId.CeresDestructionSprites] = Load(IntroSpriteSheetComposer.CeresDestructionSources(key,
                    [CeresDestructionUpload.CeresScene]), $"ceres-destruction-fixture-{RuntimeHelpers.GetHashCode(key):X8}");
                sheets[SpriteSheetId.ZebesRevealSprites] = Load(IntroSpriteSheetComposer.CeresDestructionSources(key,
                    [CeresDestructionUpload.CeresScene, CeresDestructionUpload.ZebesReveal]), $"zebes-reveal-fixture-{RuntimeHelpers.GetHashCode(key):X8}");
                // The cinematics' background tiles are pre-coloured from the intro's characters and palettes (#1392).
                var tileSources = new ScreenTileSources(() => InstalledFixtures.Maps, () => key,
                    RepositoryInstallation.Installation.LoadEndingObjectArt, RepositoryInstallation.Installation.LoadEndingPalettes,
                    RepositoryInstallation.Installation.LoadEndingMode7Art);
                foreach (SpriteSheetId id in new[] { SpriteSheetId.IntroSceneTiles, SpriteSheetId.IntroFontTiles, SpriteSheetId.CeresFlightObjectTiles,
                             SpriteSheetId.CeresIntroObjectTiles, SpriteSheetId.ZebesRevealTiles })
                    sheets[id] = LoadTiles(id, ScreenTileSheetComposer.Compose(id, tileSources), $"intro-{RuntimeHelpers.GetHashCode(key):X8}");
                sheets[SpriteSheetId.CeresFlightFloorTiles] = LoadTiles(SpriteSheetId.CeresFlightFloorTiles,
                    ScreenTileSheetComposer.ComposeFlightMode7(key.CeresFlight), $"intro-{RuntimeHelpers.GetHashCode(key):X8}");
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
        SpriteSheet sheet = SpriteSheet.Load(new MemoryStream(SpriteSheet.Write(composed.Manifest), writable: false), name,
            composed.Size, composed.Pixels, composed.PaletteIndices, composed.Priorities, composed.PartRows);
        SheetFrameSources.Register(sheet, sources);
        return sheet;
    }
}
