using static SuperMetroid.Verification.Bosses.BossMapMarkersVerification;
using static SuperMetroid.Verification.Frontend.Pause.PausePaletteSoundVerification;
using static SuperMetroid.Verification.Frontend.Pause.PauseReserveArrowRebindVerification;
using static SuperMetroid.Verification.Frontend.Pause.PauseReserveArrowVerification;
using static SuperMetroid.Verification.Frontend.Pause.PauseReserveHudVerification;
using static SuperMetroid.Verification.Frontend.Pause.PauseReserveManualVerification;
using static SuperMetroid.Verification.Frontend.Pause.PauseReserveTanksVerification;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rendering;
using static SuperMetroid.Verification.Harness.RetailPresentationFixtureVerification;
using SuperMetroid.Core.Frontend.Pause;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Samus;
namespace SuperMetroid.Verification.Frontend.Pause;
using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.Fx;

/// <summary>Checks pause-menu render packets through map, equipment, and fade states.</summary>
internal static class PauseSnapshotTestsVerification
{
    /// <summary>Compares captured packets to direct retail-frame rendering and verifies snapshot ownership.</summary>
    [PpuLayoutBoundary("exact hardware test: checks native VRAM words")]
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
        var system = new Bank80SystemState();
        system.SetAreaMapAcquired((byte)AreaId.Maridia);
        system.MarkExploredMapTile((byte)AreaId.Maridia, new(30, 5));
        // Seed retained HUD words rather than testing only the cleared menu background.
        var gameplayVram = new SnesVram();
        ushort[] hud = new ushort[128];
        for (int i = 0; i < hud.Length; i++) hud[i] = (ushort)(0x2000 | (i % 64));
        gameplayVram.ExecuteWordTransfer(hud, GameplayVramLayout.HudTilemapWord, 1);
        var pause = CreateRetailPauseFixture(new PauseMenuEntry { Samus = samus, System = system, AreaIndex = AreaId.Maridia, RoomMapPosition = new(28, 1), GameplayLayer3 = Layer3PlaneFixtures.FromVram(gameplayVram) });
        var pages = new HashSet<PauseScreenMode>();
        var levels = new HashSet<int>();
        for (int frame = 0; frame < 128; frame++)
        {
            Rgba32[] expected = pause.Render();
            LayeredRenderSnapshot packet = pause.CaptureRenderSnapshot();
            Match(expected, SoftwareFrameSnapshotRenderer.Render(
                new RenderFrameSnapshot(new(frame + 1, 1, (ushort)frame), packet)), frame);
            Match(expected, SoftwareLayeredSnapshotRenderer.Render(packet), frame);
            Match(expected, SoftwareLayeredSnapshotRenderer.Render(packet), frame);
            LayeredRenderSnapshot repeated = pause.CaptureRenderSnapshot();
            AssertTrue(packet.Memory.SpriteParts().SequenceEqual(repeated.Memory.SpriteParts()), "repeat pause capture OAM");
            pages.Add(pause.ScreenMode);
            levels.Add(packet.Brightness);
            SnesButton input = frame == 8 ? SnesButton.R
                : frame == 80 ? SnesButton.L : 0;
            pause.Step(input, input);
            Match(expected, SoftwareLayeredSnapshotRenderer.Render(packet), frame);
        }
        AssertEqual(2, pages.Count, "snapshot covers map and equipment");
        AssertEqual(16, levels.Count, "snapshot covers every page-fade brightness");

        // A caller may reuse the command array immediately after publication.
        var captured = pause.CaptureRenderSnapshot();
        RenderLayer[] layers = captured.Layers.ToArray();
        var owned = new LayeredRenderSnapshot(captured.Memory, layers, captured.Brightness);
        layers[0] = new ObjPriorityRenderLayer(3);
        AssertEqual(captured.Layers[0], owned.Layers[0], "snapshot owns priority sequence");
        AssertThrows<ArgumentException>(() => new LayeredRenderSnapshot(captured.Memory,
            new RenderLayer[] { new ObjPriorityRenderLayer(4) }, 15), "reject invalid OBJ priority");
        Console.WriteLine("  Pause snapshots: 128 retail map/equipment/fade frames match exactly, including retained HUD and packet lifetime.");

        static void Match(Rgba32[] expected, Rgba32[] actual, int frame)
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
