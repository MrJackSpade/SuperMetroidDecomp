using SuperMetroid.AssetExtraction.Frontend.CeresCinematics.Artwork;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Frontend.Ending;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Frontend.Intro;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rendering;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.Verification.Harness;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.AssetExtraction.Artwork;

namespace SuperMetroid.Verification.Frontend;

/// <summary>Checks the cinematics' published screens and the sprite entries' size pairs and raw objects (#1358).</summary>
internal static class CinematicScreenPublicationVerification
{
    /// <summary>
    /// OBSEL's size selection gives a screen's objects their sizes, and raw small objects are published exactly where they
    /// are placed: a Y below the screen stays below it for the display to clip, where OAM's eight-bit Y wrapped it above
    /// (#1587). The object index's wrap after the 128th object is not replicated (#1513), so fewer objects are stored.
    /// </summary>
    internal static void VerifyObjectSizesAndRawObjects()
    {
        ScreenObjectSizes clouds = ObjectSelectionCodec.Sizes(0xa3);
        AssertEqual(new ScreenObjectSizes(new Extent16<ScreenPixels>(32, 32), new Extent16<ScreenPixels>(64, 64)), clouds,
            "OBSEL $A3 selects 32- and 64-pixel objects");
        SpriteFrame sized = new SpriteComposition([
            new CompiledSpritePart(SpritePart.Create(Offset16<ScreenPixels>.Of(0, 0), large: true, character: 0, palette: 0, priority: 0), InheritPalette: false),
            new CompiledSpritePart(SpritePart.Create(Offset16<ScreenPixels>.Of(0, 0), large: false, character: 0, palette: 0, priority: 0), InheritPalette: false),
        ]).ToFrame("sized", 0, clouds);
        AssertEqual(clouds.Large, sized.Parts[0].Size, "a large part is composed at the selection's large size");
        AssertEqual(clouds.Small, sized.Parts[1].Size, "a small part is composed at the selection's small size");

        // The ending's stars are written straight to OAM. Each is published where it is placed, those below the screen
        // included, for the display to clip (#1587); the stars on the screen stand for the records their stores write.
        SpriteSheetFrame star = InstalledFixtures.SpriteSheets.Get(SpriteSheetId.EndingSuitedRewardSprites)
            .Frame(EndingShootingStarFrames.SheetFrameName(EndingShootingStarDefinitions.FullStarFrame));
        var raw = ScreenSpritesFixtures.Blank();
        var oam = new OamBuffer();
        raw.BeginFrame();
        oam.BeginFrame();
        var positions = new List<Position16<SpritePixels>>();
        for (int index = 0; index < 40; index++)
        {
            var position = new Position16<SpritePixels>((ushort)(index * 3 % 512), (ushort)(index * 7 % 256));
            positions.Add(position);
            raw.AddImage(star, position, PictureShading.Standard);
            oam.AddRawSmallSprite(position, SnesObjAttributeWord.From(SuperMetroid.AssetExtraction.Frontend.Ending.EndingShootingStarParts.Appearance(EndingShootingStarDefinitions.FullStarFrame)));
        }
        raw.FinishFrame();
        oam.FinalizeFrame();
        AssertEqual(oam.LastFinalizedSpriteCount, raw.Draws.Length, "each raw small object is one entry, as each direct OAM store is");
        for (int index = 0; index < raw.Draws.Length; index++)
        {
            AssertEqual(new Position16<ScreenPixels>(positions[index].X, positions[index].Y), raw.Draws[index].Origin,
                $"raw object {index} is published where it is placed");
            if (positions[index].Y < FrontendFrame.Height)
                AssertEqual(oam.GetEntry(index), raw.GetEntry(index), $"raw object {index} on the screen stands for the record its store writes");
        }
        Console.WriteLine("  Screen sprites: OBSEL size pairs match, and raw objects are published where they are placed and match OAM on the screen.");
    }

