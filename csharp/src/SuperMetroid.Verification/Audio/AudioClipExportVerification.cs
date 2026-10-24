using System.Text.Json;
using SuperMetroid.AssetExtraction.Audio;
using SuperMetroid.TestSupport.RepositoryInstallation;
using SuperMetroid.Verification.Harness;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using static SuperMetroid.Verification.Harness.VerificationHelpersVerification;
using SuperMetroid.AssetExtraction.Audio.Synthesis;

namespace SuperMetroid.Verification.Audio;

/// <summary>
/// Confirms #1305's clip export (confirmation 1): exported clips equal an independent offline
/// render through <see cref="ManagedSpcAudioBackend"/> from the same entry state, each loop region
/// equals the render's next pass, and the clip loader rejects hash, path and format mismatches
/// and a request for an effect its bank context cannot render.
/// </summary>
internal static class AudioClipExportVerification
{
    /// <summary>The Green Brinstar room track. Its tempo phase drifts by a sub-tick per pass (accepted difference (2d)), so it recurs only after nine passes.</summary>
    private const int GreenBrinstarRoomTrack = 5;

    /// <summary>A Green Brinstar track whose tempo phase recurs every pass, so its loop repeats sample-exactly.</summary>
    private const int GreenBrinstarPhaseExactTrack = 3;

    /// <summary>Exports the fixture's clips into a scratch directory and checks them.</summary>
    internal static void VerifyAudioClipExport(string audioDirectory)
    {
        ExtractedAudioAssetCatalog catalog = ExtractedAudioAssetCatalog.Load(audioDirectory);
        DirectoryInfo scratch = Directory.CreateTempSubdirectory("sm-audio-clips-");
        try
        {
            SoundEffectId[] effects =
            [
                Library1Sound.UnchargedPowerBeam,
                Library2Sound.EtecoonTheme,
                Library3Sound.SpeedBooster,
            ];
            AudioClipExporter.Export(
                catalog,
                scratch.FullName,
                [
                    new AudioClipExporter.MusicKey(AudioBank.GreenBrinstar, GreenBrinstarRoomTrack),
                    new AudioClipExporter.MusicKey(AudioBank.GreenBrinstar, GreenBrinstarPhaseExactTrack),
                ],
                [
                    .. effects.Select(effect => new AudioClipExporter.SoundEffectKey(AudioBank.GreenBrinstar, effect.Library, checked((byte)effect.Value))),
                    new AudioClipExporter.SoundEffectKey(AudioBank.EmptyCrateria, SoundEffectLibrary.Library2, checked((byte)Library2Sound.EtecoonTheme.Value)),
                ]);
            ClipAudioCatalog clips = ClipAudioCatalog.Load(scratch.FullName);
            AssertEqual(catalog.ContentIdentity, clips.Manifest.SourceContentIdentity, "clips name the catalog they were rendered from");

            VerifyMusic(catalog, clips, GreenBrinstarRoomTrack, loopRepeatsExactly: false);
            VerifyMusic(catalog, clips, GreenBrinstarPhaseExactTrack, loopRepeatsExactly: true);
            foreach (SoundEffectId effect in effects)
                VerifySoundEffect(catalog, clips, effect);
            AssertThrows<InvalidDataException>(
                () => clips.GetSoundEffect(AudioBank.EmptyCrateria, SoundEffectLibrary.Library2, Library2Sound.EtecoonTheme.Value),
                "an effect whose sample the bank context lacks fails loudly");
            VerifyLoaderRejections(scratch.FullName, clips);
            Console.WriteLine(
                "  Audio clip export: Green Brinstar tracks 5 and 3 and three effects equal an independent render; " +
                "phase-exact loops equal the next pass; the loader rejects hash, path, format and unrenderable requests.");
        }
        finally
        {
            scratch.Delete(recursive: true);
        }
    }

