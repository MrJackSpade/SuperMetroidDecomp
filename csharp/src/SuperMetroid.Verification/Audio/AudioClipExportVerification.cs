using System.Text.Json;
using SuperMetroid.AssetExtraction.Audio;
using SuperMetroid.TestSupport.RepositoryInstallation;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.Verification.Harness;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using static SuperMetroid.Verification.Harness.VerificationHelpersVerification;
using SuperMetroid.AssetExtraction.Audio.Synthesis;

namespace SuperMetroid.Verification.Audio;

/// <summary>
/// Confirms #1305's clip export (confirmation 1): exported clips equal an independent offline
/// render through <see cref="ManagedSpcAudioBackend"/> from the same entry state, each loop region
/// equals the render's next pass, the clip index stores each sound by its name and only its schema fields, and the clip
/// loader rejects path and format mismatches and a request for an effect its bank context cannot render. The catalog
/// rendered from is the audio's import data (#1693).
/// </summary>
internal static class AudioClipExportVerification
{
    /// <summary>A Green Brinstar track whose tempo phase recurs every pass, so its loop repeats sample-exactly.</summary>
    private const int GreenBrinstarPhaseExactTrack = 3;

    /// <summary>The track's clip length in stereo frames: its intro and one pass of its loop.</summary>
    private const long GreenBrinstarPhaseExactFrames = 1_340_928;

    /// <summary>The stereo frame its loop starts at.</summary>
    private const long GreenBrinstarPhaseExactLoopStart = 677_376;

    /// <summary>Exports the fixture's music clip from the stock audio catalog into a scratch directory and checks it.</summary>
    internal static void VerifyMusicClipExport() => VerifyAudioClipExport(music: true);

    /// <summary>Exports the fixture's effect clips from the stock audio catalog into a scratch directory and checks them and the loader.</summary>
    internal static void VerifyEffectClipExport() => VerifyAudioClipExport(music: false);

    /// <summary>Exports the fixture's music clip or its effect clips into a scratch directory and checks them.</summary>
    private static void VerifyAudioClipExport(bool music)
    {
        ExtractedAudioAssetCatalog catalog = InstalledFixtures.Audio;
        DirectoryInfo scratch = Directory.CreateTempSubdirectory("sm-audio-clips-");
        try
        {
            SoundEffectId[] effects =
            [
                SamusSound.UnchargedPowerBeam,
                EnemySound.EtecoonTheme,
                RoomSound.SpeedBooster,
            ];
            AudioClipExporter.Export(
                catalog,
                scratch.FullName,
                music ? [new AudioClipExporter.MusicKey(AudioBank.GreenBrinstar, GreenBrinstarPhaseExactTrack)] : [],
                music ? [] : [
                    .. effects.Select(effect => new AudioClipExporter.SoundEffectKey(AudioBank.GreenBrinstar, effect.Library, SoundEffectCommands.Command(effect))),
                    new AudioClipExporter.SoundEffectKey(AudioBank.EmptyCrateria, SoundEffectLibrary.Enemies, SoundEffectCommands.Command(EnemySound.EtecoonTheme)),
                ]);
            InstalledClipAudioLibrary clips = InstalledAudioClips.Load(scratch.FullName);
            if (music)
            {
                VerifyMusic(catalog, clips.Clips, GreenBrinstarPhaseExactTrack);
                MusicClipEntry exported = clips.Clips.GetMusic(MusicDataSet.GreenBrinstar, MusicTrackNumbers.Decode(MusicDataSet.GreenBrinstar, GreenBrinstarPhaseExactTrack));
                AssertEqual(GreenBrinstarPhaseExactFrames, exported.Clip.Frames, "the loop check renders the exported clip's length");
                AssertEqual(GreenBrinstarPhaseExactLoopStart, exported.LoopStart, "the loop check starts at the exported loop");
                VerifyLoaderRejections(scratch.FullName, clips);
                Console.WriteLine("  Music clip export: Green Brinstar track 3 equals an independent render; the loader rejects an escaping path and another format.");
                return;
            }
            VerifyStoredSoundEffectFields(Path.Combine(scratch.FullName, InstalledAudioClips.ManifestFileName));
            foreach (SoundEffectId effect in effects)
                VerifySoundEffect(clips, effect);
            AssertThrows<InvalidDataException>(
                () => clips.Clips.GetSoundEffect(MusicDataSet.EmptyCrateria, EnemySound.EtecoonTheme),
                "an effect whose sample the bank context lacks fails loudly");
            Console.WriteLine(
                "  Effect clip export: three effects equal an independent render; an effect its bank context cannot render fails loudly.");
        }
        finally
        {
            scratch.Delete(recursive: true);
        }
    }

