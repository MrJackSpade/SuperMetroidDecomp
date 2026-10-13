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
    private const string ExpectedPcmSha256 =
        "06FE91966B29B05F2012C6A542864B5692B178A7B028C2A4A56F6D34E8F8D06C";
    private const string ExpectedAcknowledgementSha256 =
        "DF414B59F7CA21C4BBAD7ABA9C379C8296DCA454B480BFCEFCB8B35123D851FD";
    private const int ShortScenarioFrames = 120;
    private const int TitleScenarioFrames = 600;
    private const int LifecycleScenarioFrames = 600;
    private const int ExpectedCanonicalSampleCount = 112;
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
        using var tally = new AudioCorpusTally();

        RunMusicScenario(
            "title-long",
            AudioAssetCatalogData.Music[0],
            TitleScenarioFrames,
            tally);
        foreach (AudioUploadAssetDefinition bank in AudioAssetCatalogData.Music)
        {
            RunMusicScenario(
                bank.Name,
                bank,
                ShortScenarioFrames,
                tally);
        }

        RunSoundScenario(
            "library-1-power-beam",
            [(ApuPort.SoundLibrary1, SoundEffectLibrary1Sounds.PowerBeam.Value)],
            tally);
        RunSoundScenario(
            "library-2-door",
            [(ApuPort.SoundLibrary2, SoundEffectLibrary2Sounds.DoorOpening.Value)],
            tally);
        RunSoundScenario(
            "library-3-cinematic",
            [(ApuPort.SoundLibrary3, 0x23)],
            tally);
        RunSoundScenario(
            "three-library-overlap-and-cancel",
            [
                (ApuPort.SoundLibrary1, SoundEffectLibrary1Sounds.PowerBeam.Value),
                (ApuPort.SoundLibrary2, SoundEffectLibrary2Sounds.DoorOpening.Value),
                (ApuPort.SoundLibrary3, 0x23),
            ],
            tally,
            cancelHalfway: true);
        RunLifecycleScenario(
            tally);

        string pcm = Convert.ToHexString(tally.Pcm.GetHashAndReset());
        string acknowledgements = Convert.ToHexString(tally.Acknowledgements.GetHashAndReset());
        if (!pcm.Equals(ExpectedPcmSha256, StringComparison.Ordinal))
            throw new InvalidDataException($"Managed audio PCM regression: {pcm}, expected {ExpectedPcmSha256}.");
        if (!acknowledgements.Equals(ExpectedAcknowledgementSha256, StringComparison.Ordinal))
        {
            throw new InvalidDataException(
                $"Managed audio acknowledgement regression: {acknowledgements}, expected {ExpectedAcknowledgementSha256}.");
        }
        return new ManagedAudioRegressionSmokeTestResult(
            tally.Scenarios,
            tally.Frames,
            tally.Samples,
            catalog.CanonicalSampleCount,
            catalog.SourceMappingCount,
            pcm,
            acknowledgements);
    }

    private static void RunMusicScenario(
        string name,
        AudioUploadAssetDefinition bank,
        int frameCount,
        AudioCorpusTally tally)
    {
        var engine = DesktopAccess.CreateAudioRenderer();
        for (int frame = 0; frame < frameCount; frame++)
        {
            IReadOnlyList<CartridgeAudioCommand> commands = frame == 0
                ?
                [
                    CartridgeAudioCommand.Upload(AudioAssetCatalogData.Common.SnesAddress),
                    CartridgeAudioCommand.Upload(bank.SnesAddress),
                    CartridgeAudioCommand.WritePort(ApuPort.Music, 1),
                ]
                : Array.Empty<CartridgeAudioCommand>();
            HashFrame(name, engine, commands, tally);
        }
        tally.Scenarios++;
    }

    private static void RunSoundScenario(
        string name,
        IReadOnlyList<(ApuPort Port, byte Command)> startCommands,
        AudioCorpusTally tally,
        bool cancelHalfway = false)
    {
        var engine = DesktopAccess.CreateAudioRenderer();
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
                    ApuPort.SoundLibrary1, SoundEffectLibrary1Sounds.CancelAll.Value));
                commands.Add(CartridgeAudioCommand.WritePort(
                    ApuPort.SoundLibrary2, SoundEffectLibrary2Sounds.CancelAll.Value));
                commands.Add(CartridgeAudioCommand.WritePort(
                    ApuPort.SoundLibrary3, SoundEffectLibrary3Sounds.CancelAll.Value));
            }
            HashFrame(name, engine, commands, tally);
        }
        tally.Scenarios++;
    }

    private static void RunLifecycleScenario(
        AudioCorpusTally tally)
    {
        AudioUploadAssetDefinition first = AudioAssetCatalogData.Music[3];
        AudioUploadAssetDefinition second = AudioAssetCatalogData.Music[4];
        var engine = DesktopAccess.CreateAudioRenderer();
        for (int frame = 0; frame < LifecycleScenarioFrames; frame++)
        {
            List<CartridgeAudioCommand> commands = [];
            if (frame == 0)
            {
                commands.Add(CartridgeAudioCommand.Upload(AudioAssetCatalogData.Common.SnesAddress));
                commands.Add(CartridgeAudioCommand.Upload(first.SnesAddress));
                commands.Add(CartridgeAudioCommand.WritePort(ApuPort.Music, 1));
            }
            else if (frame == 120)
            {
                commands.Add(CartridgeAudioCommand.WritePort(ApuPort.Music, 0));
            }
            else if (frame == 150)
            {
                commands.Add(CartridgeAudioCommand.Upload(second.SnesAddress));
                commands.Add(CartridgeAudioCommand.WritePort(ApuPort.Music, 1));
            }
            else if (frame == 270)
            {
                commands.Add(CartridgeAudioCommand.WritePort(
                    ApuPort.Music, AudioRomData.Apu.PauseMusic));
            }
            else if (frame == 330)
            {
                commands.Add(CartridgeAudioCommand.WritePort(
                    ApuPort.Music, AudioRomData.Apu.ResumeMusic));
            }
            else if (frame == 390)
            {
                commands.Add(CartridgeAudioCommand.WritePort(ApuPort.Music, 2));
            }
            else if (frame == 510)
            {
                commands.Add(CartridgeAudioCommand.WritePort(ApuPort.Music, 1));
            }
            HashFrame(
                "music-lifecycle", engine, commands, tally);
        }
        tally.Scenarios++;
    }

    private static void HashFrame(
        string scenario,
        CartridgeAudioRenderer engine,
        IReadOnlyList<CartridgeAudioCommand> commands,
        AudioCorpusTally tally)
    {
        byte[] name = System.Text.Encoding.UTF8.GetBytes(scenario);
        tally.Pcm.AppendData(name);
        tally.Acknowledgements.AppendData(name);
        ReadOnlySpan<short> pcm = engine.RenderFrame(commands);
        tally.Pcm.AppendData(MemoryMarshal.AsBytes(pcm));
        CartridgeAudioAcknowledgements acknowledgements = engine.ReadAcknowledgements();
        Span<byte> ports = stackalloc byte[AudioRomData.Apu.PortCount];
        foreach (ApuPort port in Enum.GetValues<ApuPort>())
            ports[(int)port] = acknowledgements[port];
        tally.Acknowledgements.AppendData(ports);
        tally.Frames++;
        tally.Samples += pcm.Length;
    }

    /// <summary>
    /// The corpus digests and counts every scenario adds to (#633): one PCM and one acknowledgement
    /// SHA-256 stream, and the completed scenario, frame and sample totals.
    /// </summary>
    private sealed class AudioCorpusTally : IDisposable
    {
        public IncrementalHash Pcm { get; } = IncrementalHash.CreateHash(HashAlgorithmName.SHA256);

        public IncrementalHash Acknowledgements { get; } = IncrementalHash.CreateHash(HashAlgorithmName.SHA256);

        public int Scenarios { get; set; }

        public int Frames { get; set; }

        public int Samples { get; set; }

        public void Dispose()
        {
            Pcm.Dispose();
            Acknowledgements.Dispose();
        }
    }
}
