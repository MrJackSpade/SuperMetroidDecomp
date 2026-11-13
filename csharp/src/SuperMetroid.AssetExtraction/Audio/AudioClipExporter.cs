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
/// bank, and every sound effect in every bank context, written as content-addressed 16-bit stereo
/// WAV files at the native S-DSP rate with an <see cref="AudioClipManifest"/>. Tracks and effects
/// render in parallel; an effect renders once per distinct start, and a bank context whose start
/// agrees on every input an earlier context's render read takes that render (#1503). Identical
/// PCM is written once.
/// </summary>
internal static class AudioClipExporter
{
    /// <summary>Directory of every clip file beneath the audio root, which holds it beside the clip index alone.</summary>
    internal const string ClipDirectory = "clips";

    /// <summary>Directory of music clip files beneath the audio root.</summary>
    internal const string MusicDirectory = ClipDirectory + "/music";

    /// <summary>Directory of sound-effect clip files beneath the audio root.</summary>
    internal const string SoundEffectDirectory = ClipDirectory + "/sfx";

    /// <summary>One music track to export.</summary>
    /// <param name="Bank">The track's bank.</param>
    /// <param name="Track">The track number.</param>
    internal sealed record MusicKey(AudioBank Bank, int Track);

    /// <summary>One sound effect to export in one bank context.</summary>
    /// <param name="Context">The music bank uploaded after the common upload, or null for the common upload alone.</param>
    /// <param name="Library">The effect's library.</param>
    /// <param name="Command">The effect's request byte.</param>
    internal sealed record SoundEffectKey(AudioBank? Context, SoundEffectLibrary Library, int Command)
    {
        /// <summary>The effect's request byte, 0 through 255.</summary>
        public int Command { get; init; } = Ensure.BetweenInclusive(0, byte.MaxValue, Command);
    }

    /// <summary>
    /// Renders every track and every effect in every bank context of the catalog in <paramref name="importDataDirectory"/>
    /// into <paramref name="audioDirectory"/>.
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

    /// <summary>Every effect of every library in the common context and after each music bank's upload.</summary>
    internal static IReadOnlyList<SoundEffectKey> AllSoundEffects(ExtractedAudioAssetCatalog catalog)
    {
        var contexts = new List<AudioBank?> { null };
        contexts.AddRange(AudioAssetCatalogDefinitions.Music.Select(bank => (AudioBank?)bank.AudioSet));
        return [.. from library in catalog.SoundLibraries
                   from effect in library.Effects
                   from context in contexts
                   select new SoundEffectKey(context, SoundLibraryNumbers.Decode(library.Library), effect.Command)];
    }

