using System.Collections.Concurrent;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text.Json;
using SuperMetroid.Core;
using SuperMetroid.Core.Audio;
using SuperMetroid.AssetExtraction.Audio.Synthesis;

namespace SuperMetroid.AssetExtraction.Audio;

/// <summary>
/// Renders the audio's import data, the extracted catalog, into installed clips (#1305, #1693): every music track in every
/// bank, and every sound effect the game requests once, from the shared sounds' samples alone (#2057), written as
/// content-addressed 16-bit stereo WAV files at the native S-DSP rate with an <see cref="AudioClipManifest"/>. Tracks and
/// effects render in parallel. Identical PCM is written once.
/// </summary>
internal static class AudioClipExporter
{
    /// <summary>Directory of every clip file beneath the audio root, which holds it beside the clip index alone.</summary>
    private const string ClipDirectory = "clips";

    /// <summary>Directory of music clip files beneath the audio root.</summary>
    internal const string MusicDirectory = ClipDirectory + "/music";

    /// <summary>Directory of sound-effect clip files beneath the audio root.</summary>
    internal const string SoundEffectDirectory = ClipDirectory + "/sfx";

    /// <summary>One music track to export.</summary>
    /// <param name="Bank">The track's bank.</param>
    /// <param name="Track">The track number.</param>
    internal sealed record MusicKey(AudioBank Bank, int Track);

    /// <summary>One sound effect to export, rendered from the common upload alone.</summary>
    /// <param name="Library">The effect's library.</param>
    /// <param name="Command">The effect's request byte.</param>
    internal sealed record SoundEffectKey(SoundEffectLibrary Library, int Command)
    {
        /// <summary>The effect's request byte, 0 through 255.</summary>
        public int Command { get; init; } = Ensure.BetweenInclusive(0, byte.MaxValue, Command);
    }

    /// <summary>
    /// Renders every track and every effect of the catalog in <paramref name="importDataDirectory"/> into
    /// <paramref name="audioDirectory"/>.
    /// </summary>
    /// <exception cref="InvalidDataException">A track has no period, or an effect does not render as its program structure requires.</exception>
    internal static void Export(string importDataDirectory, string audioDirectory)
    {
        ExtractedAudioAssetCatalog catalog = ExtractedAudioAssetCatalog.Load(importDataDirectory);
        Export(catalog, audioDirectory, AllTracks(catalog), AllSoundEffects(catalog));
    }

    /// <summary>Every nonzero track-table entry of every music bank.</summary>
    internal static IReadOnlyList<MusicKey> AllTracks(ExtractedAudioAssetCatalog catalog)
    {
        var keys = new List<MusicKey>();
        foreach (AudioUploadAssetDefinition bank in AudioAssetCatalogDefinitions.Music)
        {
            IReadOnlyList<ushort> pointers = catalog.GetMusicBank(bank.AudioSet).TrackPointers;
            for (int track = 1; track < pointers.Count; track++)
            {
                if (pointers[track] != 0)
                    keys.Add(new MusicKey(bank.AudioSet, track));
            }
        }
        return keys;
    }

    /// <summary>
    /// Every effect a sound of the game requests, of every library, once. A library's other effects, which no sound of the
    /// game requests (#1686), stay in the catalog and get no clip.
    /// </summary>
    internal static IReadOnlyList<SoundEffectKey> AllSoundEffects(ExtractedAudioAssetCatalog catalog) =>
        [.. from library in catalog.SoundLibraries
            let sounds = SoundLibraryNumbers.Decode(library.Library)
            from effect in library.Effects
            where SoundEffectCommands.Names(sounds, effect.Command)
            select new SoundEffectKey(sounds, effect.Command)];

