using System.Collections.Concurrent;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text.Json;
using SuperMetroid.Core;
using SuperMetroid.Core.Audio;
using SuperMetroid.AssetExtraction.Audio.Synthesis;
using SuperMetroid.Core.World.RoomStates;

namespace SuperMetroid.AssetExtraction.Audio;

/// <summary>
/// Renders the audio's import data, the extracted catalog, into installed clips (#1305, #1693): every music track in every
/// bank, and every sound effect the game requests once, from the shared sounds' samples alone (#2057) or, for a sound that
/// plays an instrument only a music data set defines, with the data set its room loads (#2603), written as
/// content-addressed 16-bit stereo WAV files at the native S-DSP rate with an <see cref="AudioClipManifest"/>. Tracks and
/// effects render in parallel. Identical PCM is written once.
/// </summary>
internal static partial class AudioClipExporter
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
    /// <param name="DataSet">The music data set the bank holds.</param>
    internal sealed partial record MusicKey(AudioBank Bank, MusicDataSet DataSet, int Track);

    /// <summary>One sound effect to export, rendered once in its <see cref="RenderContext"/>.</summary>
    /// <param name="Library">The effect's library.</param>
    /// <param name="Command">The effect's request byte.</param>
    internal sealed partial record SoundEffectKey(SoundEffectLibrary Library, int Command)
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
            MusicDataSet dataSet = Enum.GetValues<MusicDataSet>().Single(set => AudioAssetCatalogDefinitions.Get(set) == bank);
            for (int track = 1; track < pointers.Count; track++)
            {
                if (pointers[track] != 0)
                    keys.Add(new MusicKey(bank.AudioSet, dataSet, track));
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
    internal static void Export(
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
        InstallationWork.For(0, tracks.Count, index => music[index] = ExportTrack(catalog, root, files, tracks[index]));

        var soundEffects = new InstalledSoundEffectClip[effects.Count];
        InstallationWork.For(0, effects.Count, index => soundEffects[index] = ExportSoundEffect(catalog, root, files, scans, effects[index]));

        var manifest = new AudioClipManifest(
            AudioClipManifest.CurrentFormatVersion,
            SpcAudioClipRenderer.NativeSampleRate,
            // Every song was rendered from a driver started at its initial tempo, as the game starts the music.
            DriverTempoBeats.LengthAt(SpcDriverDefinitions.Music.InitialTempo),
            music,
            soundEffects);
        File.WriteAllText(
            Path.Combine(root, InstalledAudioClips.ManifestFileName),
            JsonSerializer.Serialize(manifest, global::SuperMetroid.AssetExtraction.AssetExtractionJson.Info<global::SuperMetroid.AssetExtraction.Audio.AudioClipManifest>(InstalledAudioClips.ManifestOptions)));
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
        MusicDataSet dataSet = key.DataSet;
        return new MusicClipEntry(
            dataSet,
            // The data set and its track number decode into the song (#1748).
            MusicTrackNumbers.Decode(dataSet, key.Track),
            Write(root, MusicDirectory, files, pcm),
            track.Period is MusicTrackPeriod.Loop loop ? loop.FirstArrival - start : null,
            segments);
    }

    /// <summary>
    /// The music bank loaded when the game plays <paramref name="sound"/>, which its clip renders with: the data set of the
    /// room state <see cref="SoundEffectRoomStateDefinitions"/> names for a sound that plays an instrument only a music data
    /// set defines (#2603), or null for every other sound, which renders from the shared sounds alone (#2057).
    /// </summary>
    /// <exception cref="InvalidDataException">The sound's room state keeps the data set loaded before it.</exception>
    internal static AudioBank? RenderContext(SoundEffectId sound) =>
        SoundEffectRoomStateDefinitions.PlayedIn.TryGetValue(sound, out RoomStateSettings? room)
            ? AudioAssetCatalogDefinitions.BankOf(room.Music.DataSet
                ?? throw new InvalidDataException($"{sound} plays in {room.Name}, which loads no music data set of its own."))
            : null;

    /// <summary>
    /// The instruments <paramref name="effect"/>'s programs key that <paramref name="context"/>, or the shared sounds for
    /// none, cannot play (<see cref="SoundEffectProgramScan.UnplayableInstruments"/>).
    /// </summary>
    internal static string[] UnplayableInstruments(
        ExtractedAudioAssetCatalog catalog,
        IReadOnlyDictionary<string, SoundEffectProgramScan> scans,
        AudioSoundEffectMetadata effect,
        AudioBank? context)
    {
        Ensure.NotNull(catalog);
        Ensure.NotNull(effect);
        AudioBank bank = context ?? AudioAssetCatalogDefinitions.Common.AudioSet;
        return [.. effect.ChannelPrograms
            .SelectMany(program => scans[program].UnplayableInstruments(catalog.GetInstrumentBank(bank), catalog.GetSampleBank(bank)))
            .Distinct()];
    }

    /// <summary>
    /// Renders one effect once, in its <see cref="RenderContext"/>: the shared sounds alone (#2057), or the music data set
    /// the one room state that plays it loads (#2603).
    /// </summary>
    /// <exception cref="InvalidDataException">The effect plays an instrument its render context cannot play.</exception>
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
        SoundEffectId sound = SoundEffectCommands.Decode(key.Library, key.Command);
        AudioBank? context = RenderContext(sound);
        string[] unplayable = UnplayableInstruments(catalog, scans, effect, context);
        if (unplayable.Length != 0)
        {
            throw new InvalidDataException(
                $"{sound} plays what {context?.ToString() ?? "the shared sounds"} cannot play: {string.Join("; ", unplayable)}.");
        }

        SoundEffectClipPolicy policy = Policy(key.Library, key.Command, effect.Policy);
        InstalledSoundEffectShape shape = RenderShape(catalog, root, files, scans, effect, key, context);
        return new InstalledSoundEffectClip(sound, policy, shape)
        {
            ByDataSet = [.. DataSetShapes(catalog, root, files, scans, effect, key, context, shape)],
        };
    }

    /// <summary>Renders one effect's shape with <paramref name="context"/>'s bank loaded after the shared sounds, or with them alone.</summary>
    private static InstalledSoundEffectShape RenderShape(
        ExtractedAudioAssetCatalog catalog,
        string root,
        ConcurrentDictionary<string, Lazy<AudioClipFile>> files,
        Dictionary<string, SoundEffectProgramScan> scans,
        AudioSoundEffectMetadata effect,
        SoundEffectKey key,
        AudioBank? context)
    {
        bool loops = effect.ChannelPrograms.Any(program => scans[program].RepeatsForever);
        if (loops)
        {
            IReadOnlyList<SoundEffectRendering.LoopingLayer> layers =
                SoundEffectRendering.RenderLoopingLayers(catalog, context, key.Library, key.Command, effect.ChannelPrograms.Count);
            return new InstalledLoopingSoundEffect(
                [.. layers.Select(layer => new InstalledLoopingSoundEffectLayer(
                    Write(root, SoundEffectDirectory, files, layer.Clip), layer.LoopStart))]);
        }
        SoundEffectRendering.OneShot shot = SoundEffectRendering.RenderOneShot(catalog, context, key.Library, key.Command);
        // A silent effect still occupies its library until its logical end, so it keeps an entry without layers.
        return new InstalledOneShotSoundEffect(shot.ActiveFrames,
            shot.Clip.Length == 0 ? null : Write(root, SoundEffectDirectory, files, shot.Clip));
    }

    /// <summary>
    /// The effect's shape in each music data set that plays it differently from <paramref name="shape"/>, its render in
    /// <paramref name="context"/> (#2645, #2648, #2650, #2652): each data set redefines instruments past the shared sounds',
    /// and the game plays a sound with the instruments the loaded set holds, so a set that changes an instrument record the
    /// effect keys, or the sample it plays, plays the effect with its own. Each such set's render is kept where it differs.
    /// </summary>
    private static IEnumerable<InstalledSoundEffectSetShape> DataSetShapes(
        ExtractedAudioAssetCatalog catalog,
        string root,
        ConcurrentDictionary<string, Lazy<AudioClipFile>> files,
        Dictionary<string, SoundEffectProgramScan> scans,
        AudioSoundEffectMetadata effect,
        SoundEffectKey key,
        AudioBank? context,
        InstalledSoundEffectShape shape)
    {
        AudioBank home = context ?? AudioAssetCatalogDefinitions.Common.AudioSet;
        int[] records = [.. effect.ChannelPrograms.SelectMany(program => scans[program].Records()).Distinct()];
        var byBank = new Dictionary<AudioBank, InstalledSoundEffectShape?>();
        foreach (MusicDataSet set in Enum.GetValues<MusicDataSet>())
        {
            AudioBank bank = AudioAssetCatalogDefinitions.BankOf(set);
            if (!byBank.TryGetValue(bank, out InstalledSoundEffectShape? inSet))
            {
                inSet = bank != home && InstrumentsDiffer(catalog, records, home, bank) && PlaysAll(catalog, scans, effect, bank)
                    ? RenderShape(catalog, root, files, scans, effect, key, bank)
                    : null;
                if (inSet is not null && SameShape(inSet, shape))
                    inSet = null;
                byBank[bank] = inSet;
            }
            if (inSet is not null)
                yield return new InstalledSoundEffectSetShape(set, inSet);
        }
    }

    /// <summary>
    /// Whether <paramref name="bank"/> can play every instrument the effect keys. A set that leaves one empty, past its table
    /// or on a sample it does not load is one the game never plays the effect in: the credits' and the last Metroid's sets,
    /// which load no enemy's sounds' instruments, and no sound of theirs reaches them. The effect keeps its own clip there.
    /// </summary>
    private static bool PlaysAll(ExtractedAudioAssetCatalog catalog, Dictionary<string, SoundEffectProgramScan> scans,
        AudioSoundEffectMetadata effect, AudioBank bank) =>
        UnplayableInstruments(catalog, scans, effect, bank).Length == 0;

    /// <summary>
    /// Whether <paramref name="bank"/> holds a different instrument record than <paramref name="home"/> for any of
    /// <paramref name="records"/>, or a different sample for one: either changes how the effect sounds.
    /// </summary>
    private static bool InstrumentsDiffer(ExtractedAudioAssetCatalog catalog, IReadOnlyList<int> records, AudioBank home, AudioBank bank)
    {
        IReadOnlyList<AudioInstrumentMetadata> homeTable = catalog.GetInstrumentBank(home), table = catalog.GetInstrumentBank(bank);
        ManagedPcmSampleBank homeSamples = catalog.GetSampleBank(home), samples = catalog.GetSampleBank(bank);
        foreach (int record in records)
        {
            AudioInstrumentMetadata? before = record < homeTable.Count ? homeTable[record] : null;
            AudioInstrumentMetadata? after = record < table.Count ? table[record] : null;
            if (before != after)
                return true;
            if (after is null || after.UsesNoise)
                continue;
            homeSamples.Samples.TryGetValue(after.SourceOrNoiseRate, out ManagedPcmSample? homeSample);
            samples.Samples.TryGetValue(after.SourceOrNoiseRate, out ManagedPcmSample? sample);
            if (!SameSample(homeSample, sample))
                return true;
        }
        return false;
    }

    /// <summary>Whether two samples play the same: both absent, or the same rate, loop and PCM.</summary>
    private static bool SameSample(ManagedPcmSample? a, ManagedPcmSample? b) =>
        a is null || b is null ? a is null && b is null
            : a.SampleRate == b.SampleRate && a.LoopSampleIndex == b.LoopSampleIndex && a.Samples.Span.SequenceEqual(b.Samples.Span);

    /// <summary>Whether two shapes play the same clips the same way.</summary>
    private static bool SameShape(InstalledSoundEffectShape a, InstalledSoundEffectShape b) => (a, b) switch
    {
        (InstalledOneShotSoundEffect one, InstalledOneShotSoundEffect other) => one == other,
        (InstalledLoopingSoundEffect one, InstalledLoopingSoundEffect other) => one.Layers.SequenceEqual(other.Layers),
        _ => false,
    };

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