    /// <summary>Renders the given tracks and effects into <paramref name="audioDirectory"/> and writes the manifest.</summary>
    internal static AudioClipManifest Export(
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

        // Every effect's first context renders first, then all its other contexts at once, each taking a render it
        // reproduces exactly (#1503); no effect waits on more than two renders in turn.
        var outcomes = new object[effects.Count];
        IGrouping<SoundEffectId, int>[] effectContexts =
            [.. Enumerable.Range(0, effects.Count).GroupBy(index => SoundEffectCommands.Decode(effects[index].Library, effects[index].Command))];
        Dictionary<SoundEffectId, List<SoundEffectRenderShare>> shares =
            effectContexts.ToDictionary(contexts => contexts.Key, _ => new List<SoundEffectRenderShare>());
        Parallel.ForEach(effectContexts, contexts =>
            outcomes[contexts.First()] = ExportSoundEffect(catalog, root, files, scans, effects[contexts.First()], shares[contexts.Key]));
        int[] others = [.. effectContexts.SelectMany(contexts => contexts.Skip(1))];
        Parallel.ForEach(others, index => outcomes[index] = ExportSoundEffect(catalog, root, files, scans, effects[index],
            shares[SoundEffectCommands.Decode(effects[index].Library, effects[index].Command)]));

        var manifest = new AudioClipManifest(
            AudioClipManifest.CurrentFormatVersion,
            SpcAudioClipRenderer.NativeSampleRate,
            music,
            [.. outcomes.OfType<InstalledSoundEffectClip>()],
            [.. outcomes.OfType<UnrenderableSoundEffect>()]);
        File.WriteAllText(
            Path.Combine(root, InstalledAudioClips.ManifestFileName),
            JsonSerializer.Serialize(manifest, InstalledAudioClips.ManifestOptions));
        return manifest;
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
                segments.Add(new SongTempo(sample - start, tempo));
        }
        return new MusicClipEntry(
            AudioAssetCatalogDefinitions.Get(key.Bank).DataSet
                ?? throw new InvalidDataException($"{key.Bank} holds no music data set, so it has no tracks to export."),
            MusicTrackNumbers.Decode(key.Track),
            Write(root, MusicDirectory, files, pcm),
            track.Shape == MusicTrackShape.Loops ? track.Arrivals[0] - start : null,
            segments);
    }

    /// <summary>
    /// Renders one effect in one context, or records why it has no clip there. A render in <paramref name="shares"/>, from
    /// another context of the same effect, that this context's start reproduces exactly is taken instead of rendering;
    /// other contexts of the effect may use <paramref name="shares"/> at the same time.
    /// </summary>
    private static object ExportSoundEffect(
        ExtractedAudioAssetCatalog catalog,
        string root,
        ConcurrentDictionary<string, Lazy<AudioClipFile>> files,
        Dictionary<string, SoundEffectProgramScan> scans,
        SoundEffectKey key,
        List<SoundEffectRenderShare> shares)
    {
        int libraryIndex = SoundEffectLibraries.ToQueueIndex(key.Library);
        AudioSoundEffectMetadata effect = catalog.SoundLibraries
            .Single(library => library.Library == SoundLibraryNumbers.Number(key.Library))
            .Effects.Single(candidate => candidate.Command == key.Command);
        AudioBank bank = key.Context ?? AudioAssetCatalogDefinitions.Common.AudioSet;
        MusicDataSet? context = key.Context is AudioBank loaded ? AudioAssetCatalogDefinitions.Get(loaded).DataSet : null;
        // The index records only that the clip set lacks a sample the effect plays: which instrument and sample number
        // the driver would have looked up is the catalog's numbering, which the installed clips do not carry (#1693).
        if (effect.ChannelPrograms.Any(program =>
                scans[program].UnmappedSources(catalog.GetInstrumentBank(bank), catalog.GetSampleBank(bank)).Any()))
            return new UnrenderableSoundEffect(context, SoundEffectCommands.Decode(key.Library, key.Command));

        SoundEffectClipPolicy policy = Policy(libraryIndex, key.Command, effect.Configuration);
        bool loops = effect.ChannelPrograms.Any(program => scans[program].RepeatsForever);
        SoundEffectRenderStarts starts = SoundEffectRendering.Starts(catalog, key.Context, key.Library, key.Command, loops);
        SoundEffectRenderShare? share;
        lock (shares)
            share = shares.Find(candidate => candidate.Reproduces(starts));
        if (share is null)
        {
            var reads = new SoundEffectRenderReads();
            object rendering = loops
                ? SoundEffectRendering.RenderLoopingLayers(catalog, key.Context, key.Library, key.Command, effect.ChannelPrograms.Count, reads)
                : SoundEffectRendering.RenderOneShot(catalog, key.Context, key.Library, key.Command, reads);
            share = new SoundEffectRenderShare(starts, reads, rendering);
            lock (shares)
                shares.Add(share);
        }
        if (share.Rendering is IReadOnlyList<SoundEffectRendering.LoopingLayer> layers)
        {
            return new InstalledSoundEffectClip(context, SoundEffectCommands.Decode(key.Library, key.Command), policy, null,
                [.. layers.Select(layer => new InstalledSoundEffectLayer(
                    Write(root, SoundEffectDirectory, files, layer.Clip), layer.LoopStart, layer.KeyOnFrames))]);
        }
        var shot = (SoundEffectRendering.OneShot)share.Rendering;
        // A silent effect still occupies its library until its logical end, so it keeps an entry without layers.
        return new InstalledSoundEffectClip(context, SoundEffectCommands.Decode(key.Library, key.Command), policy, shot.ActiveFrames,
            shot.Clip.Length == 0
                ? []
                : [new InstalledSoundEffectLayer(Write(root, SoundEffectDirectory, files, shot.Clip), null, shot.KeyOnFrames)]);
    }

    /// <summary>
    /// The request's acceptance and start policy, read from the driver's own rules: applying its
    /// configuration from each possible priority and low-health mode shows whether it sets them or
    /// keeps them, and the rejection rule shows which states it ignores.
    /// </summary>
    internal static SoundEffectClipPolicy Policy(int libraryIndex, int command, int configuration)
    {
        ManagedSpcSoundLibrary Configure(int priority, int mode)
        {
            var library = new ManagedSpcSoundLibrary { Priority = priority, Mode = mode };
            SpcSoundLibraries.ConfigureSoundLibrary(libraryIndex, library, configuration);
            return library;
        }

        ManagedSpcSoundLibrary fromLow = Configure(0, 0);
        ManagedSpcSoundLibrary fromHigh = Configure(1, SpcDriverDefinitions.SoundEffects.LowHealthMode);
        SoundEffectPriorityEffect priority = new LowHealthPriorityPair(fromLow.Priority, fromHigh.Priority) switch
        {
            (0, 0) => SoundEffectPriorityEffect.Low,
            (1, 1) => SoundEffectPriorityEffect.High,
            (0, 1) => SoundEffectPriorityEffect.Unchanged,
            _ => throw new InvalidDataException($"Configuration {configuration} sets priority {fromLow.Priority}/{fromHigh.Priority}."),
        };
        LowHealthModeEffect mode = new LowHealthPriorityPair(fromLow.Mode, fromHigh.Mode) switch
        {
            (0, SpcDriverDefinitions.SoundEffects.LowHealthMode) => LowHealthModeEffect.Unchanged,
            (0, 0) => LowHealthModeEffect.Clear,
            (SpcDriverDefinitions.SoundEffects.LowHealthMode, SpcDriverDefinitions.SoundEffects.LowHealthMode) => LowHealthModeEffect.Enter,
            _ => throw new InvalidDataException($"Configuration {configuration} sets low-health mode {fromLow.Mode}/{fromHigh.Mode}."),
        };
        if (fromLow.VoicesToSetup != fromHigh.VoicesToSetup)
            throw new InvalidDataException($"Configuration {configuration} allocates voices by state.");
        return new SoundEffectClipPolicy(
            fromLow.VoicesToSetup,
            priority,
            mode,
            IgnoresPriority: !SpcSoundLibraries.RejectsRequest(libraryIndex, command, priority: 1, mode: 0),
            IgnoresLowHealthMode: libraryIndex == 2 && !SpcSoundLibraries.RejectsRequest(libraryIndex, command, priority: 0, mode: SpcDriverDefinitions.SoundEffects.LowHealthMode));
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