    /// <summary>Renders the given tracks and effects into <paramref name="audioDirectory"/> and writes the manifest.</summary>
    private static void Export(
        ExtractedAudioAssetCatalog catalog,
        string audioDirectory,
        IReadOnlyList<MusicKey> tracks,
        IReadOnlyList<SoundEffectKey> effects)
    {
        Ensure.NotNull(catalog);
        string root = Path.GetFullPath(audioDirectory);
        var files = new ConcurrentDictionary<string, Lazy<AudioClipFile>>();
        Dictionary<string, SoundEffectProgramScan> scans = catalog.SoundPrograms.ToDictionary(program => program.Id, SoundEffectProgramScan.Scan);

        var music = new MusicClipEntry[tracks.Count];
        Parallel.For(0, tracks.Count, index => music[index] = ExportTrack(catalog, root, files, tracks[index]));

        var soundEffects = new InstalledSoundEffectClip[effects.Count];
        Parallel.For(0, effects.Count, index => soundEffects[index] = ExportSoundEffect(catalog, root, files, scans, effects[index]));

        var manifest = new AudioClipManifest(
            AudioClipManifest.CurrentFormatVersion,
            SpcAudioClipRenderer.NativeSampleRate,
            // Every song was rendered from a driver started at its initial tempo, as the game starts the music.
            DriverTempoBeats.LengthAt(SpcDriverDefinitions.Music.InitialTempo),
            music,
            soundEffects);
        File.WriteAllText(
            Path.Combine(root, InstalledAudioClips.ManifestFileName),
            JsonSerializer.Serialize(manifest, InstalledAudioClips.ManifestOptions));
    }

    /// <summary>Analyzes and renders one track and records its loop and tempo timeline.</summary>
    private static MusicClipEntry ExportTrack(
        ExtractedAudioAssetCatalog catalog,
        string root,
        ConcurrentDictionary<string, Lazy<AudioClipFile>> files,
        MusicKey key)
    {
        MusicTrackAnalysis track = MusicTrackAnalysis.Analyze(catalog, key.Bank, key.Track);
        short[] pcm = MusicTrackRendering.RenderClip(catalog, track);
        long start = track.StartSample;
        long clipFrames = pcm.Length / 2;
        var segments = new List<SongTempo>();
        foreach ((long sample, int tempo) in track.TempoChanges)
        {
            if (sample - start < clipFrames)
                segments.Add(new SongTempo(sample - start, DriverTempoBeats.LengthAt(tempo)));
        }
        MusicDataSet dataSet = AudioAssetCatalogDefinitions.Get(key.Bank).DataSet
            ?? throw new InvalidDataException($"{key.Bank} holds no music data set, so it has no tracks to export.");
        return new MusicClipEntry(
            dataSet,
            // The data set and its track number decode into the song (#1748).
            MusicTrackNumbers.Decode(dataSet, key.Track),
            Write(root, MusicDirectory, files, pcm),
            track.Shape == MusicTrackShape.Loops ? track.Arrivals[0] - start : null,
            segments);
    }

    /// <summary>
    /// Renders one effect from the common upload's samples alone (#2057): a sound effect sounds the same whichever music
    /// data set is loaded.
    /// </summary>
    /// <exception cref="InvalidDataException">The effect plays a sample the common upload does not hold.</exception>
    private static InstalledSoundEffectClip ExportSoundEffect(
        ExtractedAudioAssetCatalog catalog,
        string root,
        ConcurrentDictionary<string, Lazy<AudioClipFile>> files,
        Dictionary<string, SoundEffectProgramScan> scans,
        SoundEffectKey key)
    {
        AudioSoundEffectMetadata effect = catalog.SoundLibraries
            .Single(library => library.Library == SoundLibraryNumbers.Number(key.Library))
            .Effects.Single(candidate => candidate.Command == key.Command);
        AudioBank bank = AudioAssetCatalogDefinitions.Common.AudioSet;
        SoundEffectId sound = SoundEffectCommands.Decode(key.Library, key.Command);
        if (effect.ChannelPrograms.Any(program =>
                scans[program].UnmappedSources(catalog.GetInstrumentBank(bank), catalog.GetSampleBank(bank)).Any()))
            throw new InvalidDataException($"{sound} plays a sample the shared sounds do not hold.");

        SoundEffectClipPolicy policy = Policy(key.Library, key.Command, effect.Policy);
        bool loops = effect.ChannelPrograms.Any(program => scans[program].RepeatsForever);
        if (loops)
        {
            IReadOnlyList<SoundEffectRendering.LoopingLayer> layers =
                SoundEffectRendering.RenderLoopingLayers(catalog, null, key.Library, key.Command, effect.ChannelPrograms.Count);
            return new InstalledSoundEffectClip(sound, policy, new InstalledLoopingSoundEffect(
                [.. layers.Select(layer => new InstalledLoopingSoundEffectLayer(
                    Write(root, SoundEffectDirectory, files, layer.Clip), layer.LoopStart))]));
        }
        SoundEffectRendering.OneShot shot = SoundEffectRendering.RenderOneShot(catalog, null, key.Library, key.Command);
        // A silent effect still occupies its library until its logical end, so it keeps an entry without layers.
        return new InstalledSoundEffectClip(sound, policy, new InstalledOneShotSoundEffect(shot.ActiveFrames,
            shot.Clip.Length == 0 ? null : Write(root, SoundEffectDirectory, files, shot.Clip)));
    }

