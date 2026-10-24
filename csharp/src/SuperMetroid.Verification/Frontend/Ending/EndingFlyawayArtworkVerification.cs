using SuperMetroid.Core.Frontend;
using System.Text.Json;
using SuperMetroid.Core.Rendering;
using static SuperMetroid.Verification.Frontend.Intro.IntroCinematicArtworkVerification;
using static SuperMetroid.Verification.Harness.RetailGameFixtureVerification;
using SuperMetroid.Core.Frontend.Ending;
using SuperMetroid.AssetExtraction.Installation;
using SuperMetroid.Core.Frontend.CeresCinematics.Artwork;
using SuperMetroid.Core.Assets;
using SuperMetroid.TestSupport.FrameRendering;

namespace SuperMetroid.Verification.Frontend.Ending;

/// <summary>Checks the ending's Ceres flight art uploads and debugger artwork rebinding.</summary>
internal static class EndingFlyawayArtworkVerification
{
    /// <summary>Compares all 16 scheduled Mode 7 uploads and checks an edited map byte without cartridge art rereads.</summary>
    internal static void VerifyEndingFlyawayArtwork(GameInstallation installation)
    {
        string romPath = Path.GetFullPath("Super Metroid.smc");
        var installedBus = SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpaceTooling.LoadRetailRom(romPath);
        var guard = NewIntroArtworkSourceReadGuard(installedBus);
        var nativeAudio = new CartridgeAudioState();
        var installedAudio = new CartridgeAudioState();
        var native = CreateRetailEndingFixture(nativeAudio, 0, 0);
        var installed = CreateRetailEndingFixture(installedAudio, 0, 0);
        installed.Flyaway.BindFlightArtwork(installation.LoadIntroCinematicArt().CeresFlight);
        int uploadedFrames = 0;
        for (int frame = 0; frame < 10000 &&
            native.Stage.Phase != EndingCreditsPhase.PlanetEscapeFast; frame++)
        {
            AssertEqual(native.Stage.Phase, installed.Stage.Phase,
                $"installed ending flyaway preserves native phase at frame {frame}");
            if (native.Stage.Phase == EndingCreditsPhase.ZebesExplosionTileUpload)
            {
                LayeredScene expected = EndingCreditsPresentation.CaptureRenderSnapshot(native);
                byte[] expectedVram = native.ShownVram();
                LayeredScene actual = EndingCreditsPresentation.CaptureRenderSnapshot(installed);
                byte[] actualVram = installed.ShownVram();
                AssertTrue(actualVram.SequenceEqual(expectedVram) &&
                        actual.Paint.Colors.SequenceEqual(expected.Paint.Colors) &&
                        DisplayRendering.Render(actual).SequenceEqual(
                            DisplayRendering.Render(expected)),
                    $"ending Mode-7 DMA and visible pixels match at upload {uploadedFrames}");
                uploadedFrames++;
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
        AssertEqual(16, uploadedFrames,
            "the cartridge still uploads eight character and eight map chunks");
        AssertEqual(0, guard.ForbiddenReadAttempts,
            "installed ending flyaway never rereads shared Ceres art sources");

        // Rebind the player's current front-view map after the first low-byte DMA.
        // The pending chunks must retain their native schedule, not upload early.
        string overridePath = Path.Combine(installation.IntroCinematicOverrideDirectory,
            CeresFlightArtworkFormat.MapFileName);
        CeresFlightMapDocument edited;
        using (var input = File.OpenRead(Path.Combine(installation.IntroCinematicDirectory,
                   CeresFlightArtworkFormat.MapFileName)))
            edited = JsonSerializer.Deserialize<CeresFlightMapDocument>(input,
                ArtworkJson.Options)
                ?? throw new InvalidDataException("Stock Ceres flight map is empty.");
        edited.FrontTiles[0] = unchecked((byte)(edited.FrontTiles[0] + 1));
        Directory.CreateDirectory(installation.IntroCinematicOverrideDirectory);
        using (var output = File.Create(overridePath))
            CeresFlightArtworkCatalog.WriteMap(output, edited);
        CeresFlightArtworkCatalog overrideArt = installation.LoadIntroCinematicArt().CeresFlight;
        File.Delete(overridePath);

        var rebindBus = SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpaceTooling.LoadRetailRom(romPath);
        var rebindGuard = NewIntroArtworkSourceReadGuard(rebindBus);
        var rebindAudio = new CartridgeAudioState();
        var restored = CreateRetailEndingFixture(rebindAudio, 0, 0);
        restored.Flyaway.BindFlightArtwork(installation.LoadIntroCinematicArt().CeresFlight);
        for (int frame = 0; frame < 10000 &&
            restored.Stage.Phase != EndingCreditsPhase.ZebesExplosionTileUpload; frame++)
        {
            restored.Step();
            rebindAudio.AdvanceFrame(default);
        }
        AssertEqual(EndingCreditsPhase.ZebesExplosionTileUpload, restored.Stage.Phase,
            "rebind fixture remains inside the native upload phase");
        for (int chunk = 0; chunk < 9; chunk++)
        {
            restored.Step();
            rebindAudio.AdvanceFrame(default);
        }
        AssertEqual(EndingCreditsPhase.ZebesExplosionTileUpload, restored.Stage.Phase,
            "first map chunk leaves seven scheduled uploads pending");
        byte originalMapByte = restored.ShownVram()[0];
        restored.Flyaway.BindFlightArtwork(overrideArt);
        AssertEqual((byte)edited.FrontTiles[0], restored.ShownVram()[0],
            "restored ending reuploads the edited front-view map byte");
        AssertTrue(originalMapByte != edited.FrontTiles[0],
            "front-view edit changes an already transferred ending map byte");
        AssertEqual(0, rebindGuard.ForbiddenReadAttempts,
            "ending debugger rebind never reads shared cartridge art sources");
        Console.WriteLine("Ending flyaway art: sixteen native upload frames, shared Ceres edits and restore pass.");
    }
}
