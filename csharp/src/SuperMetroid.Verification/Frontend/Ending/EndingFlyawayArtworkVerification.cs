using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Rendering;
using static SuperMetroid.Verification.Frontend.Intro.IntroCinematicArtworkVerification;
using static SuperMetroid.Verification.Harness.RetailGameFixtureVerification;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Frontend.Ending;
using SuperMetroid.TestSupport.FrameRendering;

namespace SuperMetroid.Verification.Frontend.Ending;

/// <summary>Checks the ending's flyaway floor, drawn from the Ceres flight art, and debugger artwork rebinding.</summary>
internal static class EndingFlyawayArtworkVerification
{
    /// <summary>
    /// Compares every update of the explosion's sixteen-update hold, after which the flyaway floor is loaded once, without
    /// cartridge art rereads (#1513: the floor is a picture loaded once, not sixteen transfers).
    /// </summary>
    internal static void VerifyEndingFlyawayArtwork()
    {
        string romPath = Path.GetFullPath("Super Metroid.smc");
        var installedBus = SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpaceTooling.LoadRetailRom(romPath);
        var guard = NewIntroArtworkSourceReadGuard(installedBus);
        var nativeAudio = AudioQueues.Standalone();
        var installedAudio = AudioQueues.Standalone();
        var native = CreateRetailEndingFixture(nativeAudio, 0, 0);
        var installed = CreateRetailEndingFixture(installedAudio, 0, 0);
        int holdUpdates = 0;
        for (int frame = 0; frame < 10000 &&
            native.Phase != EndingCreditsPhase.PlanetEscapeFast; frame++)
        {
            AssertEqual(native.Phase, installed.Phase,
                $"installed ending flyaway preserves native phase at frame {frame}");
            if (native.Phase == EndingCreditsPhase.ZebesExplosionHold)
            {
                LayeredScene expected = EndingCreditsPresentation.ComposeScene(native);
                RgbColor[] expectedColours = EndingShownColours.Of(native);
                byte[] expectedVram = native.ShownVram();
                LayeredScene actual = EndingCreditsPresentation.ComposeScene(installed);
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
        AssertEqual(EndingCreditsPhase.PlanetEscapeFast, native.Phase,
            "ending fixture reaches flyaway scene");
        AssertEqual(EndingCreditsPhase.PlanetEscapeFast, installed.Phase,
            "installed ending fixture reaches flyaway scene");
        AssertEqual(EndingExplosionFadeDefinitions.HoldUpdates, holdUpdates,
            "the explosion holds for sixteen updates before its finale");
        AssertEqual(0, guard.ForbiddenReadAttempts,
            "installed ending flyaway never rereads shared Ceres art sources");

        var loadBus = SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpaceTooling.LoadRetailRom(romPath);
        var loadGuard = NewIntroArtworkSourceReadGuard(loadBus);
        var loadAudio = AudioQueues.Standalone();
        var restored = CreateRetailEndingFixture(loadAudio, 0, 0);
        for (int frame = 0; frame < 10000 &&
            restored.Phase != EndingCreditsPhase.ZebesExplosionHold; frame++)
        {
            restored.Step();
            loadAudio.AdvanceFrame(default);
        }
        AssertEqual(EndingCreditsPhase.ZebesExplosionHold, restored.Phase,
            "floor-load fixture reaches the explosion's hold");
        for (int update = 0; update < EndingExplosionFadeDefinitions.HoldUpdates; update++)
        {
            AssertEqual(EndingFloorShot.PlanetExplosion, EndingScreenFloor.ShotOf(restored.Phase),
                $"the exploding planet's floor shows at hold update {update}");
            restored.Step();
            loadAudio.AdvanceFrame(default);
        }
        AssertTrue(EndingScreenFloor.ShotOf(restored.Phase) == EndingFloorShot.Flyaway
                && restored.Phase == EndingCreditsPhase.ZebesExplosionAnimation,
            "the flyaway's floor is the one the phase shows as the hold ends and the finale begins");
        AssertEqual(0, loadGuard.ForbiddenReadAttempts,
            "loading the ending flyaway floor never reads shared cartridge art sources");
        Console.WriteLine("Ending flyaway art: sixteen hold updates and the floor the finale shows pass.");
    }
}
