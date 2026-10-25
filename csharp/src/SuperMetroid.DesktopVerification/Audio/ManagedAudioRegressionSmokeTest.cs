using SuperMetroid.AssetExtraction.Audio;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using SuperMetroid.Core;
using SuperMetroid.Desktop;
using SuperMetroid.AssetExtraction.Audio.Synthesis;

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
    /// <summary>Digest of the signed stereo PCM emitted by the pinned corpus; re-recorded when key-on began restarting the S-DSP interpolation position and envelope counter (#1305).</summary>
    private const string ExpectedPcmSha256 =
        "A2B82D8FE135588F9CCF562AD2E8497512B12AAF654036DC4500E3E86564E226";
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
            [Library1Sound.UnchargedPowerBeam],
            tally);
        RunSoundScenario(
            "library-2-door",
            [Library2Sound.DudShotOnDoor],
            tally);
        RunSoundScenario(
            "library-3-cinematic",
            [Library3Sound.BabyMetroidCry1],
            tally);
        RunSoundScenario(
            "three-library-overlap-and-cancel",
            [
                Library1Sound.UnchargedPowerBeam,
                Library2Sound.DudShotOnDoor,
                Library3Sound.BabyMetroidCry1,
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
            IReadOnlyList<AudioRequest> commands = frame == 0
                ?
                [
                    new AudioRequest.ChangeMusicDataSet(null),
                    new AudioRequest.ChangeMusicDataSet(bank.DataSet),
                    AudioRequest.ForTrack(new MusicTrack(1)),
                ]
                : Array.Empty<AudioRequest>();
            HashFrame(name, engine, commands, tally);
        }
        tally.Scenarios++;
    }

    /// <summary>Hashes a common-bank sound sequence, optionally issuing cancel commands to all three libraries halfway through.</summary>
    private static void RunSoundScenario(
        string name,
        IReadOnlyList<SoundEffectId> startSounds,
        AudioCorpusTally tally,
        bool cancelHalfway = false)
    {
        var engine = DesktopAccess.CreateAudioSession();
        for (int frame = 0; frame < ShortScenarioFrames; frame++)
        {
            List<AudioRequest> commands = [];
            if (frame == 0)
            {
                commands.Add(new AudioRequest.ChangeMusicDataSet(null));
                commands.AddRange(startSounds.Select(sound => new AudioRequest.RequestSound(sound)));
            }
            if (cancelHalfway && frame == ShortScenarioFrames / 2)
            {
                commands.Add(new AudioRequest.RequestSound(Library1Sound.Silence));
                commands.Add(new AudioRequest.RequestSound(Library2Sound.Silence));
                commands.Add(new AudioRequest.RequestSound(Library3Sound.Silence));
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
            List<AudioRequest> commands = [];
            if (frame == 0)
            {
                commands.Add(new AudioRequest.ChangeMusicDataSet(null));
                commands.Add(new AudioRequest.ChangeMusicDataSet(first.DataSet));
                commands.Add(AudioRequest.ForTrack(new MusicTrack(1)));
            }
            else if (frame == 120)
            {
                commands.Add(new AudioRequest.StopMusic());
            }
            else if (frame == 150)
            {
                commands.Add(new AudioRequest.ChangeMusicDataSet(second.DataSet));
                commands.Add(AudioRequest.ForTrack(new MusicTrack(1)));
            }
            else if (frame == 270)
            {
                // The driver's pause and resume are no game request; write them to the emulated driver directly.
                engine.PlayerForVerification.WriteMusicPort((byte)MusicPortCommand.Pause);
            }
            else if (frame == 330)
            {
                engine.PlayerForVerification.WriteMusicPort((byte)MusicPortCommand.Resume);
            }
            else if (frame == 390)
            {
                commands.Add(AudioRequest.ForTrack(new MusicTrack(2)));
            }
            else if (frame == 510)
            {
                commands.Add(AudioRequest.ForTrack(new MusicTrack(1)));
            }
            HashFrame(
                "music-lifecycle", engine, commands, tally);
        }
        tally.Scenarios++;
    }

    /// <summary>Feeds the scenario name, rendered PCM and four acknowledgement bytes into the corpus digests after one audio update.</summary>
    private static void HashFrame(
        string scenario,
        SynthesizingAudioSession engine,
        IReadOnlyList<AudioRequest> commands,
        AudioCorpusTally tally)
    {
        byte[] name = System.Text.Encoding.UTF8.GetBytes(scenario);
        tally.Pcm.AppendData(name);
        tally.Acknowledgements.AppendData(name);
        ReadOnlySpan<short> pcm = engine.RenderFrame(commands);
        tally.Pcm.AppendData(MemoryMarshal.AsBytes(pcm));
        SoundAcknowledgements<SoundEffectId> acknowledgements = engine.Acknowledgements;
        // The corpus pinned four bytes per update: the sequencer's music echo, which the session never publishes, then
        // each library's acknowledgement in library order.
        Span<byte> reported = stackalloc byte[1 + SoundClockTiming.LibraryCount];
        reported[0] = checked((byte)engine.PlayerForVerification.Music.musicPortToSnes);
        for (int library = 0; library < SoundClockTiming.LibraryCount; library++)
            reported[1 + library] = SoundEffectPortBytes.PortByte(acknowledgements[library]);
        tally.Acknowledgements.AppendData(reported);
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
