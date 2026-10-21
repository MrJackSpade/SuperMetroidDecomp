using System.Collections.Concurrent;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using SuperMetroid.AssetExtraction.Audio;
using SuperMetroid.AssetExtraction.Audio.Synthesis;

namespace SuperMetroid.DebugRunner.Tools;

/// <summary>
/// The #1305 audio clip inventory: every music track's loop structure, shipped-loop seam and
/// tempo-phase period, the static sound-program scan for inherited state, and every sound effect's
/// clips in every bank context, measured over an installed extracted catalog with the same
/// analysis and renderers the installer's clip export uses. It writes
/// <c>audio-clip-inventory.json</c> and <c>audio-clip-inventory.md</c>.
/// </summary>
internal static class AudioClipInventory
{
    /// <summary>JSON options: Indented output that includes public fields.</summary>
    private static readonly JsonSerializerOptions IndentedFieldsJson = new() { WriteIndented = true, IncludeFields = true };

    /// <summary>Bytes per native stereo frame in a 16-bit stereo clip.</summary>
    private const int BytesPerStereoFrame = 4;

    /// <summary>Runs the inventory over <paramref name="audioDirectory"/> into <paramref name="outputDirectory"/>.</summary>
    internal static int Run(string audioDirectory, string outputDirectory)
    {
        ExtractedAudioAssetCatalog catalog = ExtractedAudioAssetCatalog.Load(Path.GetFullPath(audioDirectory));
        Directory.CreateDirectory(outputDirectory);
        var timings = new List<PhaseTiming>();

        IReadOnlyList<AudioClipExporter.MusicKey> trackKeys = AudioClipExporter.AllTracks(catalog);
        MusicRow[] music = Timed(timings, "music (structure, clip, seam, tempo phase)", trackKeys.Count, index =>
        {
            AudioClipExporter.MusicKey key = trackKeys[index];
            MusicTrackAnalysis track = MusicTrackAnalysis.Analyze(catalog, key.Bank, key.Track);
            if (track.Shape == MusicTrackShape.NoPeriodFound)
                return new MusicRow(key.Bank, key.Track, track.Shape, 0, null, null, null, "");
            short[] clip = MusicTrackRendering.RenderClip(catalog, track);
            bool loops = track.Shape == MusicTrackShape.Loops;
            return new MusicRow(
                key.Bank, key.Track, track.Shape, clip.Length / 2,
                loops ? track.Arrivals[1] - track.Arrivals[0] : null,
                loops ? AudioClipMeasurements.ShippedLoopSeam(catalog, track) : null,
                loops ? AudioClipMeasurements.TempoPhasePeriod(catalog, track) : null,
                Hash(clip));
        });

        SoundEffectStateScan[] scans = [.. catalog.SoundPrograms.Select(SoundEffectStateScan.Scan)];
        Dictionary<string, SoundEffectProgramScan> exportScans = catalog.SoundPrograms.ToDictionary(program => program.Id, SoundEffectProgramScan.Scan);
        IReadOnlyList<AudioClipExporter.SoundEffectKey> effectKeys = AudioClipExporter.AllSoundEffects(catalog);
        SoundEffectRow[] effects = Timed(timings, "sound effects (clips in every bank context)", effectKeys.Count, index =>
        {
            AudioClipExporter.SoundEffectKey key = effectKeys[index];
            AudioSoundEffectMetadata effect = catalog.SoundLibraries
                .Single(library => library.Library == SoundEffectLibraries.ToQueueIndex(key.Library) + 1)
                .Effects.Single(candidate => candidate.Command == key.Command);
            AudioBank bank = key.Context ?? AudioAssetCatalogDefinitions.Common.AudioSet;
            string[] missing = [.. effect.ChannelPrograms
                .SelectMany(program => exportScans[program].UnmappedSources(catalog.GetInstrumentBank(bank), catalog.GetSampleBank(bank)))
                .Distinct()];
            if (missing.Length != 0)
                return new SoundEffectRow(key.Context, key.Library, key.Command, null, [], [], missing);
            if (effect.ChannelPrograms.Any(program => exportScans[program].RepeatsForever))
            {
                IReadOnlyList<SoundEffectRendering.LoopingLayer> layers = SoundEffectRendering.RenderLoopingLayers(
                    catalog, key.Context, key.Library, key.Command, effect.ChannelPrograms.Count);
                return new SoundEffectRow(key.Context, key.Library, key.Command, null,
                    [.. layers.Select(layer => layer.Clip.Length / 2L)], [.. layers.Select(layer => Hash(layer.Clip))], []);
            }
            SoundEffectRendering.OneShot shot = SoundEffectRendering.RenderOneShot(catalog, key.Context, key.Library, key.Command);
            return new SoundEffectRow(key.Context, key.Library, key.Command, shot.ActiveFrames,
                shot.Clip.Length == 0 ? [] : [shot.Clip.Length / 2L], shot.Clip.Length == 0 ? [] : [Hash(shot.Clip)], []);
        });

        var document = new InventoryDocument(music, scans, effects, timings);
        var options = IndentedFieldsJson;
        options.Converters.Add(new JsonStringEnumConverter());
        File.WriteAllText(Path.Combine(outputDirectory, "audio-clip-inventory.json"), JsonSerializer.Serialize(document, options));
        string summary = Summarize(catalog, document);
        File.WriteAllText(Path.Combine(outputDirectory, "audio-clip-inventory.md"), summary);
        Console.WriteLine(summary);
        return 0;
    }