    /// <summary>
    /// A Green Brinstar track's clip against the independent render, and, when its tempo phase
    /// recurs every pass, its loop against the render's next pass.
    /// </summary>
    private static void VerifyMusic(ExtractedAudioAssetCatalog catalog, ClipAudioCatalog clips, int track, bool loopRepeatsExactly)
    {
        MusicClipEntry entry = clips.GetMusic(AudioBank.GreenBrinstar, track);
        long loopStart = entry.LoopStart ?? throw new InvalidOperationException($"Green Brinstar track {track} loops.");
        short[] clip = clips.ReadClip(entry.Clip);
        long frames = entry.Clip.Frames;
        short[] independent = RenderIndependently(
            catalog,
            AudioBank.GreenBrinstar,
            AudioRequest.ForTrack(MusicTrack.NumberedOrNone(unchecked((byte)track))),
            player => player.Music.musicPortToSnes == track,
            byte.MaxValue,
            frames + (frames - loopStart));
        AssertSequenceEqual(clip, independent[..(int)(frames * 2)], $"track {track} clip equals the independent render");
        if (loopRepeatsExactly)
            AssertSequenceEqual(clip[(int)(loopStart * 2)..], independent[(int)(frames * 2)..], $"track {track} loop equals the next pass");
        AssertEqual(0L, entry.TempoSegments[0].Frame, "the tempo timeline starts at the clip's first frame");
    }

    /// <summary>Each layer of an effect against the independent render of its voices, including the next pass of a looping layer.</summary>
    private static void VerifySoundEffect(ExtractedAudioAssetCatalog catalog, ClipAudioCatalog clips, SoundEffectId effect)
    {
        SoundEffectClipEntry entry = clips.GetSoundEffect(AudioBank.GreenBrinstar, effect.Library, effect.Value);
        int libraryIndex = SoundEffectLibraries.ToQueueIndex(effect.Library);
        foreach (SoundEffectClipLayer layer in entry.Layers)
        {
            short[] clip = clips.ReadClip(layer.Clip);
            long frames = layer.Clip.Frames;
            long rendered = layer.LoopStart is long loop ? frames + (frames - loop) : frames;
            short[] independent = RenderIndependently(
                catalog,
                AudioBank.GreenBrinstar,
                AudioRequest.ForSound(effect.Library, effect.Value),
                player => player.soundLibraries[libraryIndex].CurrentSound == effect.Value,
                layer.Voices.ToVoiceMask(),
                rendered);
            AssertSequenceEqual(clip, independent[..(int)(frames * 2)], $"{effect} layer on voices ${layer.Voices.ToVoiceMask():X2} equals the independent render");
            if (layer.LoopStart is long loopStart)
                AssertSequenceEqual(clip[(int)(loopStart * 2)..], independent[(int)(frames * 2)..], $"{effect} loop on voices ${layer.Voices.ToVoiceMask():X2} equals the next pass");
        }
        AssertEqual(effect == Library3Sound.SpeedBooster, entry.ActiveFrames is null, $"{effect} never ends exactly when it repeats forever");
    }

    /// <summary>A tampered clip, an escaping path and a wrong format version are each rejected.</summary>
    private static void VerifyLoaderRejections(string directory, ClipAudioCatalog clips)
    {
        string manifestPath = Path.Combine(directory, ClipAudioCatalog.ManifestFileName);
        string manifestText = File.ReadAllText(manifestPath);

        AudioClipFile first = clips.Manifest.Music[0].Clip;
        string clipPath = Path.Combine(directory, first.Path);
        byte[] original = File.ReadAllBytes(clipPath);
        byte[] tampered = (byte[])original.Clone();
        tampered[^1] ^= 1;
        File.WriteAllBytes(clipPath, tampered);
        AssertThrows<InvalidDataException>(() => clips.ReadClip(first), "a clip whose bytes changed fails its hash");
        File.WriteAllBytes(clipPath, original);

        AudioClipManifest escaping = clips.Manifest with
        {
            Music = [clips.Manifest.Music[0] with { Clip = first with { Path = "../escaped.wav" } }],
        };
        File.WriteAllText(manifestPath, JsonSerializer.Serialize(escaping, ClipAudioCatalog.JsonOptions));
        AssertThrows<InvalidDataException>(() => ClipAudioCatalog.Load(directory), "a clip path outside the audio root is rejected");

        File.WriteAllText(manifestPath, JsonSerializer.Serialize(
            clips.Manifest with { FormatVersion = AudioClipManifest.CurrentFormatVersion + 1 }, ClipAudioCatalog.JsonOptions));
        AssertThrows<InvalidDataException>(() => ClipAudioCatalog.Load(directory), "another manifest format is rejected");
        File.WriteAllText(manifestPath, manifestText);
    }