    /// <summary>
    /// The Ceres flight's rear view shows the rear map slice, and each cinematic's published state survives a debugger
    /// round trip: once its artwork is rebound it shows the same image, palette and sprite entries.
    /// </summary>
    internal static void VerifyCinematicRoundTrips()
    {
        var flight = new IntroCeresFlightState(AudioQueues.Standalone(), InstalledFixtures.IntroArtwork.FlightCharacters.BoundCatalog, sheets: CinematicSheetFixtures.WithFlight(InstalledFixtures.IntroArtwork.FlightCharacters));
        for (int frame = 0; frame < 5000 && flight.Phase != IntroCeresFlightPhase.FlyingTowardCeres; frame++)
            flight.StepFrame();
        AssertEqual(IntroCeresFlightPhase.FlyingTowardCeres, flight.Phase, "the flight reaches its rear view");
        FramePaint rear = flight.CaptureRenderSnapshot().Paint;
        byte[] rearVram = flight.ShownVram(InstalledFixtures.IntroArtwork.FlightCharacters.BoundCatalog);
        ReadOnlySpan<byte> maps = InstalledFixtures.IntroArtwork.FlightCharacters.FloorMaps.Span;
        for (int cell = 0; cell < CeresFlightArtworkFileFormat.MapCellsPerView; cell++)
            AssertEqual(maps[CeresFlightArtworkFileFormat.MapCellsPerView + cell], rearVram[cell * 2], $"rear view shows rear map cell {cell}");
        var restoredFlight = RoundTrip(flight);
        restoredFlight.BindFixtureFlightArtwork(InstalledFixtures.IntroArtwork.FlightCharacters);
        AssertSameFrame(rearVram, rear, restoredFlight.ShownVram(InstalledFixtures.IntroArtwork.FlightCharacters.BoundCatalog), restoredFlight.CaptureRenderSnapshot().Paint, "Ceres flight");

        var destruction = InstalledFixtures.CreateDestruction();
        for (int frame = 0; frame < 600; frame++)
            destruction.Step();
        FramePaint shown = destruction.CaptureRenderSnapshot().Paint;
        byte[] shownVram = destruction.ShownVram();
        var restoredDestruction = RoundTrip(destruction);
        restoredDestruction.BindFixtureDestructionArtwork(InstalledFixtures.IntroArtwork);
        AssertSameFrame(shownVram, shown, restoredDestruction.ShownVram(), restoredDestruction.CaptureRenderSnapshot().Paint, "Ceres destruction");

        IntroCinematicState intro = InstalledFixtures.CreateIntro();
        intro.SetupFirstIllustratedPage();
        (intro.CurrentObjects ?? throw new InvalidOperationException("The first illustrated page creates the cinematic objects.")).Step(0);
        byte[] image = intro.ShownVram();
        var restoredIntro = RoundTrip(intro);
        restoredIntro.BindFixtureIntroArtwork(InstalledFixtures.IntroArtwork);
        AssertTrue(image.AsSpan().SequenceEqual(restoredIntro.ShownVram()), "a restored intro shows the same characters, blanked glyphs and tilemaps");
        // The intro publishes its faded backdrop and its pictures' shading, not a palette (#1512).
        LayeredScene introFrame = intro.CaptureTranslatedRenderSnapshot(), restoredFrame = restoredIntro.CaptureTranslatedRenderSnapshot();
        AssertEqual(introFrame.Paint.Backdrop, restoredFrame.Paint.Backdrop, "a restored intro keeps its backdrop");
        AssertTrue(PlacedSpritePart.Of(introFrame.SpriteDraws).SequenceEqual(PlacedSpritePart.Of(restoredFrame.SpriteDraws)),
            "a restored intro places the same sprite entries");
        Console.WriteLine("  Cinematic screens: the rear view shows its map, and the flight, destruction and intro survive a debugger round trip.");
    }

    /// <summary>
    /// A copy of <paramref name="state"/> through the debugger state serializer, captured and restored bound to the
    /// installed content as a host's store binds it (#1532): what the state reaches of that content is named by path, as
    /// a debugger state names it, rather than copied wholesale only to be rebound.
    /// </summary>
    private static T RoundTrip<T>(T state) where T : class => GraphDigestVerification.RoundTrip(state);

    /// <summary>Asserts that two captured frames show the same image, backdrop and sprite parts, the images their scenes composed.</summary>
    private static void AssertSameFrame(byte[] expectedVram, FramePaint expected, byte[] actualVram, FramePaint actual, string scene)
    {
        AssertTrue(expectedVram.SequenceEqual(actualVram), $"a restored {scene} shows the same image");
        AssertEqual(expected.Backdrop, actual.Backdrop, $"a restored {scene} shows the same backdrop");
        AssertTrue(PlacedSpritePart.Of(expected.SpriteDraws).SequenceEqual(PlacedSpritePart.Of(actual.SpriteDraws)),
            $"a restored {scene} places the same sprite entries");
    }

    /// <summary>Runs the cinematic screen publication checks.</summary>
    [VerificationCommand("--cinematic-screens")]
    internal static int CinematicScreensCommand()
    {
        Suite(nameof(VerifyObjectSizesAndRawObjects), () => VerifyObjectSizesAndRawObjects());
        Suite(nameof(VerifyCinematicRoundTrips), () => VerifyCinematicRoundTrips());
        return 0;
    }
}
