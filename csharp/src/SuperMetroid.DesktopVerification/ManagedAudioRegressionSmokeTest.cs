using System.Runtime.InteropServices;
using System.Security.Cryptography;
using SuperMetroid.Core.Audio;

namespace SuperMetroid.Desktop;

/// <summary>Aggregate corpus counts and verified PCM/acknowledgement digests for the installed managed-audio regression scenarios.</summary>
/// <param name="Scenarios">Completed music, sound-effect, and lifecycle scenarios.</param>
/// <param name="Frames">Total audio updates rendered across all scenarios.</param>
/// <param name="PcmSamples">Number of signed 16-bit interleaved PCM values hashed, counting both stereo channels separately.</param>
/// <param name="CanonicalSamples">Distinct canonical waveforms in the installed catalog.</param>
/// <param name="SourceAliases">Installed source-number mappings across uploads.</param>
/// <param name="PcmSha256">Uppercase SHA-256 of scenario names and the rendered PCM bytes in corpus order.</param>
/// <param name="AcknowledgementSha256">Uppercase SHA-256 of scenario names and four-port acknowledgement bytes in corpus order.</param>
public readonly record struct ManagedAudioRegressionSmokeTestResult(
    int Scenarios,
    int Frames,
    int PcmSamples,
    int CanonicalSamples,
    int SourceAliases,
    string PcmSha256,
    string AcknowledgementSha256);

/// <summary>
/// Permanent, ROM-free regression corpus captured only after every byte matched the pinned
/// native translation. It covers every music-bank upload, three SFX libraries,
/// overlap/cancellation, bank changes, stop, pause/resume, and shared-cue restoration.
/// The music scenarios request shared track one, not bank-specific track five; their
/// hashes therefore do not establish room-music correctness. The original SPC CPU
/// cancellation probe exercises bank music separately and is not this corpus's oracle.
/// </summary>
public static class ManagedAudioRegressionSmokeTest
{
    /// <summary>Expected digest of scenario identifiers and rendered interleaved PCM bytes.</summary>
    private const string ExpectedPcmSha256 =
        "06FE91966B29B05F2012C6A542864B5692B178A7B028C2A4A56F6D34E8F8D06C";

    /// <summary>Expected digest of scenario identifiers and four-port audio acknowledgements.</summary>
    private const string ExpectedAcknowledgementSha256 =
        "DF414B59F7CA21C4BBAD7ABA9C379C8296DCA454B480BFCEFCB8B35123D851FD";

    /// <summary>Frame count for each short music and sound-effect scenario.</summary>
    private const int ShortScenarioFrames = 120;

    /// <summary>Frame count for the title music scenario.</summary>
    private const int TitleScenarioFrames = 600;

    /// <summary>Frame count for the music stop, bank-change, pause, and resume sequence.</summary>
    private const int LifecycleScenarioFrames = 600;

    /// <summary>Required number of distinct canonical waveforms in the installed audio catalog.</summary>
    private const int ExpectedCanonicalSampleCount = 112;

    /// <summary>Required number of source-number mappings in the installed audio catalog.</summary>
    private const int ExpectedSourceAliasCount = 935;

