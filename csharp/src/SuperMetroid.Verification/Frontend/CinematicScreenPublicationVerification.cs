using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Frontend.CeresCinematics;
using SuperMetroid.Core.Frontend.Intro;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Diagnostics.SaveStates;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.Verification.Harness;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;

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
        var sprites = new ScreenSprites(new SpriteArtwork(new SpriteArtworkId(SpriteArtworkKind.FrontendObjects),
            new byte[2 * 256 * SpriteArtwork.CharacterByteCount]), clouds);
        sprites.BeginFrame();
        sprites.AddPart(SnesSpritemapXWord.Create(0, true), SnesSpritemapYByte.Create(0), SnesObjAttributeWord.Create(0, 0, 0), new(40, 40));
        sprites.AddPart(SnesSpritemapXWord.Create(0, false), SnesSpritemapYByte.Create(0), SnesObjAttributeWord.Create(0, 0, 0), new(40, 40));
        sprites.FinishFrame();
        AssertEqual(clouds.Large, sprites.Draws[0].Frame.Parts[0].Size, "a large part draws at the selection's large size");
        AssertEqual(clouds.Small, sprites.Draws[1].Frame.Parts[0].Size, "a small part draws at the selection's small size");

        var raw = ScreenSpritesFixtures.Blank();
        var oam = new OamBuffer();
        raw.BeginFrame();
        oam.BeginFrame();
        for (int index = 0; index < ScreenSprites.Capacity + 2; index++)
        {
            var position = new Position16<OamPixels>((ushort)(index % 200), (ushort)(index % 100));
            var attributes = SnesObjAttributeWord.Create(index, index % 8, index % 4);
            raw.AddSmallObject(position, attributes);
            oam.AddRawSmallSprite(position, attributes);
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
        var flight = new IntroCeresFlightState(new CartridgeAudioState(), InstalledFixtures.IntroArtwork.CeresFlight, spriteSheet: CinematicSheetFixtures.CeresFlight(InstalledFixtures.IntroArtwork.CeresFlight));
        for (int frame = 0; frame < 5000 && flight.Phase != IntroCeresFlightPhase.FlyingTowardCeres; frame++)
            flight.StepFrame();
        AssertEqual(IntroCeresFlightPhase.FlyingTowardCeres, flight.Phase, "the flight reaches its rear view");
        PpuMemorySnapshot rear = flight.CaptureRenderSnapshot().Memory;
        ReadOnlySpan<byte> maps = InstalledFixtures.IntroArtwork.CeresFlight.Mode7Maps.Span;
        for (int cell = 0; cell < CeresFlightRomData.Vram.Mode7MapSliceByteCount; cell++)
            AssertEqual(maps[CeresFlightRomData.Vram.Mode7MapSliceByteCount + cell], rear.Vram[cell * 2], $"rear view shows rear map cell {cell}");
        var restoredFlight = RoundTrip(flight);
        restoredFlight.BindFixtureFlightArtwork(InstalledFixtures.IntroArtwork.CeresFlight);
        AssertSameFrame(rear, restoredFlight.CaptureRenderSnapshot().Memory, "Ceres flight");

        var destruction = InstalledFixtures.CreateDestruction();
        for (int frame = 0; frame < 600; frame++)
            destruction.Step();
        PpuMemorySnapshot shown = destruction.CaptureRenderSnapshot().Memory;
        var restoredDestruction = RoundTrip(destruction);
        restoredDestruction.BindFixtureDestructionArtwork(InstalledFixtures.IntroArtwork);
        AssertSameFrame(shown, restoredDestruction.CaptureRenderSnapshot().Memory, "Ceres destruction");

        IntroCinematicState intro = InstalledFixtures.CreateIntro();
        intro.SetupFirstIllustratedPage();
        (intro.CurrentObjects ?? throw new InvalidOperationException("The first illustrated page creates the cinematic objects.")).Step(0);
        byte[] image = intro.Image();
        var restoredIntro = RoundTrip(intro);
        restoredIntro.BindIntroFont(InstalledFixtures.Maps.IntroFont);
        restoredIntro.BindFixtureIntroArtwork(InstalledFixtures.IntroArtwork);
        AssertTrue(image.AsSpan().SequenceEqual(restoredIntro.Image()), "a restored intro shows the same characters, blanked glyphs and tilemaps");
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

    /// <summary>Asserts that two captured frames show the same image, palette and sprite parts.</summary>
    private static void AssertSameFrame(PpuMemorySnapshot expected, PpuMemorySnapshot actual, string scene)
    {
        AssertTrue(expected.Vram.SequenceEqual(actual.Vram), $"a restored {scene} shows the same image");
        AssertTrue(expected.Cgram.SequenceEqual(actual.Cgram), $"a restored {scene} shows the same palette");
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
