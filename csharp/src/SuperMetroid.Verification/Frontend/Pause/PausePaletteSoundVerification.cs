using static SuperMetroid.Verification.Harness.RetailPresentationFixtureVerification;
using SuperMetroid.Core.Frontend.Pause;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Samus;
using SuperMetroid.TestSupport.RepositoryInstallation;
namespace SuperMetroid.Verification.Frontend.Pause;

/// <summary>Checks the pause palette's 54-tick loop queues its map beep and
/// produces audible SPC output at the authored cadence.</summary>
internal static class PausePaletteSoundVerification
{
    /// <summary>Advances 500 pause animation ticks and checks request timing,
    /// port delivery, and generated PCM for each palette-loop beep.</summary>
    internal static void VerifyPausePaletteSound()
    {
        string rom = Path.GetFullPath("Super Metroid.smc");
        if (!File.Exists(rom)) throw new FileNotFoundException("Pause palette integration requires the private ROM.", rom);
        var bus = SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpaceTooling.LoadRetailRom(rom);
        var audio = new CartridgeAudioState();
        var pause = CreateRetailPauseFixture(bus, new PauseMenuEntry { Samus = new SamusState(), System = new Bank80SystemState(), AreaIndex = AreaId.Crateria, RoomMapPosition = new(0, 0), Audio = audio });
        var assets = ExtractedAudioAssetCatalog.Load(RepositoryInstallation.Installation.AudioDirectory);
        var player = new ManagedSpcPlayer();
        player.Upload(assets.GetUpload(AudioUploadAddresses.SpcEngine).Span);
        player.SetSampleBank(assets.GetSampleBank(AudioUploadAddresses.SpcEngine));
        short[] pcm = new short[1600];
        int requests = 0;
        int nonzeroSamples = 0;
        int soundWrites = 0;
        // SoundState_1 ($82:8A5E) rewrites the request until the APU acknowledges it, so one
        // beep can reach port three on consecutive frames. Count requests, not port writes.
        byte lastPort3 = 0;
        for (int frame = 0; frame < 500; frame++)
        {
            pause.AdvanceAnimations();
            // Retail timings: initial frame advances after one tick, thirteen frames
            // last three ticks each, and frame zero lasts fifteen ticks on later loops.
            bool expectedLoop = frame >= 39 && (frame - 39) % 54 == 0;
            AssertEqual(expectedLoop, audio.HasQueuedSounds, $"pause beep request cadence at frame {frame}");
            if (expectedLoop) requests++;
            var acknowledgements = new CartridgeAudioAcknowledgements(player.ReadPort(ApuPort.Music), player.ReadPort(ApuPort.SoundLibrary1), player.ReadPort(ApuPort.SoundLibrary2), player.ReadPort(ApuPort.SoundLibrary3));
            foreach (var command in audio.AdvanceFrame(bus, acknowledgements))
            {
                if (command.Kind == CartridgeAudioCommandKind.Upload)
                {
                    AssertEqual(0, frame, "only initial audio reset may upload during this pause probe");
                    player.Upload(assets.GetUpload(command.UploadAddress).Span);
                    player.SetSampleBank(assets.GetSampleBank(command.UploadAddress));
                    continue;
                }
                player.WritePort(command.InputPort, command.Value);
                if (command.InputPort == ApuPort.SoundLibrary3)
                {
                    if (command.Value == SoundEffectLibrary3Sounds.MapPaletteLoop.Value && lastPort3 != command.Value)
                        soundWrites++;
                    lastPort3 = command.Value;
                }
            }
            player.GenerateFrame(pcm);
            int audible = pcm.Count(sample => sample != 0);
            if (frame < 39) AssertEqual(0, audible, "isolated pause sound has no premature PCM");
            nonzeroSamples += audible;
        }
        AssertEqual(9, requests, "nine palette loops in 500 frames");
        AssertEqual(requests, soundWrites, "every palette beep reaches its SPC port");
        AssertTrue(nonzeroSamples > 0, "pause beep produces actual PCM rather than only queued commands");
        Console.WriteLine($"  Pause beep: nine exact 54-frame loops reach SPC; {nonzeroSamples} nonzero PCM samples.");
    }
}
