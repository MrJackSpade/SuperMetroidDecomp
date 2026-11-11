using SuperMetroid.Core.Assets;
using SuperMetroid.Core;
using static SuperMetroid.Verification.Frontend.Ending.EndingSnapshotTestsVerification;
using static SuperMetroid.Verification.Frontend.FrontendCompletionVerification;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rendering;
using static SuperMetroid.Verification.Frontend.CeresCinematics.CeresExplosionVerification;
using static SuperMetroid.Verification.Harness.GraphDigestVerification;
using static SuperMetroid.Verification.Harness.RetailGameFixtureVerification;
using static SuperMetroid.Verification.Harness.RetailPresentationFixtureVerification;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using static SuperMetroid.Verification.Areas.Escape.ZebesDescentVerification;
using SuperMetroid.Core.Frontend.GameOver;
using SuperMetroid.Core.Frontend.Intro;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.Core.Frontend.CeresCinematics;
namespace SuperMetroid.Verification.Frontend.Intro;
using SuperMetroid.Verification.Harness;
using SuperMetroid.TestSupport.FrameRendering;

/// <summary>Checks rendered cinematic snapshots against live scene rendering.</summary>
internal static class CinematicSnapshotTestsVerification
{
    /// <summary>Samples Ceres flight, destruction, and game-over phases before and after simulation advances.</summary>
    internal static void VerifyCinematicRenderSnapshots()
    {
        Suite(nameof(VerifyCeresExplosionTimeline), () => VerifyCeresExplosionTimeline());
        Suite(nameof(VerifyZebesDoesNotWrapDuringDescent), () => VerifyZebesDoesNotWrapDuringDescent());
        Suite(nameof(VerifyIntroDisplayCapture), () => VerifyIntroDisplayCapture());
        var bus = SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        var flight = new IntroCeresFlightState(AudioQueues.Standalone(), InstalledFixtures.IntroArtwork.CeresFlight.BoundCatalog, sheets: CinematicSheetFixtures.WithFlight(InstalledFixtures.IntroArtwork.CeresFlight));
        var flightPhases = new HashSet<IntroCeresFlightPhase>();
        int flightSamples = 0;
        bool coloredRearView = false;
        for (int tick = 0; tick < 4000 && !flight.Finished; tick++)
        {
            // Each phase's first update and every fourteenth are rendered and digested (#1532).
            bool changed = flightPhases.Add(flight.Phase);
            if (changed || tick % 14 == 0)
            {
                LayeredScene snapshot = flight.CaptureRenderSnapshot();
                byte[] held = GraphDigest(snapshot);
                coloredRearView |= snapshot.Layers.Any(layer => layer is ScreenTintLayer c
                    && c.Colour != RgbColor.Black);
                AssertTrue(ScenePixels.Same(flight.CaptureRenderSnapshot(), snapshot), $"Ceres flight snapshot tick {tick} shows the live scene's pixels");
                flight.StepFrame();
                Survives(held, snapshot, tick);
                flightSamples++;
            }
            else flight.StepFrame();
        }
        AssertTrue(flight.Finished && coloredRearView, "flight fixture completes and exercises nonzero fixed color");
        AssertTrue(flightPhases.Contains(IntroCeresFlightPhase.SpaceColonyTitle), "flight caption captured");
        Console.WriteLine($"  Ceres flight snapshots: {flightSamples} samples across {flightPhases.Count} phases match, including fixed-color rear view.");
        var destruction = CreateRetailDestructionFixture();
        var phases = new HashSet<CeresDestructionPhase>();
        int samples = 0;
        bool mode1 = false, mode7 = false, explosionWindow = false, explosionCoversTop = false;
        for (int tick = 0; tick < 2000 && !destruction.Finished; tick++)
        {
            // Each phase's first update and every twenty-sixth are rendered and digested (#1532).
            bool changed = phases.Add(destruction.Phase);
            if (changed || tick % 26 == 0)
            {
                LayeredScene snapshot = destruction.CaptureRenderSnapshot();
                byte[] held = GraphDigest(snapshot);
                mode7 |= snapshot.Layers.Any(layer => layer is TurningFloorLayer);
                mode1 |= snapshot.Layers.Any(layer => layer is PicturedPlaneLayer);
                foreach (var window in snapshot.LayersOf<ColourAddWindowLayer>())
                {
                    bool coloured = window.Colour != RgbColor.Black;
                    explosionWindow |= !window.Window.IsClosed && coloured;
                    // The blast's oval reaches the top line and its window starts there, with no HUD band cut out (#1468).
                    explosionCoversTop |= coloured && window.Window.FirstLine == 0 && window.Window.Shapes.Length != 0 &&
                        window.Window.Shapes[0] is WindowOval oval && oval.CoreStart.Y - oval.Radii.Y <= 0;
                }
                AssertTrue(ScenePixels.Same(destruction.CaptureRenderSnapshot(), snapshot), $"Ceres destruction snapshot tick {tick} shows the live scene's pixels");
                destruction.Step();
                Survives(held, snapshot, tick);
                samples++;
            }
            else destruction.Step();
        }
        AssertTrue(destruction.Finished, "destruction snapshot fixture completes");
        AssertTrue(mode1 && mode7, "destruction captures both PPU modes");
        AssertTrue(explosionWindow, "Ceres destruction publishes the cartridge power-bomb color window");
        AssertTrue(explosionCoversTop, "Ceres explosion covers the top of the cinematic, without a gameplay HUD exclusion");
        int menuSamples = 0;
        foreach (bool chooseNo in new[] { false, true })
        {
            var audio = AudioQueues.Standalone();
            var menu = new GameOverMenuState(audio, RetailPresentationFixture(), MenuSheetFixtures.For(RetailPresentationFixture()));
            bool toggled = false, released = false;
            GameOverMenuPhase? sampledPhase = null;
            for (int tick = 0; tick < 300 && !menu.ContinueRequested && !menu.TitleRequested; tick++)
            {
                // The menu is captured every update, as the game draws it; each phase's first update and every fourth are
                // rendered and digested (#1532).
                LayeredScene snapshot = menu.CaptureRenderSnapshot();
                bool sample = menu.Phase != sampledPhase || tick % 4 == 0;
                sampledPhase = menu.Phase;
                byte[] held = sample ? GraphDigest(snapshot) : [];
                if (sample)
                    AssertTrue(ScenePixels.Same(menu.CaptureRenderSnapshot(), snapshot), $"game-over snapshot tick {tick} shows the live menu's pixels");
                ControllerButton input = 0;
                if (menu.Phase == GameOverMenuPhase.Main)
                {
                    if (chooseNo && !toggled) { input = ControllerButton.Down; toggled = true; }
                    else if (!released) released = true;
                    else input = ControllerButton.A;
                }
                menu.Step(input);
                audio.AdvanceFrame(default);
                if (!sample)
                    continue;
                Survives(held, snapshot, tick);
                menuSamples++;
            }
            AssertEqual(chooseNo, menu.TitleRequested, "game-over no outcome");
            AssertEqual(!chooseNo, menu.ContinueRequested, "game-over yes outcome");
        }
        Console.WriteLine($"  Cinematic snapshots: {samples} Ceres/Zebes frames ({phases.Count} phases) and {menuSamples} game-over frames match.");

        // A held packet shows what it showed when captured as long as its whole description, every value it reaches
        // through shared or owned state, is unchanged (#1532): the digest is taken when captured and again after the update.
        static void Survives(byte[] held, LayeredScene snapshot, int tick) =>
            AssertTrue(held.AsSpan().SequenceEqual(GraphDigest(snapshot)), $"Cinematic snapshot tick {tick} survives the simulation update unchanged");
    }