    /// <summary>Loads installed audio assets, checks catalog dimensions, and compares the fixed music/SFX/lifecycle corpus against its pinned PCM and acknowledgement digests without a ROM or audio device.</summary>
    /// <remarks>Music scenarios select shared track one, so passing this corpus does not establish bank-specific room-music correctness.</remarks>
    public static ManagedAudioRegressionSmokeTestResult Run()
    {
        ExtractedAudioAssetCatalog catalog = ExtractedAudioAssetCatalog.Load(
            ExtractedAudioAssetLocator.FindAudioDirectory());
        if (catalog.CanonicalSampleCount != ExpectedCanonicalSampleCount ||
            catalog.SourceMappingCount != ExpectedSourceAliasCount)
        {
            throw new InvalidDataException(
                $"Managed audio catalog has {catalog.CanonicalSampleCount} canonical samples/" +
                $"{catalog.SourceMappingCount} aliases; expected {ExpectedCanonicalSampleCount}/" +
                $"{ExpectedSourceAliasCount}.");
        }
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
            scenarios,
            frames,
            samples,
            catalog.CanonicalSampleCount,
            catalog.SourceMappingCount,
            pcm,
            acknowledgements);
    }

    /// <summary>Renders one uploaded music bank for a fixed duration and appends its PCM and acknowledgement data to the corpus hashes.</summary>
    /// <param name="name">Scenario identifier included in both digests.</param>
    /// <param name="bank">Music upload selected for this run.</param>
    /// <param name="frameCount">Number of audio frames to render.</param>
    /// <param name="pcmHash">Incremental digest receiving the scenario name and PCM bytes.</param>
    /// <param name="acknowledgementHash">Incremental digest receiving the scenario name and audio-port acknowledgements.</param>
    /// <param name="scenarios">Aggregate scenario count updated after completion.</param>
    /// <param name="frames">Aggregate rendered frame count updated by each frame.</param>
    /// <param name="samples">Aggregate interleaved PCM sample count updated by each frame.</param>
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
        using var engine = DesktopAccess.CreateAudioEngine();
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

    /// <summary>Renders a short SFX command sequence, optionally issuing cancellation commands halfway through.</summary>
    /// <param name="name">Scenario identifier included in both digests.</param>
    /// <param name="startCommands">Port writes issued at the beginning of the scenario.</param>
    /// <param name="pcmHash">Incremental digest receiving rendered PCM bytes.</param>
    /// <param name="acknowledgementHash">Incremental digest receiving audio-port acknowledgements.</param>
    /// <param name="scenarios">Aggregate scenario count updated after completion.</param>
    /// <param name="frames">Aggregate rendered frame count updated by each frame.</param>
    /// <param name="samples">Aggregate interleaved PCM sample count updated by each frame.</param>
    /// <param name="cancelHalfway">Whether to send each library's cancel-all command at the midpoint.</param>
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
        using var engine = DesktopAccess.CreateAudioEngine();
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

    /// <summary>Hashes a fixed music sequence covering stop, bank replacement, pause, resume, and track changes.</summary>
    /// <param name="pcmHash">Incremental digest receiving the scenario name and PCM bytes.</param>
    /// <param name="acknowledgementHash">Incremental digest receiving the scenario name and audio-port acknowledgements.</param>
    /// <param name="scenarios">Aggregate scenario count updated after completion.</param>
    /// <param name="frames">Aggregate rendered frame count updated by each frame.</param>
    /// <param name="samples">Aggregate interleaved PCM sample count updated by each frame.</param>
    private static void RunLifecycleScenario(
        IncrementalHash pcmHash,
        IncrementalHash acknowledgementHash,
        ref int scenarios,
        ref int frames,
        ref int samples)
    {
        AudioUploadAssetDefinition first = AudioAssetCatalogData.Music[3];
        AudioUploadAssetDefinition second = AudioAssetCatalogData.Music[4];
        using var engine = DesktopAccess.CreateAudioEngine();
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

    /// <summary>Renders one command frame and appends its PCM and port acknowledgements in corpus order.</summary>
    /// <param name="scenario">Scenario identifier prefixed to both digest streams.</param>
    /// <param name="engine">Managed SPC audio engine used to render and report acknowledgements.</param>
    /// <param name="commands">Cartridge audio commands submitted for this frame.</param>
    /// <param name="pcmHash">Incremental digest receiving the interleaved PCM bytes.</param>
    /// <param name="acknowledgementHash">Incremental digest receiving one byte per audio port.</param>
    /// <param name="frames">Aggregate frame count incremented once.</param>
    /// <param name="samples">Aggregate PCM sample count incremented by the returned sample length.</param>
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
