using System.Runtime.InteropServices;
using System.Security.Cryptography;
using SuperMetroid.Core.Audio;

namespace SuperMetroid.Desktop;

public readonly record struct ManagedAudioRegressionSmokeTestResult(
    int Scenarios,
    int Frames,
    int PcmSamples,
    string PcmSha256,
    string AcknowledgementSha256);

/// <summary>
/// Permanent, ROM-free regression corpus captured only after every byte matched the pinned
/// native oracle. It covers every music bank, three SFX libraries, overlap/cancellation,
/// bank changes, stop, pause/resume, and track restoration.
/// </summary>
public static class ManagedAudioRegressionSmokeTest
{
    private const string ExpectedPcmSha256 =
        "62656660AB1B53265C88CB01BB1215F0E83D7D63A2978C1D7A5DF60D55D44F85";
    private const string ExpectedAcknowledgementSha256 =
        "DF414B59F7CA21C4BBAD7ABA9C379C8296DCA454B480BFCEFCB8B35123D851FD";
    private const int ShortScenarioFrames = 120;
    private const int TitleScenarioFrames = 600;
    private const int LifecycleScenarioFrames = 600;

    public static ManagedAudioRegressionSmokeTestResult Run()
    {
        using IncrementalHash pcmHash = IncrementalHash.CreateHash(HashAlgorithmName.SHA256);
        using IncrementalHash acknowledgementHash = IncrementalHash.CreateHash(HashAlgorithmName.SHA256);
        int scenarios = 0;
        int frames = 0;
        int samples = 0;

        RunMusicScenario(
            "title-long",
            AudioAssetCatalogData.Music[0],
            TitleScenarioFrames,
            pcmHash,
            acknowledgementHash,
            ref scenarios,
            ref frames,
            ref samples);
        foreach (AudioUploadAssetDefinition bank in AudioAssetCatalogData.Music)
        {
            RunMusicScenario(
                bank.Name,
                bank,
                ShortScenarioFrames,
                pcmHash,
                acknowledgementHash,
                ref scenarios,
                ref frames,
                ref samples);
        }

        RunSoundScenario(
            "library-1-power-beam",
            [(AudioRomData.Apu.LibraryOnePort, SoundEffectLibrary1Sounds.PowerBeam.Value)],
            pcmHash,
            acknowledgementHash,
            ref scenarios,
            ref frames,
            ref samples);
        RunSoundScenario(
            "library-2-door",
            [(AudioRomData.Apu.LibraryTwoPort, SoundEffectLibrary2Sounds.DoorOpening.Value)],
            pcmHash,
            acknowledgementHash,
            ref scenarios,
            ref frames,
            ref samples);
        RunSoundScenario(
            "library-3-cinematic",
            [(AudioRomData.Apu.LibraryThreePort, 0x23)],
            pcmHash,
            acknowledgementHash,
            ref scenarios,
            ref frames,
            ref samples);
        RunSoundScenario(
            "three-library-overlap-and-cancel",
            [
                (AudioRomData.Apu.LibraryOnePort, SoundEffectLibrary1Sounds.PowerBeam.Value),
                (AudioRomData.Apu.LibraryTwoPort, SoundEffectLibrary2Sounds.DoorOpening.Value),
                (AudioRomData.Apu.LibraryThreePort, 0x23),
            ],
            pcmHash,
            acknowledgementHash,
            ref scenarios,
            ref frames,
            ref samples,
            cancelHalfway: true);
        RunLifecycleScenario(
            pcmHash,
            acknowledgementHash,
            ref scenarios,
            ref frames,
            ref samples);

        string pcm = Convert.ToHexString(pcmHash.GetHashAndReset());
        string acknowledgements = Convert.ToHexString(acknowledgementHash.GetHashAndReset());
        if (!pcm.Equals(ExpectedPcmSha256, StringComparison.Ordinal))
            throw new InvalidDataException($"Managed audio PCM regression: {pcm}, expected {ExpectedPcmSha256}.");
        if (!acknowledgements.Equals(ExpectedAcknowledgementSha256, StringComparison.Ordinal))
        {
            throw new InvalidDataException(
                $"Managed audio acknowledgement regression: {acknowledgements}, expected {ExpectedAcknowledgementSha256}.");
        }
        return new ManagedAudioRegressionSmokeTestResult(
            scenarios, frames, samples, pcm, acknowledgements);
    }

    private static void RunMusicScenario(
        string name,
        AudioUploadAssetDefinition bank,
        int frameCount,
        IncrementalHash pcmHash,
        IncrementalHash acknowledgementHash,
        ref int scenarios,
        ref int frames,
        ref int samples)
    {
        using var engine = new SpcAudioEngine();
        for (int frame = 0; frame < frameCount; frame++)
        {
            IReadOnlyList<CartridgeAudioCommand> commands = frame == 0
                ?
                [
                    CartridgeAudioCommand.Upload(AudioAssetCatalogData.Common.SnesAddress),
                    CartridgeAudioCommand.Upload(bank.SnesAddress),
                    CartridgeAudioCommand.WritePort(AudioRomData.Apu.MusicPort, 1),
                ]
                : Array.Empty<CartridgeAudioCommand>();
            HashFrame(name, engine, commands, pcmHash, acknowledgementHash, ref frames, ref samples);
        }
        scenarios++;
    }