    /// <summary>
    /// Renders through a fresh event source and <see cref="ManagedSpcAudioBackend"/>: the common
    /// and <paramref name="context"/> uploads on frame 0, <paramref name="request"/> on frame 1,
    /// with only <paramref name="voices"/> in the mix. Returns <paramref name="frames"/> stereo
    /// frames from the tick at which <paramref name="started"/> first holds.
    /// </summary>
    private static short[] RenderIndependently(
        ExtractedAudioAssetCatalog catalog,
        AudioBank context,
        AudioRequest request,
        Func<ManagedSpcPlayer, bool> started,
        byte voices,
        long frames)
    {
        var events = new CartridgeAudioEventSource();
        var backend = new ManagedSpcAudioBackend(catalog);
        backend.Player.dsp.OutputVoices = voices;
        var observer = new StartObserver(backend.Player, started);
        backend.Player.Music.observer = observer;
        short[] frame = new short[ManagedSnesDsp.NativeStereoFramesPerVideoFrame * 2];
        var pcm = new List<short>();
        IReadOnlyList<AudioRequest>[] script =
        [
            [new AudioRequest.ChangeMusicDataSet(AudioAssetCatalogDefinitions.Common.AudioSet), new AudioRequest.ChangeMusicDataSet(context)],
            [request],
        ];
        for (int index = 0; pcm.Count < frames * 2; index++)
        {
            observer.Frame = index;
            IReadOnlyList<AudioRequest> commands = index < script.Length ? script[index] : [];
            backend.RenderNative(new AudioFrameEvents(events.Advance(commands)), frame);
            if (observer.StartSample is long start)
            {
                long frameStart = (long)index * ManagedSnesDsp.NativeStereoFramesPerVideoFrame;
                pcm.AddRange(frame.AsSpan((int)Math.Max(0, start - frameStart) * 2).ToArray());
            }
        }
        return [.. pcm.GetRange(0, (int)(frames * 2))];
    }

    /// <summary>Records the sample of the first tick after which the start condition holds.</summary>
    private sealed class StartObserver(ManagedSpcPlayer player, Func<ManagedSpcPlayer, bool> started) : ISpcDriverObserver
    {
        /// <summary>Sample of the tick being serviced.</summary>
        private long tickSample;

        /// <summary>The frame being rendered.</summary>
        internal int Frame { get; set; }

        /// <summary>The start tick's sample, once seen.</summary>
        internal long? StartSample { get; private set; }

        /// <inheritdoc />
        public void TickStarting(SoundClockStep<SoundEffectId> tick) =>
            tickSample = (long)Frame * ManagedSnesDsp.NativeStereoFramesPerVideoFrame + tick.SampleOffset;

        /// <inheritdoc />
        public void PhraseStarting()
        {
        }

        /// <inheritdoc />
        public void KeysPublished(int keyOn, int keyOff)
        {
            if (StartSample is null && started(player))
                StartSample = tickSample;
        }
    }

    /// <summary>Runs the clip-export confirmation over the repository installation's audio.</summary>
    [VerificationCommand("--audio-clip-export")]
    internal static int AudioClipExportCommand()
    {
        string audioDirectory = RepositoryInstallation.Installation.AudioDirectory;
        Suite(nameof(VerifyAudioClipExport), () => VerifyAudioClipExport(audioDirectory));
        return 0;
    }
}
