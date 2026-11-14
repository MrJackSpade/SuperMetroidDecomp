using static SuperMetroid.Verification.Bosses.BossMapMarkersVerification;
using static SuperMetroid.Verification.Frontend.Pause.PausePaletteSoundVerification;
using static SuperMetroid.Verification.Frontend.Pause.PauseReserveArrowRebindVerification;
using static SuperMetroid.Verification.Frontend.Pause.PauseReserveArrowVerification;
using static SuperMetroid.Verification.Frontend.Pause.PauseReserveHudVerification;
using static SuperMetroid.Verification.Frontend.Pause.PauseReserveManualVerification;
using static SuperMetroid.Verification.Frontend.Pause.PauseReserveTanksVerification;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
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
        var samus = new SamusState(InstalledFixtures.FramesBeyondAnimation, new SuperMetroid.Core.Compatibility.UnauthoredBoostCadence(), SuperMetroid.Core.Samus.SamusCompatibility.ForNewSamus(), new SuperMetroid.Core.Samus.Projectiles.BeamChargeState())
        {
            CollectedItems = (SamusEquipmentFlags.MorphBall | SamusEquipmentFlags.Bombs),
            EquippedItems = (SamusEquipmentFlags.MorphBall | SamusEquipmentFlags.Bombs),
            MaxReserveEnergy = 100, ReserveEnergy = 47,
            Position = FixedPosition<RoomPixels>.FromWhole(new(512, 768)),
        };
        var system = new SharedGameState(new SharedRandom());
        system.SetAreaMapAcquired(AreaId.Maridia);
        system.MarkExploredMapTile(AreaId.Maridia, new(30, 5));
        // Retain a drawn HUD rather than testing only the cleared menu background (#1466).
        var gameplayHud = new HudState();
        gameplayHud.BindPresentation(InstalledFixtures.Maps.GameplayHud);
        gameplayHud.Initialize(HudSnapshot.CeresDebug);
        var pause = CreateRetailPauseFixture(new PauseMenuEntry { Samus = samus, System = system, Area = AreaId.Maridia, RoomMapPosition = new(28, 1), GameplayHud = gameplayHud.Shown() });
        var pages = new HashSet<PauseScreenMode>();
        var levels = new HashSet<int>();
        for (int frame = 0; frame < 128; frame++)
        {
            // Each comparison draws only packets whose descriptions differ (#1532): the direct capture, the published
            // packet under its frame number, and the packet again once the menu has stepped past it.
            LayeredScene expected = pause.CaptureRenderSnapshot();
            LayeredScene packet = pause.CaptureRenderSnapshot();
            AssertTrue(ScenePixels.Same(new DisplayPacket(0, expected), new DisplayPacket((ushort)frame, packet)),
                $"Pause snapshot frame {frame} shows the direct capture's pixels");
            LayeredScene repeated = pause.CaptureRenderSnapshot();
            AssertTrue(packet.SpriteParts().SequenceEqual(repeated.SpriteParts()), "repeat pause capture OAM");
            pages.Add(pause.ScreenMode);
            levels.Add(packet.Brightness);
            ControllerButton input = frame == 8 ? ControllerButton.R
                : frame == 80 ? ControllerButton.L : 0;
            pause.Step(input, input);
            AssertTrue(ScenePixels.Same(expected, packet), $"Pause snapshot frame {frame} keeps its pixels after the menu steps");
        }
        AssertEqual(2, pages.Count, "snapshot covers map and equipment");
        AssertEqual(16, levels.Count, "snapshot covers every page-fade brightness");

        // A caller may reuse the command array immediately after publication.
        var captured = pause.CaptureRenderSnapshot();
        DisplayLayer[] layers = captured.Layers.ToArray();
        var owned = new LayeredScene(captured.Paint, layers, captured.Brightness);
        layers[0] = new SpritesLayer([]);
        AssertEqual(captured.Layers[0], owned.Layers[0], "snapshot owns its layer order");
        Console.WriteLine("  Pause snapshots: 128 retail map/equipment/fade frames match exactly, including retained HUD and packet lifetime.");
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
        Suite(nameof(VerifyPauseReserveArrowRebindKeepsPicture), () => VerifyPauseReserveArrowRebindKeepsPicture());
        Suite(nameof(VerifyPauseReserveTanks), () => VerifyPauseReserveTanks());
        Suite(nameof(VerifyPauseReserveHud), () => VerifyPauseReserveHud());
        return 0;
    }
}
