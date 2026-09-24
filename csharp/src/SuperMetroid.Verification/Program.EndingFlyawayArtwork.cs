using System.Text.Json;
using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Audio;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rendering;

internal static partial class Program
{
    private static void VerifyEndingFlyawayArtwork(GameInstallation installation)
    {
        string romPath = Path.GetFullPath("Super Metroid.smc");
        var nativeBus = SuperMetroidAddressSpace.LoadRetailRom(romPath);
        var installedBus = SuperMetroidAddressSpace.LoadRetailRom(romPath);
        var guard = new IntroArtworkSourceReadGuard(installedBus);
        var nativeAudio = new CartridgeAudioState();
        var installedAudio = new CartridgeAudioState();
        var native = new EndingCreditsState(nativeBus, nativeAudio, 0, 0);
        var installed = new EndingCreditsState(guard, installedAudio, 0, 0);
        installed.BindFlightArtwork(installation.LoadIntroCinematicArt().CeresFlight);
        int uploadedFrames = 0;
        for (int frame = 0; frame < 10000 &&
            native.Phase != EndingCreditsPhase.PlanetEscapeFast; frame++)
        {
            AssertEqual(native.Phase, installed.Phase,
                $"installed ending flyaway preserves native phase at frame {frame}");
            if (native.Phase == EndingCreditsPhase.ZebesExplosionTileUpload)
            {
                LayeredRenderSnapshot expected = native.CaptureRenderSnapshot();
                LayeredRenderSnapshot actual = installed.CaptureRenderSnapshot();
                AssertTrue(actual.Memory.Vram.SequenceEqual(expected.Memory.Vram) &&
                        actual.Memory.Cgram.SequenceEqual(expected.Memory.Cgram) &&
                        SoftwareLayeredSnapshotRenderer.Render(actual).AsSpan().SequenceEqual(
                            SoftwareLayeredSnapshotRenderer.Render(expected)),
                    $"ending Mode-7 DMA and visible pixels match at upload {uploadedFrames}");
                uploadedFrames++;
            }
            native.Step();
            installed.Step();
            nativeAudio.AdvanceFrame(nativeBus, default);
            installedAudio.AdvanceFrame(guard, default);
        }
        AssertEqual(EndingCreditsPhase.PlanetEscapeFast, native.Phase,
            "ending fixture reaches flyaway scene");
        AssertEqual(EndingCreditsPhase.PlanetEscapeFast, installed.Phase,
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
                MapPresentationFormat.JsonOptions)
                ?? throw new InvalidDataException("Stock Ceres flight map is empty.");
        edited.FrontTiles[0] = (edited.FrontTiles[0] + 1) & 255;
        using (var output = File.Create(overridePath))
            CeresFlightArtworkCatalog.WriteMap(output, edited);
        CeresFlightArtworkCatalog overrideArt = installation.LoadIntroCinematicArt().CeresFlight;
        File.Delete(overridePath);

        var rebindBus = SuperMetroidAddressSpace.LoadRetailRom(romPath);
        var rebindGuard = new IntroArtworkSourceReadGuard(rebindBus);
        var rebindAudio = new CartridgeAudioState();
        var restored = new EndingCreditsState(rebindGuard, rebindAudio, 0, 0);
        restored.BindFlightArtwork(installation.LoadIntroCinematicArt().CeresFlight);
        for (int frame = 0; frame < 10000 &&
            restored.Phase != EndingCreditsPhase.ZebesExplosionTileUpload; frame++)
        {
            restored.Step();
            rebindAudio.AdvanceFrame(rebindGuard, default);
        }
        AssertEqual(EndingCreditsPhase.ZebesExplosionTileUpload, restored.Phase,
            "rebind fixture remains inside the native upload phase");
        for (int chunk = 0; chunk < 9; chunk++)
        {
            restored.Step();
            rebindAudio.AdvanceFrame(rebindGuard, default);
        }
        AssertEqual(EndingCreditsPhase.ZebesExplosionTileUpload, restored.Phase,
            "first map chunk leaves seven scheduled uploads pending");
        byte originalMapByte = restored.CaptureRenderSnapshot().Memory.Vram[0];
        restored.BindFlightArtwork(overrideArt);
        AssertEqual((byte)edited.FrontTiles[0], restored.CaptureRenderSnapshot().Memory.Vram[0],
            "restored ending reuploads the edited front-view map byte");
        AssertTrue(originalMapByte != edited.FrontTiles[0],
            "front-view edit changes an already transferred ending map byte");
        AssertEqual(0, rebindGuard.ForbiddenReadAttempts,
            "ending debugger rebind never reads shared cartridge art sources");
        Console.WriteLine("Ending flyaway art: sixteen native upload frames, shared Ceres edits and restore pass.");
    }
}
