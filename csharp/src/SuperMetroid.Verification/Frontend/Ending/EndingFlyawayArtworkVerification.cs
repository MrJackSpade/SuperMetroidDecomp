using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Rendering;
using static SuperMetroid.Verification.Frontend.Intro.IntroCinematicArtworkVerification;
using static SuperMetroid.Verification.Harness.RetailGameFixtureVerification;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Frontend.Ending;
using SuperMetroid.AssetExtraction.Installation;
using SuperMetroid.TestSupport.FrameRendering;

namespace SuperMetroid.Verification.Frontend.Ending;

/// <summary>Checks the ending's flyaway floor, drawn from the Ceres flight art, and debugger artwork rebinding.</summary>
internal static class EndingFlyawayArtworkVerification
{
    /// <summary>
    /// Compares every update of the explosion's sixteen-update hold, after which the flyaway floor is loaded once, without
    /// cartridge art rereads (#1513: the floor is a picture loaded once, not sixteen transfers).
    /// </summary>
    internal static void VerifyEndingFlyawayArtwork(GameInstallation installation)
    {
        string romPath = Path.GetFullPath("Super Metroid.smc");
        var installedBus = SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpaceTooling.LoadRetailRom(romPath);
        var guard = NewIntroArtworkSourceReadGuard(installedBus);
        var nativeAudio = new AudioQueues();
        var installedAudio = new AudioQueues();
        var native = CreateRetailEndingFixture(nativeAudio, 0, 0);
        var installed = CreateRetailEndingFixture(installedAudio, 0, 0);
        installed.Flyaway.BindFlightArtwork(installation.LoadIntroCinematicArt().CeresFlight);
        int holdUpdates = 0;
        for (int frame = 0; frame < 10000 &&
            native.Stage.Phase != EndingCreditsPhase.PlanetEscapeFast; frame++)
        {
            AssertEqual(native.Stage.Phase, installed.Stage.Phase,
                $"installed ending flyaway preserves native phase at frame {frame}");
            if (native.Stage.Phase == EndingCreditsPhase.ZebesExplosionHold)
            {
                LayeredScene expected = EndingCreditsPresentation.CaptureRenderSnapshot(native);
                RgbColor[] expectedColours = EndingShownColours.Of(native);
                byte[] expectedVram = native.ShownVram();
                LayeredScene actual = EndingCreditsPresentation.CaptureRenderSnapshot(installed);
                RgbColor[] actualColours = EndingShownColours.Of(installed);
                byte[] actualVram = installed.ShownVram();
                AssertTrue(actualVram.SequenceEqual(expectedVram) &&
                        actualColours.SequenceEqual(expectedColours) &&
                        ScenePixels.Same(actual, expected),
                    $"ending floor and visible pixels match at hold update {holdUpdates}");
                holdUpdates++;
            }
            native.Step();
            installed.Step();
            nativeAudio.AdvanceFrame(default);
            installedAudio.AdvanceFrame(default);
        }
        AssertEqual(EndingCreditsPhase.PlanetEscapeFast, native.Stage.Phase,
            "ending fixture reaches flyaway scene");
        AssertEqual(EndingCreditsPhase.PlanetEscapeFast, installed.Stage.Phase,
            "installed ending fixture reaches flyaway scene");
        AssertEqual(EndingExplosionFadeDefinitions.HoldUpdates, holdUpdates,
            "the explosion holds for sixteen updates before its finale");
        AssertEqual(0, guard.ForbiddenReadAttempts,
            "installed ending flyaway never rereads shared Ceres art sources");

        var loadBus = SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpaceTooling.LoadRetailRom(romPath);
        var loadGuard = NewIntroArtworkSourceReadGuard(loadBus);
        var loadAudio = new AudioQueues();
        var restored = CreateRetailEndingFixture(loadAudio, 0, 0);
        restored.Flyaway.BindFlightArtwork(installation.LoadIntroCinematicArt().CeresFlight);
        for (int frame = 0; frame < 10000 &&
            restored.Stage.Phase != EndingCreditsPhase.ZebesExplosionHold; frame++)
        {
            restored.Step();
            loadAudio.AdvanceFrame(default);
        }
        AssertEqual(EndingCreditsPhase.ZebesExplosionHold, restored.Stage.Phase,
            "floor-load fixture reaches the explosion's hold");
        for (int update = 0; update < EndingExplosionFadeDefinitions.HoldUpdates; update++)
        {
            AssertEqual(EndingFloorShot.PlanetExplosion, EndingScreenFloor.ShotOf(restored.Stage.Phase),
                $"the exploding planet's floor shows at hold update {update}");
            restored.Step();
            loadAudio.AdvanceFrame(default);
        }
        AssertTrue(EndingScreenFloor.ShotOf(restored.Stage.Phase) == EndingFloorShot.Flyaway
                && restored.Stage.Phase == EndingCreditsPhase.ZebesExplosionAnimation,
            "the flyaway's floor is the one the phase shows as the hold ends and the finale begins");
        AssertEqual(0, loadGuard.ForbiddenReadAttempts,
            "loading the ending flyaway floor never reads shared cartridge art sources");
        Console.WriteLine("Ending flyaway art: sixteen hold updates and the floor the finale shows pass.");
    }
}