    /// <summary>
    /// A Green Brinstar track whose tempo phase recurs every pass: its clip against the independent render, and its loop
    /// against the render's next pass.
    /// </summary>
    private static void VerifyMusic(ExtractedAudioAssetCatalog catalog, ClipAudioLibrary clips, int track)
    {
        MusicClipEntry entry = clips.GetMusic(MusicDataSet.GreenBrinstar, MusicTrackNumbers.Decode(MusicDataSet.GreenBrinstar, track));
        _ = entry.LoopStart ?? throw new InvalidOperationException($"Green Brinstar track {track} loops.");
        short[] clip = clips.ReadClip(entry.Clip);
        long frames = entry.Clip.Frames;
        short[] independent = RenderTrack(catalog, track, frames);
        AssertSequenceEqual(clip, independent, $"track {track} clip equals the independent render");
        AssertEqual(0L, entry.TempoSegments[0].Frame, "the tempo timeline starts at the clip's first frame");
    }

    /// <summary>
    /// A track whose tempo phase recurs every pass loops sample-exactly: an independent render of it carries on past its
    /// clip's end with its loop's first pass again. The clip's length and loop start are the export's, which the music
    /// export check confirms equal the same render.
    /// </summary>
    internal static void VerifyMusicLoopRepeatsExactly()
    {
        ExtractedAudioAssetCatalog catalog = InstalledFixtures.Audio;
        (long frames, long loopStart) = (GreenBrinstarPhaseExactFrames, GreenBrinstarPhaseExactLoopStart);
        short[] independent = RenderTrack(catalog, GreenBrinstarPhaseExactTrack, frames + (frames - loopStart));
        AssertSequenceEqual(independent[(int)(loopStart * 2)..(int)(frames * 2)], independent[(int)(frames * 2)..],
            $"track {GreenBrinstarPhaseExactTrack} loop equals the next pass");
        Console.WriteLine($"  Music loop: Green Brinstar track {GreenBrinstarPhaseExactTrack}'s phase-exact loop equals the next pass.");
    }

    /// <summary>An independent render of Green Brinstar track <paramref name="track"/>'s first <paramref name="frames"/> stereo frames.</summary>
    private static short[] RenderTrack(ExtractedAudioAssetCatalog catalog, int track, long frames) =>
        RenderIndependently(
            catalog,
            MusicDataSet.GreenBrinstar,
            AudioRequest.ForTrack(MusicTrackNumbers.Decode(MusicDataSet.GreenBrinstar, track)),
            player => player.Music.musicPortToSnes == track,
            byte.MaxValue,
            frames);

    /// <summary>An effect's clip entry ends exactly when it stops sounding, unless it repeats forever.</summary>
    private static void VerifySoundEffect(InstalledClipAudioLibrary clips, SoundEffectId effect)
    {
        InstalledSoundEffectClip entry = clips.Manifest.SoundEffects.Single(candidate =>
            candidate.Context == MusicDataSet.GreenBrinstar && candidate.Sound == effect);
        AssertEqual(effect == RoomSound.SpeedBooster, entry.ActiveFrames is null, $"{effect} never ends exactly when it repeats forever");
    }