    /// <summary>Runs <paramref name="work"/> over <paramref name="count"/> items in parallel, recording wall and summed per-item time.</summary>
    private static T[] Timed<T>(List<PhaseTiming> timings, string phase, int count, Func<int, T> work)
    {
        var results = new T[count];
        var itemTicks = new ConcurrentBag<long>();
        var wall = Stopwatch.StartNew();
        Parallel.For(0, count, index =>
        {
            long start = Stopwatch.GetTimestamp();
            results[index] = work(index);
            itemTicks.Add(Stopwatch.GetTimestamp() - start);
        });
        timings.Add(new PhaseTiming(phase, count, wall.Elapsed.TotalSeconds, itemTicks.Sum() / (double)Stopwatch.Frequency));
        return results;
    }

    /// <summary>SHA-256 of a clip's samples, identifying identical clips.</summary>
    private static string Hash(short[] pcm) => Convert.ToHexString(SHA256.HashData(MemoryMarshal.AsBytes(pcm.AsSpan())));

    /// <summary>Seconds of native audio in <paramref name="frames"/> stereo frames.</summary>
    private static string Seconds(long frames) => (frames / (double)SpcAudioClipRenderer.NativeSampleRate).ToString("0.00");

    /// <summary>Writes the gate answer, the summary tables, sizes and timings.</summary>
    private static string Summarize(ExtractedAudioAssetCatalog catalog, InventoryDocument document)
    {
        var text = new StringBuilder();
        text.AppendLine("# #1305 audio clip inventory");
        text.AppendLine();

        MusicRow[] noPeriod = [.. document.Music.Where(row => row.Shape == MusicTrackShape.NoPeriodFound)];
        SoundEffectStateScan[] dependent = [.. document.Programs.Where(scan => scan.DependsOnPriorStateInAnyContext)];
        var contexts = new List<AudioBank?> { null };
        contexts.AddRange(AudioAssetCatalogDefinitions.Music.Select(bank => (AudioBank?)bank.AudioSet));
        string[] inheritedNoise = [.. document.Programs
            .Where(scan => contexts.Any(context => scan.UsesInheritedNoiseRate(
                catalog.GetInstrumentBank(context ?? AudioAssetCatalogDefinitions.Common.AudioSet))))
            .Select(scan => scan.ProgramId)];
        text.AppendLine("## Gate");
        text.AppendLine();
        text.AppendLine($"- Tracks with no finite loop period found: {noPeriod.Length}" +
            (noPeriod.Length == 0 ? "" : ": " + string.Join(", ", noPeriod.Select(row => $"{row.Bank} {row.Track}"))));
        text.AppendLine($"- Sound programs with (b) retained note, (c) repeat without start, or (e) percussion instrument: {dependent.Length}" +
            (dependent.Length == 0 ? "" : ": " + string.Join(", ", dependent.Select(scan => scan.ProgramId))));
        text.AppendLine($"- Sound programs with (d) noise at an inherited rate: {inheritedNoise.Length}" +
            (inheritedNoise.Length == 0 ? "" : ": " + string.Join(", ", inheritedNoise)));
        text.AppendLine();

        text.AppendLine("## Music");
        text.AppendLine();
        text.AppendLine("Seam: differing stereo frames / largest difference in the first 64 frames / largest difference between the shipped loop [E1, E2) and the cartridge's next pass.");
        text.AppendLine();
        text.AppendLine("| Bank | Track | Shape | Clip s | Loop s | Seam | Tempo-phase period (passes) |");
        text.AppendLine("|---|---|---|---|---|---|---|");
        foreach (MusicRow row in document.Music)
        {
            string seam = row.Seam is null ? "-" : row.Seam.Exact ? "exact" : $"{row.Seam.DifferingFrames} / {row.Seam.WrapDifference} / {row.Seam.MaximumDifference}";
            text.AppendLine($"| {row.Bank} | {row.Track} | {row.Shape} | {Seconds(row.ClipFrames)} | " +
                $"{(row.LoopFrames is long loop ? Seconds(loop) : "-")} | {seam} | {row.TempoPhasePeriod?.ToString() ?? "-"} |");
        }
        text.AppendLine();

        SoundEffectRow[] rendered = [.. document.SoundEffects.Where(row => row.Missing.Count == 0)];
        SoundEffectRow[] unrenderable = [.. document.SoundEffects.Where(row => row.Missing.Count != 0)];
        text.AppendLine("## Sound effects");
        text.AppendLine();
        text.AppendLine($"- {document.SoundEffects.Count} (bank context, effect) pairs: {rendered.Length} rendered, {unrenderable.Length} without a clip because the context does not upload a sample they key.");
        text.AppendLine($"- Never-ending effects (per-channel looping layers): {rendered.Count(row => row.ActiveFrames is null)}; silent one-shots: {rendered.Count(row => row.ActiveFrames is not null && row.LayerFrames.Count == 0)}.");
        foreach (var group in unrenderable.GroupBy(row => row.Context))
        {
            text.AppendLine($"  - {group.Key?.ToString() ?? "common"}: " + string.Join(", ", group.Select(row =>
                $"{row.Library} ${row.Command:X2} ({string.Join("; ", row.Missing)})")));
        }
        text.AppendLine();

        long musicBytes = document.Music.Where(row => row.ClipHash.Length != 0)
            .GroupBy(row => row.ClipHash).Sum(group => group.First().ClipFrames * BytesPerStereoFrame);
        var effectClips = new Dictionary<string, long>(StringComparer.Ordinal);
        foreach (SoundEffectRow row in rendered)
        {
            for (int layer = 0; layer < row.LayerHashes.Count; layer++)
                effectClips[row.LayerHashes[layer]] = row.LayerFrames[layer];
        }
        long effectBytes = effectClips.Values.Sum() * BytesPerStereoFrame;
        text.AppendLine("## Size and render cost");
        text.AppendLine();
        text.AppendLine($"- Music: {document.Music.Select(row => row.ClipHash).Where(hash => hash.Length != 0).Distinct().Count()} distinct clips, {musicBytes / 1048576.0:0.0} MiB at 16-bit stereo 32,040 Hz.");
        text.AppendLine($"- Sound effects: {effectClips.Count} distinct clips, {effectBytes / 1048576.0:0.0} MiB.");
        text.AppendLine();
        text.AppendLine("| Phase | Items | Wall s | Summed single-thread s |");
        text.AppendLine("|---|---|---|---|");
        foreach (PhaseTiming timing in document.Timings)
            text.AppendLine($"| {timing.Phase} | {timing.Items} | {timing.WallSeconds:0.0} | {timing.SummedSeconds:0.0} |");
        text.AppendLine();
        text.AppendLine($"Measured on this desktop with {Environment.ProcessorCount} logical processors.");
        return text.ToString();
    }

