using SuperMetroid.Core.Audio;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    static void VerifyPausePaletteSound()
    {
        string rom = Path.GetFullPath("Super Metroid.smc");
        if (!File.Exists(rom)) throw new FileNotFoundException("Pause palette integration requires the private ROM.", rom);
        var bus = SuperMetroidAddressSpace.LoadRetailRom(rom);
        var audio = new CartridgeAudioState();
        var pause = new PauseMenuState(bus, new SamusState(), new Bank80SystemState(), AreaId.Crateria, 0, 0, audio);
        var assets = ExtractedAudioAssetCatalog.Load(Path.GetFullPath("standalone-assets/audio"));
        var player = new ManagedSpcPlayer();
        player.Upload(assets.GetUpload(AudioUploadAddresses.SpcEngine).Span);
        player.SetSampleBank(assets.GetSampleBank(AudioUploadAddresses.SpcEngine));
        short[] pcm = new short[1600];
        int requests = 0;
        int nonzeroSamples = 0;
        int soundWrites = 0;
        for (int frame = 0; frame < 500; frame++)
        {
            pause.AdvanceAnimations();
            // Retail timings: initial frame advances after one tick, thirteen frames
            // last three ticks each, and frame zero lasts fifteen ticks on later loops.
            bool expectedLoop = frame >= 39 && (frame - 39) % 54 == 0;
            AssertEqual(expectedLoop, audio.HasQueuedSounds, $"pause beep request cadence at frame {frame}");
            if (expectedLoop) requests++;
            var acknowledgements = new CartridgeAudioAcknowledgements(player.ReadPort(0), player.ReadPort(1), player.ReadPort(2), player.ReadPort(3));
            foreach (var command in audio.AdvanceFrame(bus, acknowledgements))
            {
                if (command.Kind == CartridgeAudioCommandKind.Upload)
                {
                    AssertEqual(0, frame, "only initial audio reset may upload during this pause probe");
                    player.Upload(assets.GetUpload(command.UploadAddress).Span);
                    player.SetSampleBank(assets.GetSampleBank(command.UploadAddress));
                    continue;
                }
                player.WritePort(command.Port, command.Value);
                if (command.Port == 3 && command.Value == SoundEffectLibrary3Sounds.MapPaletteLoop.Value) soundWrites++;
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
