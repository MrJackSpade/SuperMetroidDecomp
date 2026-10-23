using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Frontend.Ending;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Frontend.CeresCinematics;
using SuperMetroid.Core.Frontend.Intro;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Diagnostics.SaveStates;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.Verification.Harness;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.AssetExtraction.Artwork;

namespace SuperMetroid.Verification.Frontend;

/// <summary>Checks the cinematics' published screens and the sprite entries' size pairs and raw objects (#1358).</summary>
internal static class CinematicScreenPublicationVerification
{
    /// <summary>
    /// OBSEL's size selection gives a screen's objects their sizes, and raw small objects wrap after the 128th exactly
    /// as direct OAM stores do.
    /// </summary>
    internal static void VerifyObjectSizesAndRawObjects()
    {
        ScreenObjectSizes clouds = ObjectSelectionCodec.Sizes(0xa3);
        AssertEqual(new ScreenObjectSizes(new Extent16<ScreenPixels>(32, 32), new Extent16<ScreenPixels>(64, 64)), clouds,
            "OBSEL $A3 selects 32- and 64-pixel objects");
        SpriteFrame sized = new SpriteComposition([
            new CompiledSpritePart(SnesSpritemapXWord.Create(0, true), SnesSpritemapYByte.Create(0), SnesObjAttributeWord.Create(0, 0, 0), InheritPalette: false),
            new CompiledSpritePart(SnesSpritemapXWord.Create(0, false), SnesSpritemapYByte.Create(0), SnesObjAttributeWord.Create(0, 0, 0), InheritPalette: false),
        ]).ToFrame("sized", 0, clouds);
        AssertEqual(clouds.Large, sized.Parts[0].Size, "a large part is composed at the selection's large size");
        AssertEqual(clouds.Small, sized.Parts[1].Size, "a small part is composed at the selection's small size");

        // The ending's stars are written straight to OAM, some below the screen so that they wrap above it.
        SpriteSheetFrame star = InstalledFixtures.SpriteSheets.Get(SpriteSheetId.EndingShootingSprites)
            .Frame(EndingShootingStarDefinitions.SheetFrameName(EndingShootingStarDefinitions.FullStarFrame));
        var raw = ScreenSpritesFixtures.Blank();
        var oam = new OamBuffer();
        raw.BeginFrame();
        oam.BeginFrame();
        for (int index = 0; index < ScreenSprites.Capacity + 2; index++)
        {
            var position = new Position16<OamPixels>((ushort)(index * 3 % 512), (ushort)(index * 7 % 256));
            raw.AddObjectImage(star, position);
            oam.AddRawSmallSprite(position, EndingShootingStarDefinitions.Attribute(EndingShootingStarDefinitions.FullStarFrame));
        }
        raw.FinishFrame();
        oam.FinalizeFrame();
        AssertEqual(oam.LastFinalizedSpriteCount, raw.Draws.Length, "raw small objects wrap after the 128th like direct OAM stores");
        for (int index = 0; index < raw.Draws.Length; index++)
            AssertEqual(oam.GetEntry(index), raw.GetEntry(index), $"wrapped raw object {index} stands for the record its store writes");
        Console.WriteLine("  Screen sprites: OBSEL size pairs and wrapping raw objects match OAM.");
    }

    /// <summary>
    /// The Ceres flight's rear view shows the rear map slice, and each cinematic's published state survives a debugger
    /// round trip: once its artwork is rebound it shows the same image, palette and sprite entries.
    /// </summary>
    internal static void VerifyCinematicRoundTrips()
    {
        var flight = new IntroCeresFlightState(new CartridgeAudioState(), InstalledFixtures.IntroArtwork.CeresFlight, sheets: CinematicSheetFixtures.WithFlight(InstalledFixtures.IntroArtwork.CeresFlight));
        for (int frame = 0; frame < 5000 && flight.Phase != IntroCeresFlightPhase.FlyingTowardCeres; frame++)
            flight.StepFrame();
        AssertEqual(IntroCeresFlightPhase.FlyingTowardCeres, flight.Phase, "the flight reaches its rear view");
        FramePaint rear = flight.CaptureRenderSnapshot().Paint;
        byte[] rearVram = flight.ShownVram();
        ReadOnlySpan<byte> maps = InstalledFixtures.IntroArtwork.CeresFlight.FloorMaps.Span;
        for (int cell = 0; cell < CeresFlightRomData.Transfers.FloorMapSliceByteCount; cell++)
            AssertEqual(maps[CeresFlightRomData.Transfers.FloorMapSliceByteCount + cell], rearVram[cell * 2], $"rear view shows rear map cell {cell}");
        var restoredFlight = RoundTrip(flight);
        restoredFlight.BindFixtureFlightArtwork(InstalledFixtures.IntroArtwork.CeresFlight);
        AssertSameFrame(rearVram, rear, restoredFlight.ShownVram(), restoredFlight.CaptureRenderSnapshot().Paint, "Ceres flight");

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
        restoredIntro.BindIntroFont(InstalledFixtures.Maps.IntroFont);
        restoredIntro.BindFixtureIntroArtwork(InstalledFixtures.IntroArtwork);
        AssertTrue(image.AsSpan().SequenceEqual(restoredIntro.ShownVram()), "a restored intro shows the same characters, blanked glyphs and tilemaps");
        AssertTrue(intro.palette.Colors.SequenceEqual(restoredIntro.palette.Colors), "a restored intro keeps its palette");
        Console.WriteLine("  Cinematic screens: the rear view shows its map, and the flight, destruction and intro survive a debugger round trip.");
    }

    /// <summary>A copy of <paramref name="state"/> through the debugger state serializer.</summary>
    private static T RoundTrip<T>(T state) where T : class
    {
        using var stream = new MemoryStream();
        DebuggerObjectGraphSerializer.Serialize(stream, state);
        stream.Position = 0;
        return DebuggerObjectGraphSerializer.Deserialize<T>(stream);
    }

    /// <summary>Asserts that two captured frames show the same image, palette and sprite parts, the images their scenes composed.</summary>
    private static void AssertSameFrame(byte[] expectedVram, FramePaint expected, byte[] actualVram, FramePaint actual, string scene)
    {
        AssertTrue(expectedVram.SequenceEqual(actualVram), $"a restored {scene} shows the same image");
        AssertTrue(expected.Colors.SequenceEqual(actual.Colors), $"a restored {scene} shows the same palette");
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
