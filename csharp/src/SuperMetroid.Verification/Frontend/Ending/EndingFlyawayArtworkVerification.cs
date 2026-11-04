using SuperMetroid.AssetExtraction.Areas.Ceres;
using SuperMetroid.AssetExtraction.Frontend.CeresCinematics.Artwork;
using SuperMetroid.Core.Frontend;
using System.Text.Json;
using SuperMetroid.Core.Rendering;
using static SuperMetroid.Verification.Frontend.Intro.IntroCinematicArtworkVerification;
using static SuperMetroid.Verification.Harness.RetailGameFixtureVerification;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Frontend.Ending;
using SuperMetroid.AssetExtraction.Installation;
using SuperMetroid.Core.Frontend.CeresCinematics.Artwork;
using SuperMetroid.TestSupport.FrameRendering;
using SuperMetroid.AssetExtraction;

namespace SuperMetroid.Verification.Frontend.Ending;

/// <summary>Checks the ending's flyaway floor, drawn from the Ceres flight art, and debugger artwork rebinding.</summary>
internal static class EndingFlyawayArtworkVerification
{
    /// <summary>
    /// Compares every update of the explosion's sixteen-update hold, after which the flyaway floor is loaded, and checks
    /// an edited map byte without cartridge art rereads (#1513: the floor is a picture loaded once, not sixteen transfers).
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
                        DisplayRendering.Render(actual).SequenceEqual(
                            DisplayRendering.Render(expected)),
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

        // Rebind the player's current front-view map once the flyaway floor is loaded.
        string overridePath = Path.Combine(installation.IntroCinematicOverrideDirectory,
            CeresFlightArtworkFileFormat.MapFileName);
        CeresFlightMapDocument edited;
        using (var input = File.OpenRead(Path.Combine(installation.IntroCinematicDirectory,
                   CeresFlightArtworkFileFormat.MapFileName)))
            edited = JsonSerializer.Deserialize<CeresFlightMapDocument>(input,
                ArtworkJson.Options)
                ?? throw new InvalidDataException("Stock Ceres flight map is empty.");
        edited.FrontTiles[0] = unchecked((byte)(edited.FrontTiles[0] + 1));
        Directory.CreateDirectory(installation.IntroCinematicOverrideDirectory);
        using (var output = File.Create(overridePath))
            CeresFlightCharacters.WriteMap(output, edited);
        CeresFlightArtwork overrideArt = installation.LoadIntroCinematicArt().CeresFlight;
        File.Delete(overridePath);

        var rebindBus = SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpaceTooling.LoadRetailRom(romPath);
        var rebindGuard = NewIntroArtworkSourceReadGuard(rebindBus);
        var rebindAudio = new AudioQueues();
        var restored = CreateRetailEndingFixture(rebindAudio, 0, 0);
        restored.Flyaway.BindFlightArtwork(installation.LoadIntroCinematicArt().CeresFlight);
        for (int frame = 0; frame < 10000 &&
            restored.Stage.Phase != EndingCreditsPhase.ZebesExplosionHold; frame++)
        {
            restored.Step();
            rebindAudio.AdvanceFrame(default);
        }
        AssertEqual(EndingCreditsPhase.ZebesExplosionHold, restored.Stage.Phase,
            "rebind fixture reaches the explosion's hold");
        for (int update = 0; update < EndingExplosionFadeDefinitions.HoldUpdates; update++)
        {
            AssertTrue(!restored.screen.FlyawayFloorLoaded, $"the flyaway floor is not loaded at hold update {update}");
            restored.Step();
            rebindAudio.AdvanceFrame(default);
        }
        AssertTrue(restored.screen.FlyawayFloorLoaded && restored.Stage.Phase == EndingCreditsPhase.ZebesExplosionAnimation,
            "the flyaway floor is loaded as the hold ends and the finale begins");
        byte originalMapByte = restored.ShownVram()[0];
        restored.Flyaway.BindFlightArtwork(overrideArt);
        AssertEqual((byte)edited.FrontTiles[0], restored.ShownVram()[0],
            "restored ending reuploads the edited front-view map byte");
        AssertTrue(originalMapByte != edited.FrontTiles[0],
            "front-view edit changes an already loaded ending map byte");
        AssertEqual(0, rebindGuard.ForbiddenReadAttempts,
            "ending debugger rebind never reads shared cartridge art sources");
        Console.WriteLine("Ending flyaway art: sixteen hold updates, shared Ceres edits and restore pass.");
    }
}