    /// <summary>
    /// The request's acceptance and start policy, read from the driver's own rules: applying its
    /// configuration from each possible priority and low-health mode shows whether it sets them or
    /// keeps them, and the rejection rule shows which states it ignores.
    /// </summary>
    internal static SoundEffectClipPolicy Policy(SoundEffectLibrary soundLibrary, int command, string policyKey)
    {
        ManagedSpcSoundLibrary Configure(int priority, SoundLibraryMode mode)
        {
            var library = new ManagedSpcSoundLibrary { Priority = priority, Mode = mode };
            SpcSoundLibraries.ConfigureSoundLibrary(soundLibrary, library, policyKey);
            return library;
        }

        ManagedSpcSoundLibrary fromLow = Configure(0, SoundLibraryMode.Normal);
        ManagedSpcSoundLibrary fromHigh = Configure(1, SoundLibraryMode.LowHealth);
        SoundEffectPriorityEffect priority = new LowHealthPriorityPair(fromLow.Priority, fromHigh.Priority) switch
        {
            (0, 0) => SoundEffectPriorityEffect.Low,
            (1, 1) => SoundEffectPriorityEffect.High,
            (0, 1) => SoundEffectPriorityEffect.Unchanged,
            _ => throw new InvalidDataException($"Policy {policyKey} sets priority {fromLow.Priority}/{fromHigh.Priority}."),
        };
        LowHealthModeEffect mode =
            fromLow.Mode == SoundLibraryMode.Normal && fromHigh.Mode == SoundLibraryMode.LowHealth ? LowHealthModeEffect.Unchanged
            : fromLow.Mode == SoundLibraryMode.Normal && fromHigh.Mode == SoundLibraryMode.Normal ? LowHealthModeEffect.Clear
            : fromLow.Mode == SoundLibraryMode.LowHealth && fromHigh.Mode == SoundLibraryMode.LowHealth ? LowHealthModeEffect.Enter
            : throw new InvalidDataException($"Policy {policyKey} sets low-health mode {fromLow.Mode}/{fromHigh.Mode}.");
        // The voices the configuration sets up stay the driver's: the render plays them, and the clips carry the result
        // (#1749).
        return new SoundEffectClipPolicy(
            priority,
            mode,
            IgnoresPriority: !SpcSoundLibraries.RejectsRequest(soundLibrary, command, priority: 1, SoundLibraryMode.Normal),
            IgnoresLowHealthMode: SpcSoundEffectTables.HasLowHealthMode(soundLibrary) && !SpcSoundLibraries.RejectsRequest(soundLibrary, command, priority: 0, SoundLibraryMode.LowHealth));
    }

    /// <summary>
    /// Writes <paramref name="pcm"/> once, named by its samples' SHA-256 so identical PCM shares one file, and returns its
    /// index record.
    /// </summary>
    private static AudioClipFile Write(string root, string directory, ConcurrentDictionary<string, Lazy<AudioClipFile>> files, short[] pcm)
    {
        string pcmHash = Convert.ToHexString(SHA256.HashData(MemoryMarshal.AsBytes(pcm.AsSpan())));
        string relative = $"{directory}/{pcmHash}.wav";
        return files.GetOrAdd(relative, _ => new Lazy<AudioClipFile>(() =>
        {
            string path = Path.Combine(root, relative);
            Directory.CreateDirectory(Path.GetDirectoryName(path) ?? throw new InvalidOperationException($"Clip path '{path}' has no directory."));
            PcmWaveFile.WriteStereoPcm16(path, SpcAudioClipRenderer.NativeSampleRate, pcm);
            return new AudioClipFile(relative, pcm.Length / 2);
        })).Value;
    }

}