    private static void RunSoundScenario(
        string name,
        IReadOnlyList<(byte Port, byte Command)> startCommands,
        IncrementalHash pcmHash,
        IncrementalHash acknowledgementHash,
        ref int scenarios,
        ref int frames,
        ref int samples,
        bool cancelHalfway = false)
    {
        using var engine = new SpcAudioEngine();
        for (int frame = 0; frame < ShortScenarioFrames; frame++)
        {
            List<CartridgeAudioCommand> commands = [];
            if (frame == 0)
            {
                commands.Add(CartridgeAudioCommand.Upload(AudioAssetCatalogData.Common.SnesAddress));
                commands.AddRange(startCommands.Select(command =>
                    CartridgeAudioCommand.WritePort(command.Port, command.Command)));
            }
            if (cancelHalfway && frame == ShortScenarioFrames / 2)
            {
                commands.Add(CartridgeAudioCommand.WritePort(
                    AudioRomData.Apu.LibraryOnePort, SoundEffectLibrary1Sounds.CancelAll.Value));
                commands.Add(CartridgeAudioCommand.WritePort(
                    AudioRomData.Apu.LibraryTwoPort, SoundEffectLibrary2Sounds.CancelAll.Value));
                commands.Add(CartridgeAudioCommand.WritePort(
                    AudioRomData.Apu.LibraryThreePort, SoundEffectLibrary3Sounds.CancelAll.Value));
            }
            HashFrame(name, engine, commands, pcmHash, acknowledgementHash, ref frames, ref samples);
        }
        scenarios++;
    }

    private static void RunLifecycleScenario(
        IncrementalHash pcmHash,
        IncrementalHash acknowledgementHash,
        ref int scenarios,
        ref int frames,
        ref int samples)
    {
        AudioUploadAssetDefinition first = AudioAssetCatalogData.Music[3];
        AudioUploadAssetDefinition second = AudioAssetCatalogData.Music[4];
        using var engine = new SpcAudioEngine();
        for (int frame = 0; frame < LifecycleScenarioFrames; frame++)
        {
            List<CartridgeAudioCommand> commands = [];
            if (frame == 0)
            {
                commands.Add(CartridgeAudioCommand.Upload(AudioAssetCatalogData.Common.SnesAddress));
                commands.Add(CartridgeAudioCommand.Upload(first.SnesAddress));
                commands.Add(CartridgeAudioCommand.WritePort(AudioRomData.Apu.MusicPort, 1));
            }
            else if (frame == 120)
            {
                commands.Add(CartridgeAudioCommand.WritePort(AudioRomData.Apu.MusicPort, 0));
            }
            else if (frame == 150)
            {
                commands.Add(CartridgeAudioCommand.Upload(second.SnesAddress));
                commands.Add(CartridgeAudioCommand.WritePort(AudioRomData.Apu.MusicPort, 1));
            }
            else if (frame == 270)
            {
                commands.Add(CartridgeAudioCommand.WritePort(
                    AudioRomData.Apu.MusicPort, AudioRomData.Apu.PauseMusic));
            }
            else if (frame == 330)
            {
                commands.Add(CartridgeAudioCommand.WritePort(
                    AudioRomData.Apu.MusicPort, AudioRomData.Apu.ResumeMusic));
            }
            else if (frame == 390)
            {
                commands.Add(CartridgeAudioCommand.WritePort(AudioRomData.Apu.MusicPort, 2));
            }
            else if (frame == 510)
            {
                commands.Add(CartridgeAudioCommand.WritePort(AudioRomData.Apu.MusicPort, 1));
            }
            HashFrame(
                "music-lifecycle", engine, commands, pcmHash, acknowledgementHash,
                ref frames, ref samples);
        }
        scenarios++;
    }

    private static void HashFrame(
        string scenario,
        SpcAudioEngine engine,
        IReadOnlyList<CartridgeAudioCommand> commands,
        IncrementalHash pcmHash,
        IncrementalHash acknowledgementHash,
        ref int frames,
        ref int samples)
    {
        byte[] name = System.Text.Encoding.UTF8.GetBytes(scenario);
        pcmHash.AppendData(name);
        acknowledgementHash.AppendData(name);
        ReadOnlySpan<short> pcm = engine.RenderFrame(commands);
        pcmHash.AppendData(MemoryMarshal.AsBytes(pcm));
        CartridgeAudioAcknowledgements acknowledgements = engine.ReadAcknowledgements();
        Span<byte> ports = stackalloc byte[AudioRomData.Apu.PortCount];
        for (int port = 0; port < ports.Length; port++)
            ports[port] = acknowledgements[port];
        acknowledgementHash.AppendData(ports);
        frames++;
        samples += pcm.Length;
    }
}
