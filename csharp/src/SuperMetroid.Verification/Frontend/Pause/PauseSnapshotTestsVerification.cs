using SuperMetroid.Core.Assets;
using static SuperMetroid.Verification.Bosses.BossMapMarkersVerification;
using static SuperMetroid.Verification.Frontend.Pause.PausePaletteSoundVerification;
using static SuperMetroid.Verification.Frontend.Pause.PauseReserveArrowRebindVerification;
using static SuperMetroid.Verification.Frontend.Pause.PauseReserveArrowVerification;
using static SuperMetroid.Verification.Frontend.Pause.PauseReserveHudVerification;
using static SuperMetroid.Verification.Frontend.Pause.PauseReserveManualVerification;
using static SuperMetroid.Verification.Frontend.Pause.PauseReserveTanksVerification;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rendering;
using static SuperMetroid.Verification.Harness.RetailPresentationFixtureVerification;
using SuperMetroid.Core.Frontend.Pause;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Samus;
namespace SuperMetroid.Verification.Frontend.Pause;
using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.Hud;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.TestSupport.FrameRendering;

/// <summary>Checks pause-menu render packets through map, equipment, and fade states.</summary>
internal static class PauseSnapshotTestsVerification
{
    /// <summary>Compares captured packets to direct retail-frame rendering and verifies snapshot ownership.</summary>
    internal static void VerifyPauseRenderSnapshots()
    {
        var bus = SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        var samus = new SamusState
        {
            CollectedItems = (SamusEquipmentFlags.MorphBall | SamusEquipmentFlags.Bombs),
            EquippedItems = (SamusEquipmentFlags.MorphBall | SamusEquipmentFlags.Bombs),
            MaxReserveEnergy = 100, ReserveEnergy = 47,
            Position = FixedPosition<RoomPixels>.FromWhole(new(512, 768)),
        };
        var system = new SharedGameState();
        system.SetAreaMapAcquired((byte)AreaId.Maridia);
        system.MarkExploredMapTile((byte)AreaId.Maridia, new(30, 5));
        // Retain a drawn HUD rather than testing only the cleared menu background (#1466).
        var gameplayHud = new HudState();
        gameplayHud.BindPresentation(InstalledFixtures.Maps.GameplayHud);
        gameplayHud.Initialize(HudSnapshot.CeresDebug);
        var pause = CreateRetailPauseFixture(new PauseMenuEntry { Samus = samus, System = system, AreaIndex = AreaId.Maridia, RoomMapPosition = new(28, 1), GameplayHud = gameplayHud.Shown() });
        var pages = new HashSet<PauseScreenMode>();
        var levels = new HashSet<int>();
        for (int frame = 0; frame < 128; frame++)
        {
            RgbaPixel[] expected = pause.Render();
            LayeredScene packet = pause.CaptureRenderSnapshot();
            Match(expected, DisplayRendering.Render(
                new DisplayPacket(new(frame + 1, 1, (ushort)frame), packet)), frame);
            Match(expected, DisplayRendering.Render(packet), frame);
            Match(expected, DisplayRendering.Render(packet), frame);
            LayeredScene repeated = pause.CaptureRenderSnapshot();
            AssertTrue(packet.Paint.SpriteParts().SequenceEqual(repeated.Paint.SpriteParts()), "repeat pause capture OAM");
            pages.Add(pause.ScreenMode);
            levels.Add(packet.Brightness);
            ControllerButton input = frame == 8 ? ControllerButton.R
                : frame == 80 ? ControllerButton.L : 0;
            pause.Step(input, input);
            Match(expected, DisplayRendering.Render(packet), frame);
        }
        AssertEqual(2, pages.Count, "snapshot covers map and equipment");
        AssertEqual(16, levels.Count, "snapshot covers every page-fade brightness");

        // A caller may reuse the command array immediately after publication.
        var captured = pause.CaptureRenderSnapshot();
        DisplayLayer[] layers = captured.Layers.ToArray();
        var owned = new LayeredScene(captured.Paint, layers, captured.Brightness);
        layers[0] = new SpriteDepthLayer(3);
        AssertEqual(captured.Layers[0], owned.Layers[0], "snapshot owns priority sequence");
        AssertThrows<ArgumentException>(() => new LayeredScene(captured.Paint,
            new DisplayLayer[] { new SpriteDepthLayer(4) }, 15), "reject invalid OBJ priority");
        Console.WriteLine("  Pause snapshots: 128 retail map/equipment/fade frames match exactly, including retained HUD and packet lifetime.");

        static void Match(RgbaPixel[] expected, RgbaPixel[] actual, int frame)
        {
            AssertEqual(expected.Length, actual.Length, "pause snapshot geometry");
            for (int pixel = 0; pixel < expected.Length; pixel++)
                if (expected[pixel] != actual[pixel])
                    throw new InvalidOperationException($"Pause snapshot frame {frame} pixel ({pixel % FrontendFrame.Width},{pixel / FrontendFrame.Width}): expected {expected[pixel]}, got {actual[pixel]}.");
        }
    }

    /// <summary>Runs pause rendering, marker, palette, reserve-energy, and HUD fixtures together.</summary>
    [VerificationCommand("--pause-fixtures")]
    internal static int PauseFixturesCommand()
    {
        Suite(nameof(VerifyPauseRenderSnapshots), () => VerifyPauseRenderSnapshots());
        Suite(nameof(VerifyPauseBossMarkers), () => VerifyPauseBossMarkers());
        Suite(nameof(VerifyPausePaletteSound), () => VerifyPausePaletteSound());
        Suite(nameof(VerifyPauseReserveManual), () => VerifyPauseReserveManual());
        Suite(nameof(VerifyPauseReserveArrow), () => VerifyPauseReserveArrow());
        Suite(nameof(VerifyPauseReserveArrowRebindKeepsLatch), () => VerifyPauseReserveArrowRebindKeepsLatch());
        Suite(nameof(VerifyPauseReserveTanks), () => VerifyPauseReserveTanks());
        Suite(nameof(VerifyPauseReserveHud), () => VerifyPauseReserveHud());
        return 0;
    }
}