    /// <summary>Compares full intro snapshots with an independent legacy display owner while both scenes advance.</summary>
    private static void VerifyIntroDisplayCapture()
    {
        // Each owner keeps its own music queue, which the Ceres flight waits on; the game runs
        // the queue handler after every dispatch.
        var legacyAudio = AudioQueues.Standalone();
        var capturedAudio = AudioQueues.Standalone();
        var legacy = CreateRetailIntroFixture(legacyAudio);
        var captured = CreateRetailIntroFixture(capturedAudio);
        var phases = new HashSet<IntroCinematicPhase>();
        int samples = 0;
        for (int tick = 0; tick < 20000 && !legacy.CeresFlightFinished; tick++)
        {
            // Independent owners are essential: legacy draw advances projectile trails.
            // Calling both producers on one scene would run those side effects twice.
            // Each owner draws every update for those side effects; the pixels are compared on sampled updates only (#1532).
            LayeredScene expected = legacy.CaptureTranslatedRenderSnapshot();
            LayeredScene snapshot = captured.CaptureTranslatedRenderSnapshot();
            bool newPhase = phases.Add(legacy.Phase);
            bool sample = newPhase || tick % 62 == 0;
            byte[] held = sample ? GraphDigest(snapshot) : [];
            if (sample)
            {
                AssertTrue(ScenePixels.Same(new DisplayPacket(tick, expected), new DisplayPacket(tick, snapshot)),
                    $"intro captured pixels: {legacy.Phase}, tick {tick}");
                samples++;
            }
            ControllerButton input = tick % 47 == 0 ? ControllerButton.A : 0;
            captured.Step(input);
            capturedAudio.AdvanceFrame(default);
            legacy.Step(input);
            legacyAudio.AdvanceFrame(default);
            // The packet's whole description, every value it reaches through owned or shared state, is digested when
            // captured and again after both owners advance.
            if (sample)
                AssertTrue(held.AsSpan().SequenceEqual(GraphDigest(snapshot)),
                    "intro packet survives subsequent simulation mutations");
            AssertEqual(legacy.Phase, captured.Phase, "intro phase after draw");
            AssertEqual(legacy.MotherBrainHitCount, captured.MotherBrainHitCount, "intro hits after draw");
            AssertEqual(legacy.FlashbackSamusX, captured.FlashbackSamusX, "intro Samus X after draw");
            AssertEqual(legacy.FlashbackSamusY, captured.FlashbackSamusY, "intro Samus Y after draw");
            AssertEqual(legacy.ActiveFlashbackProjectileCount, captured.ActiveFlashbackProjectileCount, "intro projectiles after draw");
        }
        AssertTrue(legacy.CeresFlightFinished && captured.CeresFlightFinished, "full intro capture reaches completed flight");
        AssertTrue(phases.Contains(IntroCinematicPhase.MotherBrainFlashback)
            && phases.Contains(IntroCinematicPhase.BabyDiscovery)
            && phases.Contains(IntroCinematicPhase.BabyMetroidExamination), "full intro covers each illustration family");
        Console.WriteLine($"  Intro snapshots: {samples} sampled frames across {phases.Count} phases agree with independent legacy display owner.");
    }

    /// <summary>Runs cinematic snapshot checks together with ending display and credits checks.</summary>
    [VerificationCommand("--cinematic-fixtures")]
    internal static int CinematicFixturesCommand()
    {
     VerifyCinematicRenderSnapshots(); VerifyEndingRenderSnapshots(); VerifyEndingCreditsState(); return 0; 
    }
}