    /// <summary>One music track's measurements.</summary>
    /// <param name="Bank">The track's bank.</param>
    /// <param name="Track">The track number.</param>
    /// <param name="Shape">Whether it loops, ends or has no period found.</param>
    /// <param name="ClipFrames">The exported clip's stereo frames.</param>
    /// <param name="LoopFrames">The shipped loop's stereo frames, or null when the track ends.</param>
    /// <param name="Seam">The shipped loop against the cartridge's next pass, or null when the track ends.</param>
    /// <param name="TempoPhasePeriod">Loop passes until the tempo phase recurs, or null when the track ends.</param>
    /// <param name="ClipHash">The clip's sample hash, or empty without a period.</param>
    private sealed record MusicRow(
        AudioBank Bank,
        int Track,
        MusicTrackShape Shape,
        long ClipFrames,
        long? LoopFrames,
        SeamProfile? Seam,
        int? TempoPhasePeriod,
        string ClipHash);

    /// <summary>One sound effect in one bank context.</summary>
    /// <param name="Context">The music bank uploaded after the common upload, or null for the common upload alone.</param>
    /// <param name="Library">The effect's library.</param>
    /// <param name="Command">The effect's request byte.</param>
    /// <param name="ActiveFrames">Frames to the logical end, or null for a never-ending effect.</param>
    /// <param name="LayerFrames">Each clip layer's stereo frames.</param>
    /// <param name="LayerHashes">Each clip layer's sample hash.</param>
    /// <param name="Missing">Instruments whose samples the context lacks; empty when rendered.</param>
    private sealed record SoundEffectRow(
        AudioBank? Context,
        SoundEffectLibrary Library,
        int Command,
        long? ActiveFrames,
        IReadOnlyList<long> LayerFrames,
        IReadOnlyList<string> LayerHashes,
        IReadOnlyList<string> Missing);

    /// <summary>Wall and summed per-item seconds of one phase.</summary>
    /// <param name="Phase">The phase's name.</param>
    /// <param name="Items">Items measured.</param>
    /// <param name="WallSeconds">Elapsed seconds with parallel items.</param>
    /// <param name="SummedSeconds">Sum of each item's elapsed seconds, approximating one core.</param>
    private sealed record PhaseTiming(string Phase, int Items, double WallSeconds, double SummedSeconds);

    /// <summary>The complete inventory written as JSON.</summary>
    /// <param name="Music">Every track's measurements.</param>
    /// <param name="Programs">Every sound program's inherited-state scan.</param>
    /// <param name="SoundEffects">Every effect in every bank context.</param>
    /// <param name="Timings">Phase timings.</param>
    private sealed record InventoryDocument(
        IReadOnlyList<MusicRow> Music,
        IReadOnlyList<SoundEffectStateScan> Programs,
        IReadOnlyList<SoundEffectRow> SoundEffects,
        IReadOnlyList<PhaseTiming> Timings);
}