    /// <summary>An escaping clip path and a wrong manifest format version are each rejected.</summary>
    private static void VerifyLoaderRejections(string directory, InstalledClipAudioLibrary clips)
    {
        string manifestPath = Path.Combine(directory, InstalledAudioClips.ManifestFileName);
        string manifestText = File.ReadAllText(manifestPath);

        AudioClipFile first = clips.Manifest.Music[0].Clip;
        AudioClipManifest escaping = clips.Manifest with
        {
            Music = [clips.Manifest.Music[0] with { Clip = first with { Path = "../escaped.wav" } }],
        };
        File.WriteAllText(manifestPath, JsonSerializer.Serialize(escaping, InstalledAudioClips.ManifestOptions));
        AssertThrows<InvalidDataException>(() => InstalledAudioClips.Load(directory), "a clip path outside the audio root is rejected");

        File.WriteAllText(manifestPath, JsonSerializer.Serialize(
            clips.Manifest with { FormatVersion = AudioClipManifest.CurrentFormatVersion + 1 }, InstalledAudioClips.ManifestOptions));
        AssertThrows<InvalidDataException>(() => InstalledAudioClips.Load(directory), "another manifest format is rejected");
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
        MusicDataSet context,
        AudioRequest request,
        Func<ManagedSpcPlayer, bool> started,
        byte voices,
        long frames)
    {
        var events = new AudioEventSource(new SoundAcceptanceClock<SoundEffectLibrary, SoundEffectId>(SoundEffectLibraries.InQueueOrder, SoundEffectLibrary.Samus));
        var backend = new ManagedSpcAudioBackend(catalog);
        backend.Player.dsp.OutputVoices = voices;
        var observer = new StartObserver(backend.Player, started);
        backend.Player.Music.observer = observer;
        short[] frame = new short[ManagedSnesDsp.NativeStereoFramesPerVideoFrame * 2];
        var pcm = new List<short>();
        IReadOnlyList<AudioRequest>[] script =
        [
            [new AudioRequest.ChangeMusicDataSet(null), new AudioRequest.ChangeMusicDataSet(context)],
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
        public void TickStarting(SoundClockStep<SoundEffectLibrary, byte> tick) =>
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

    /// <summary>
    /// Confirms the index stores itself, each sound effect, layer, clip and unrenderable effect as exactly its schema
    /// fields: the game's views the records convert to are not written beside them (a stored <c>layer</c> copy changed
    /// the receipt in #1503), and no source catalog identity, clip digest, request number or instrument source is
    /// written (#1693). Each sound is stored by its name.
    /// </summary>
    private static void VerifyStoredSoundEffectFields(string manifestPath)
    {
        using JsonDocument manifest = JsonDocument.Parse(File.ReadAllBytes(manifestPath));
        string[] indexFields = ["formatVersion", "sampleRate", "initialBeat", "music", "soundEffects", "unrenderable"];
        string[] effectFields = ["context", "sound", "policy", "activeFrames", "layers"];
        // A layer no longer records the voices it was rendered from: only layer replacement read them (#1533).
        string[] layerFields = ["clip", "loopStart", "noteStarts"];
        string[] clipFields = ["path", "frames"];
        string[] unrenderableFields = ["context", "sound"];
        AssertTrue(manifest.RootElement.EnumerateObject().Select(field => field.Name).SequenceEqual(indexFields),
            "the stored index holds exactly its schema fields");
        foreach (JsonElement effect in manifest.RootElement.GetProperty("soundEffects").EnumerateArray())
        {
            AssertTrue(effect.EnumerateObject().Select(field => field.Name).SequenceEqual(effectFields),
                "a stored sound effect holds exactly its schema fields");
            foreach (JsonElement layer in effect.GetProperty("layers").EnumerateArray())
            {
                AssertTrue(layer.EnumerateObject().Select(field => field.Name).SequenceEqual(layerFields),
                    "a stored sound-effect layer holds exactly its schema fields");
                AssertTrue(layer.GetProperty("clip").EnumerateObject().Select(field => field.Name).SequenceEqual(clipFields),
                    "a stored clip holds exactly its path and length");
            }
        }
        JsonElement[] unrenderable = [.. manifest.RootElement.GetProperty("unrenderable").EnumerateArray()];
        AssertEqual(1, unrenderable.Length, "the Etecoons' theme is the one effect its Empty Crateria context cannot render");
        AssertTrue(unrenderable[0].EnumerateObject().Select(field => field.Name).SequenceEqual(unrenderableFields),
            "a stored unrenderable effect holds exactly its schema fields");
        AssertEqual("EnemySound.EtecoonTheme", unrenderable[0].GetProperty("sound").GetString(), "an unrenderable effect is stored by its sound's name");
        AssertTrue(manifest.RootElement.GetProperty("soundEffects").EnumerateArray()
                .Any(effect => effect.GetProperty("sound").GetString() == "SamusSound.UnchargedPowerBeam"),
            "a sound effect is stored by its sound's name");
    }

    /// <summary>Runs the clip-export confirmation over the repository installation's audio catalog.</summary>
    [VerificationCommand("--music-clip-export")]
    internal static int MusicClipExportCommand()
    {
        Suite(nameof(VerifyMusicClipExport), () => VerifyMusicClipExport());
        return 0;
    }

    /// <summary>Runs the music loop confirmation over the repository installation's audio catalog.</summary>
    [VerificationCommand("--music-loop-repeats-exactly")]
    internal static int MusicLoopRepeatsExactlyCommand()
    {
        Suite(nameof(VerifyMusicLoopRepeatsExactly), () => VerifyMusicLoopRepeatsExactly());
        return 0;
    }

    /// <summary>Runs the effect clip-export confirmation over the repository installation's audio catalog.</summary>
    [VerificationCommand("--effect-clip-export")]
    internal static int EffectClipExportCommand()
    {
        Suite(nameof(VerifyEffectClipExport), () => VerifyEffectClipExport());
        return 0;
    }
}
