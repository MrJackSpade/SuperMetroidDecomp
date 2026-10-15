using System.Runtime.InteropServices;
using System.Security.Cryptography;
using SuperMetroid.Core.Audio;
using SuperMetroid.Core;
using SuperMetroid.Desktop;

namespace SuperMetroid.DesktopVerification.Audio;

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
    /// <summary>Digest of the signed stereo PCM emitted by the pinned corpus.</summary>
    private const string ExpectedPcmSha256 =
        "A147AA83AAA0DB4A7763E7A4EF7813C80FF4F6CBB91967B2E6796BFFC7B12139";
    /// <summary>Digest of all four APU acknowledgement ports after each corpus frame.</summary>
    private const string ExpectedAcknowledgementSha256 =
        "35436A1DFC6846D7A00EBD58CF2F51CB16BCCB197F8B56ACE433CE2F85DAFE99";
    /// <summary>Frames rendered for each music-bank sample and sound-effect scenario.</summary>
    private const int ShortScenarioFrames = 120;
    /// <summary>Longer title-music run that exposes sustained playback drift.</summary>
    private const int TitleScenarioFrames = 600;
    /// <summary>Frames covering stop, bank replacement, pause, resume and cue restoration.</summary>
    private const int LifecycleScenarioFrames = 600;
    /// <summary>Number of unique waveforms expected after deduplicating the installed audio catalog.</summary>
    private const int ExpectedCanonicalSampleCount = 112;
    /// <summary>Number of source-number mappings expected across all installed uploads.</summary>
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
            AudioAssetCatalogDefinitions.Music[0],
            TitleScenarioFrames,
            tally);
        foreach (AudioUploadAssetDefinition bank in AudioAssetCatalogDefinitions.Music)
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
        // Report both together: whether acknowledgements still match separates a sequencing change from a content change.
        bool pcmMatches = pcm.Equals(ExpectedPcmSha256, StringComparison.Ordinal);
        bool acknowledgementsMatch = acknowledgements.Equals(ExpectedAcknowledgementSha256, StringComparison.Ordinal);
        if (!pcmMatches || !acknowledgementsMatch)
        {
            throw new InvalidDataException(
                $"Managed audio regression: PCM {pcm} (expected {ExpectedPcmSha256}, {(pcmMatches ? "matches" : "differs")}); " +
                $"acknowledgements {acknowledgements} (expected {ExpectedAcknowledgementSha256}, {(acknowledgementsMatch ? "matches" : "differs")}).");
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

    /// <summary>Uploads common audio and a music bank on the first frame, then hashes shared track one through the requested duration.</summary>
    private static void RunMusicScenario(
        string name,
        AudioUploadAssetDefinition bank,
        int frameCount,
        AudioCorpusTally tally)
    {
        var engine = DesktopAccess.CreateAudioSession();
        for (int frame = 0; frame < frameCount; frame++)
        {
            IReadOnlyList<CartridgeAudioCommand> commands = frame == 0
                ?
                [
                    CartridgeAudioCommand.Upload(AudioAssetCatalogDefinitions.Common.Bank),
                    CartridgeAudioCommand.Upload(bank.Bank),
                    CartridgeAudioCommand.WritePort(ApuPort.Music, 1),
                ]
                : Array.Empty<CartridgeAudioCommand>();
            HashFrame(name, engine, commands, tally);
        }
        tally.Scenarios++;
    }

    /// <summary>Hashes a common-bank sound sequence, optionally issuing cancel commands to all three libraries halfway through.</summary>
    private static void RunSoundScenario(
        string name,
        IReadOnlyList<(ApuPort Port, byte Command)> startCommands,
        AudioCorpusTally tally,
        bool cancelHalfway = false)
    {
        var engine = DesktopAccess.CreateAudioSession();
        for (int frame = 0; frame < ShortScenarioFrames; frame++)
        {
            List<CartridgeAudioCommand> commands = [];
            if (frame == 0)
            {
                commands.Add(CartridgeAudioCommand.Upload(AudioAssetCatalogDefinitions.Common.Bank));
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

    /// <summary>Exercises music stop, bank replacement, pause/resume and shared-cue selection at fixed frame boundaries.</summary>
    private static void RunLifecycleScenario(
        AudioCorpusTally tally)
    {
        AudioUploadAssetDefinition first = AudioAssetCatalogDefinitions.Music[3];
        AudioUploadAssetDefinition second = AudioAssetCatalogDefinitions.Music[4];
        var engine = DesktopAccess.CreateAudioSession();
        for (int frame = 0; frame < LifecycleScenarioFrames; frame++)
        {
            List<CartridgeAudioCommand> commands = [];
            if (frame == 0)
            {
                commands.Add(CartridgeAudioCommand.Upload(AudioAssetCatalogDefinitions.Common.Bank));
                commands.Add(CartridgeAudioCommand.Upload(first.Bank));
                commands.Add(CartridgeAudioCommand.WritePort(ApuPort.Music, 1));
            }
            else if (frame == 120)
            {
                commands.Add(CartridgeAudioCommand.WritePort(ApuPort.Music, 0));
            }
            else if (frame == 150)
            {
                commands.Add(CartridgeAudioCommand.Upload(second.Bank));
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

    /// <summary>Feeds the scenario name, rendered PCM and four acknowledgement bytes into the corpus digests after one audio update.</summary>
    private static void HashFrame(
        string scenario,
        CartridgeAudioSession engine,
        IReadOnlyList<CartridgeAudioCommand> commands,
        AudioCorpusTally tally)
    {
        byte[] name = System.Text.Encoding.UTF8.GetBytes(scenario);
        tally.Pcm.AppendData(name);
        tally.Acknowledgements.AppendData(name);
        ReadOnlySpan<short> pcm = engine.RenderFrame(commands);
        tally.Pcm.AppendData(MemoryMarshal.AsBytes(pcm));
        CartridgeAudioAcknowledgements acknowledgements = engine.Acknowledgements;
        Span<byte> ports = stackalloc byte[AudioRomData.Apu.PortCount];
        foreach (ApuPort port in Enum.GetValues<ApuPort>())
            ports[(int)port] = acknowledgements[port];
        // The corpus pinned port zero as the sequencer's music echo, which the session never publishes.
        ports[(int)ApuPort.Music] = engine.PlayerForVerification.musicPortToSnes;
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
        /// <summary>Incremental digest of interleaved PCM samples and scenario names.</summary>
        public IncrementalHash Pcm { get; } = IncrementalHash.CreateHash(HashAlgorithmName.SHA256);

        /// <summary>Incremental digest of four-port APU acknowledgements and scenario names.</summary>
        public IncrementalHash Acknowledgements { get; } = IncrementalHash.CreateHash(HashAlgorithmName.SHA256);

        /// <summary>Number of fully rendered scenarios.</summary>
        public int Scenarios { get; set; }

        /// <summary>Number of audio updates included in both digests.</summary>
        public int Frames { get; set; }

        /// <summary>Number of signed 16-bit PCM values, counting left and right channels separately.</summary>
        public int Samples { get; set; }

        /// <summary>Releases both incremental hash accumulators after the corpus has been checked.</summary>
        public void Dispose()
        {
            Pcm.Dispose();
            Acknowledgements.Dispose();
        }
    }
}
