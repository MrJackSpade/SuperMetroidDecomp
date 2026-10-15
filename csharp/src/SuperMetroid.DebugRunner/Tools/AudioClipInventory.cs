using System.Collections.Concurrent;
using System.Diagnostics;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using SuperMetroid.AssetExtraction.Audio;
using SuperMetroid.Core.Audio;

namespace SuperMetroid.DebugRunner.Tools;

/// <summary>
/// The #1305 stage-1 audio inventory: every music track's loop structure, PCM seam and entry-state
/// sensitivity, and every sound effect's state dependence, logical end, loop and key-on markers in
/// each bank context, measured with <see cref="SpcAudioClipRenderer"/> over an installed audio
/// catalog. It writes <c>audio-clip-inventory.json</c> (all measurements) and
/// <c>audio-clip-inventory.md</c> (the gate answer and summary tables).
/// </summary>
internal static class AudioClipInventory
{
    /// <summary>Bytes per native stereo frame in a 16-bit stereo clip.</summary>
    private const int BytesPerStereoFrame = 4;

    /// <summary>Runs the inventory over <paramref name="audioDirectory"/> into <paramref name="outputDirectory"/>.</summary>
    internal static int Run(string audioDirectory, string outputDirectory)
    {
        ExtractedAudioAssetCatalog catalog = ExtractedAudioAssetCatalog.Load(Path.GetFullPath(audioDirectory));
        Directory.CreateDirectory(outputDirectory);
        var timings = new List<(string Phase, double WallSeconds, double SummedSeconds)>();

        // Music structure: sequencer-only, per distinct (bank, pointer).
        var trackKeys = new List<(AudioBank Bank, int Track, ushort Pointer)>();
        foreach (AudioUploadAssetDefinition bank in AudioAssetCatalogDefinitions.Music)
        {
            IReadOnlyList<ushort> pointers = catalog.GetMusicBank(bank.Bank).TrackPointers;
            for (int track = 1; track < pointers.Count; track++)
            {
                if (pointers[track] != 0)
                    trackKeys.Add((bank.Bank, track, pointers[track]));
            }
        }
        MusicTrackAnalysis[] tracks = Timed(timings, "music structure (sequencer only)", trackKeys.Count, index =>
            MusicTrackAnalysis.Analyze(catalog, trackKeys[index].Bank, trackKeys[index].Track, trackKeys[index].Pointer));

        // Music PCM: canonical render, seam check and entry-state perturbations with fixed donors.
        MusicTrackAnalysis[] periodic = [.. tracks.Where(track => track.Shape != MusicTrackShape.NoPeriodFound)];
        MusicTrackAnalysis echoDonor = periodic
            .OrderByDescending(track => track.MaximumEchoDelay)
            .ThenByDescending(track => BitCount(track.EchoVoices))
            .First();
        MusicTrackAnalysis? noiseDonor = periodic
            .Where(track => track.NoiseVoices != 0)
            .OrderByDescending(track => BitCount(track.NoiseVoices))
            .FirstOrDefault();
        ManagedSpcPlayer echoDonorPlayer = MusicTrackRendering.DonorAtLoopPoint(catalog, echoDonor);
        ManagedSpcPlayer? noiseDonorPlayer = noiseDonor is null ? null : MusicTrackRendering.DonorAtLoopPoint(catalog, noiseDonor);
        var pcmResults = new MusicPcmResult[periodic.Length];
        Timed(timings, "music PCM (canonical, seam, entry perturbations)", periodic.Length, index =>
        {
            MusicTrackAnalysis track = periodic[index];
            short[] canonical = MusicTrackRendering.RenderCanonical(catalog, track);
            SeamProfile? seam = track.Shape == MusicTrackShape.Loops ? MusicTrackRendering.CompareSeam(track, canonical) : null;
            SeamProfile? earlySeam = track.Shape == MusicTrackShape.Loops ? MusicTrackRendering.CompareEarlySeam(track, canonical) : null;
            long clipEnd = track.Shape == MusicTrackShape.Loops ? track.Arrivals[2] : track.IntroSamples;
            string clipHash = Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(
                System.Runtime.InteropServices.MemoryMarshal.AsBytes(canonical.AsSpan(0, (int)(clipEnd * 2)))));
            string earlyClipHash = track.Shape == MusicTrackShape.Loops
                ? Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(
                    System.Runtime.InteropServices.MemoryMarshal.AsBytes(canonical.AsSpan(0, (int)(track.Arrivals[1] * 2)))))
                : clipHash;
            long? echo = MusicTrackRendering.FirstEntryDifference(catalog, track, canonical, SpcEntryStateGroup.Echo, echoDonorPlayer);
            long? noise = noiseDonorPlayer is null
                ? null
                : MusicTrackRendering.FirstEntryDifference(catalog, track, canonical, SpcEntryStateGroup.Noise, noiseDonorPlayer);
            long? channels = MusicTrackRendering.FirstEntryDifference(catalog, track, canonical, SpcEntryStateGroup.Channels, echoDonorPlayer);
            pcmResults[index] = new MusicPcmResult(track.Bank, track.Track, canonical.Length / 2,
                seam, earlySeam, clipHash, earlyClipHash, echo, noise, channels);
            return pcmResults[index];
        });

        // Sound effects: static program scan, then every effect in every bank context.
        Dictionary<string, SoundEffectProgramScan> scans = catalog.SoundPrograms
            .ToDictionary(program => program.Id, SoundEffectProgramScan.Scan);
        var contexts = new List<AudioBank?> { null };
        contexts.AddRange(AudioAssetCatalogDefinitions.Music.Select(bank => (AudioBank?)bank.Bank));
        var effectKeys = new List<(AudioBank? Context, SoundEffectLibrary Library, AudioSoundEffectMetadata Effect, bool Forever)>();
        var unmapped = new List<UnmappedEffect>();
        foreach (AudioSoundLibraryMetadata library in catalog.SoundLibraries)
        {
            SoundEffectLibrary id = LibraryOf(library.Library);
            foreach (AudioSoundEffectMetadata effect in library.Effects)
            {
                bool forever = effect.ChannelPrograms.Any(program => scans[program].RepeatsForever);
                foreach (AudioBank? context in contexts)
                {
                    AudioBank bank = context ?? AudioAssetCatalogDefinitions.Common.Bank;
                    string[] missing = [.. effect.ChannelPrograms
                        .SelectMany(program => scans[program].UnmappedSources(catalog.GetInstrumentBank(bank), catalog.GetSampleBank(bank)))
                        .Distinct()];
                    if (missing.Length != 0)
                        unmapped.Add(new UnmappedEffect(context, id, effect.Command, missing));
                    else
                        effectKeys.Add((context, id, effect, forever));
                }
            }
        }
        SoundEffectAnalysis[] effects = Timed(timings, "sound effects (PCM, every bank context)", effectKeys.Count, index =>
            SoundEffectAnalysis.Analyze(catalog, effectKeys[index].Context, effectKeys[index].Library,
                effectKeys[index].Effect.Command, effectKeys[index].Forever));

        var inventory = new InventoryDocument(
            tracks, pcmResults,
            echoDonor is null ? null : $"{echoDonor.Bank} track {echoDonor.Track}",
            noiseDonor is null ? null : $"{noiseDonor.Bank} track {noiseDonor.Track}",
            [.. scans.Values.OrderBy(scan => scan.ProgramId, StringComparer.Ordinal)],
            effects,
            unmapped,
            [.. timings.Select(timing => new PhaseTiming(timing.Phase, timing.WallSeconds, timing.SummedSeconds))]);
        var options = new JsonSerializerOptions { WriteIndented = true, IncludeFields = true };
        options.Converters.Add(new JsonStringEnumConverter());
        File.WriteAllText(Path.Combine(outputDirectory, "audio-clip-inventory.json"), JsonSerializer.Serialize(inventory, options));
        string summary = Summarize(catalog, inventory, contexts);
        File.WriteAllText(Path.Combine(outputDirectory, "audio-clip-inventory.md"), summary);
        Console.WriteLine(summary);
        return 0;
    }

    /// <summary>Runs <paramref name="work"/> over <paramref name="count"/> items in parallel, recording wall and summed per-item time.</summary>
    private static T[] Timed<T>(List<(string, double, double)> timings, string phase, int count, Func<int, T> work)
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
        timings.Add((phase, wall.Elapsed.TotalSeconds, itemTicks.Sum() / (double)Stopwatch.Frequency));
        return results;
    }

    /// <summary>The library identity of the manifest's one-based library number.</summary>
    private static SoundEffectLibrary LibraryOf(int library) => library switch
    {
        1 => SoundEffectLibrary.Library1,
        2 => SoundEffectLibrary.Library2,
        3 => SoundEffectLibrary.Library3,
        _ => throw new InvalidDataException($"Unknown sound library {library}."),
    };

    /// <summary>Number of set voice bits.</summary>
    private static int BitCount(byte voices) => System.Numerics.BitOperations.PopCount(voices);

    /// <summary>Seconds of native audio in <paramref name="samples"/> stereo frames.</summary>
    private static string Seconds(long samples) => (samples / (double)SpcAudioClipRenderer.NativeSampleRate).ToString("0.00");

    /// <summary>Writes the gate answer and the summary tables.</summary>
    private static string Summarize(ExtractedAudioAssetCatalog catalog, InventoryDocument inventory, List<AudioBank?> contexts)
    {
        var text = new StringBuilder();
        text.AppendLine("# #1305 stage 1 audio inventory");
        text.AppendLine();

        // Gate.
        MusicTrackAnalysis[] noPeriod = [.. inventory.Tracks.Where(track => track.Shape == MusicTrackShape.NoPeriodFound)];
        SoundEffectProgramScan[] dependentPrograms = [.. inventory.Programs.Where(scan => scan.DependsOnPriorStateInAnyContext)];
        var noiseRateDependent = new List<string>();
        foreach (SoundEffectProgramScan scan in inventory.Programs.Where(scan => scan.EnablesNoise))
        {
            string[] dependentContexts = [.. contexts
                .Where(context => scan.UsesInheritedNoiseRate(context is null
                    ? catalog.GetInstrumentBank(AudioAssetCatalogDefinitions.Common.Bank)
                    : catalog.GetInstrumentBank(context.Value)))
                .Select(context => context?.ToString() ?? "common")];
            if (dependentContexts.Length != 0)
                noiseRateDependent.Add($"{scan.ProgramId} ({string.Join(", ", dependentContexts)})");
        }
        text.AppendLine("## Gate");
        text.AppendLine();
        text.AppendLine($"- Tracks with no finite loop period found: {noPeriod.Length}" +
            (noPeriod.Length == 0 ? "" : ": " + string.Join(", ", noPeriod.Select(track => $"{track.Bank} {track.Track}"))));
        text.AppendLine($"- Sound programs with (b) retained note, (c) repeat without start, or (e) percussion instrument: {dependentPrograms.Length}");
        foreach (SoundEffectProgramScan scan in dependentPrograms)
        {
            var reasons = new List<string>();
            if (scan.RetainsPreviousNote)
                reasons.Add("(b) first note is $F6");
            if (scan.RepeatsWithoutStart)
                reasons.Add("(c) repeat before repeat start");
            if (scan.PercussionInstruments.Count != 0)
                reasons.Add("(e) percussion instrument " + string.Join(", ", scan.PercussionInstruments.Select(value => $"${value:X2}")));
            text.AppendLine($"  - {scan.ProgramId}: {string.Join("; ", reasons)}; used by {string.Join(", ", EffectsUsing(catalog, scan.ProgramId))}");
        }
        text.AppendLine($"- Sound programs with (d) noise at an inherited rate: {noiseRateDependent.Count}");
        foreach (string entry in noiseRateDependent)
            text.AppendLine($"  - {entry}");
        text.AppendLine();

        // Music.
        int loops = inventory.Tracks.Count(track => track.Shape == MusicTrackShape.Loops);
        int ends = inventory.Tracks.Count(track => track.Shape == MusicTrackShape.Ends);
        text.AppendLine("## Music");
        text.AppendLine();
        text.AppendLine($"{inventory.Tracks.Count} (bank, track) entries: {loops} loop, {ends} end, {noPeriod.Length} without a period found.");
        text.AppendLine($"Entry-state donors: echo and channels from {inventory.EchoDonor}; noise from {inventory.NoiseDonor ?? "none (no track keys noise)"}.");
        text.AppendLine();
        text.AppendLine("Seam columns: differing stereo frames / largest difference in the first 64 frames / largest difference, for loop [E2, E3) and for the shorter loop [E1, E2).");
        text.AppendLine();
        text.AppendLine("| Bank | Track | Shape | Intro s | Loop s | E1 s | Tempo phase repeats | Seam [E2,E3) | Seam [E1,E2) | Echo voices | Noise voices | Echo entry | Noise entry | Channel entry |");
        text.AppendLine("|---|---|---|---|---|---|---|---|---|---|---|---|---|---|");
        foreach (MusicTrackAnalysis track in inventory.Tracks)
        {
            MusicPcmResult? pcm = inventory.MusicPcm.FirstOrDefault(result => result.Bank == track.Bank && result.Track == track.Track);
            string Entry(long? first) => pcm is null ? "-" : first is null ? "same" : $"differs at {Seconds(first.Value)} s";
            text.AppendLine(
                $"| {track.Bank} | {track.Track} | {track.Shape} | {Seconds(track.IntroSamples)} | {Seconds(track.LoopSamples)} | " +
                $"{(track.Arrivals.Count == 3 ? Seconds(track.Arrivals[0]) : "-")} | " +
                $"{(track.Shape == MusicTrackShape.Loops ? track.TempoPhaseRepeats ? "yes" : "no" : "-")} | " +
                $"{Seam(pcm?.Seam)} | {Seam(pcm?.EarlySeam)} | " +
                $"${track.EchoVoices:X2} | ${track.NoiseVoices:X2} | {Entry(pcm?.EchoEntryDifference)} | {Entry(pcm?.NoiseEntryDifference)} | {Entry(pcm?.ChannelEntryDifference)} |");
        }
        text.AppendLine();

        // Sound effects.
        var byEffect = inventory.SoundEffects.GroupBy(effect => (effect.Library, effect.Command)).ToList();
        int looping = byEffect.Count(group => group.First().ActiveSamples is null);
        int contextDependent = byEffect.Count(group => group.Select(effect => effect.PcmSha256).Distinct().Count() > 1);
        var distinctClips = inventory.SoundEffects
            .GroupBy(effect => effect.PcmSha256)
            .Select(group => group.First())
            .ToList();
        int loopSeamsInexact = inventory.SoundEffects.Count(effect => effect.Seam is { Exact: false });
        text.AppendLine("## Sound effects");
        text.AppendLine();
        text.AppendLine($"- {byEffect.Count} effects × {contexts.Count} bank contexts = {inventory.SoundEffects.Count} renders; {distinctClips.Count} distinct clips.");
        text.AppendLine($"- Never-ending (forever repeat): {looping}; renders whose loop seam is not sample-exact: {loopSeamsInexact}.");
        text.AppendLine($"- Effects whose PCM differs between bank contexts: {contextDependent}.");
        text.AppendLine($"- (effect, context) pairs not rendered because the context does not map a source the effect keys: {inventory.Unmapped.Count}.");
        foreach (var group in inventory.Unmapped.GroupBy(entry => entry.Context))
        {
            text.AppendLine($"  - {group.Key?.ToString() ?? "common"}: " + string.Join(", ", group.Select(entry =>
                $"{entry.Library} ${entry.Command:X2} ({string.Join("; ", entry.Missing)})")));
        }
        text.AppendLine();
        text.AppendLine("| Library | Command | Active s | Loop E2/E3 s | Seam [E2,E3) | Channel loops E1/E2 s | Key-ons | Distinct clips across contexts |");
        text.AppendLine("|---|---|---|---|---|---|---|---|");
        foreach (var group in byEffect)
        {
            SoundEffectAnalysis common = group.FirstOrDefault(effect => effect.Context is null) ?? group.First();
            text.AppendLine(
                $"| {group.Key.Library} | ${group.Key.Command:X2} | {(common.ActiveSamples is long active ? Seconds(active) : "loops")} | " +
                $"{(common.Arrivals.Count == 3 ? $"{Seconds(common.Arrivals[1])}/{Seconds(common.Arrivals[2])}" : "-")} | " +
                $"{Seam(common.Seam)} | " +
                $"{(common.ChannelLoops.Count == 0 ? "-" : string.Join(", ", common.ChannelLoops.Select(loop => $"{Seconds(loop.First)}/{Seconds(loop.Second)}")))} | " +
                $"{common.KeyOns.Count} | {group.Select(effect => effect.PcmSha256).Distinct().Count()} |");
        }
        text.AppendLine();

        // Size and cost.
        long musicBytes = inventory.MusicPcm.Sum(result => (long)result.RenderedSamples) * BytesPerStereoFrame;
        Dictionary<(AudioBank, int), MusicTrackAnalysis> byTrack = inventory.Tracks.ToDictionary(track => (track.Bank, track.Track));
        long musicClipBytes = inventory.MusicPcm
            .Sum(result => ClipFrames(byTrack[(result.Bank, result.Track)], early: false) * BytesPerStereoFrame);
        long dedupedMusicBytes = inventory.MusicPcm
            .GroupBy(result => result.ClipSha256)
            .Sum(group => ClipFrames(byTrack[(group.First().Bank, group.First().Track)], early: false) * BytesPerStereoFrame);
        long dedupedEarlyMusicBytes = inventory.MusicPcm
            .GroupBy(result => result.EarlyClipSha256)
            .Sum(group => ClipFrames(byTrack[(group.First().Bank, group.First().Track)], early: true) * BytesPerStereoFrame);
        int distinctMusic = inventory.MusicPcm.Select(result => result.ClipSha256).Distinct().Count();
        long effectBytes = distinctClips.Sum(effect => effect.ClipSamples * BytesPerStereoFrame);
        long oneShotEffectBytes = distinctClips.Where(effect => effect.ActiveSamples is not null).Sum(effect => effect.ClipSamples * BytesPerStereoFrame);
        long perChannelLoopBytes = distinctClips
            .Where(effect => effect.ActiveSamples is null)
            .Sum(effect => effect.ChannelLoops.Sum(loop => (2 * loop.Second - loop.First) * BytesPerStereoFrame));
        text.AppendLine("## Size and render cost");
        text.AppendLine();
        text.AppendLine($"- Music clips (intro [0, E2) plus loop [E2, E3), or to the end), one per (bank, track): {musicClipBytes / 1048576.0:0.0} MiB at 16-bit stereo 32,040 Hz.");
        text.AppendLine($"- The same, deduplicated by PCM hash: {distinctMusic} distinct clips, {dedupedMusicBytes / 1048576.0:0.0} MiB.");
        text.AppendLine($"- With the shorter loop [E1, E2) (intro [0, E1)), deduplicated: {dedupedEarlyMusicBytes / 1048576.0:0.0} MiB.");
        text.AppendLine($"- Distinct sound-effect clips: {effectBytes / 1048576.0:0.0} MiB, of which one-shot sounds {oneShotEffectBytes / 1048576.0:0.0} MiB.");
        text.AppendLine($"- Looping sounds as one clip per channel (each to its own E2 plus one loop): {perChannelLoopBytes / 1048576.0:0.0} MiB instead of {(effectBytes - oneShotEffectBytes) / 1048576.0:0.0} MiB.");
        text.AppendLine($"- Music PCM rendered for the seam check: {musicBytes / 1048576.0:0.0} MiB of audio.");
        text.AppendLine();
        text.AppendLine("| Phase | Items | Wall s | Summed single-thread s |");
        text.AppendLine("|---|---|---|---|");
        foreach (PhaseTiming timing in inventory.Timings)
            text.AppendLine($"| {timing.Phase} | | {timing.WallSeconds:0.0} | {timing.SummedSeconds:0.0} |");
        text.AppendLine();
        text.AppendLine($"Measured on this desktop with {Environment.ProcessorCount} logical processors; Android was not measured.");
        return text.ToString();
    }

    /// <summary>A seam cell: exact, or differing frames / wrap difference / largest difference.</summary>
    private static string Seam(SeamProfile? seam) =>
        seam is null ? "-" : seam.Exact ? "exact" : $"{seam.DifferingFrames} / {seam.WrapDifference} / {seam.MaximumDifference}";

    /// <summary>A track's clip length in stereo frames: E3 (or E2 for the shorter loop) when looping, else its end.</summary>
    private static long ClipFrames(MusicTrackAnalysis track, bool early) => track.Shape switch
    {
        MusicTrackShape.Loops => early ? track.Arrivals[1] : track.Arrivals[2],
        MusicTrackShape.Ends => track.IntroSamples,
        MusicTrackShape.NoPeriodFound => 0,
        _ => throw new InvalidOperationException($"Unknown track shape {track.Shape}."),
    };

    /// <summary>The effects whose channel programs include <paramref name="programId"/>.</summary>
    private static IEnumerable<string> EffectsUsing(ExtractedAudioAssetCatalog catalog, string programId) =>
        catalog.SoundLibraries.SelectMany(library => library.Effects
            .Where(effect => effect.ChannelPrograms.Contains(programId))
            .Select(effect => $"lib{library.Library} ${effect.Command:X2}"));

    /// <summary>A music track's PCM-level measurements.</summary>
    /// <param name="Bank">The track's bank.</param>
    /// <param name="Track">The track number.</param>
    /// <param name="RenderedSamples">Stereo frames rendered for the canonical clip and seam check.</param>
    /// <param name="Seam">How loop [E2, E3) differs from the following pass; null for an ending track.</param>
    /// <param name="EarlySeam">How the shorter loop [E1, E2) differs from [E2, E3); null for an ending track.</param>
    /// <param name="ClipSha256">Hash of the clip PCM [0, E3), or to the end for an ending track.</param>
    /// <param name="EarlyClipSha256">Hash of the shorter clip PCM [0, E2), or to the end for an ending track.</param>
    /// <param name="EchoEntryDifference">First stereo frame changed by the echo donor's entry state, or null if none.</param>
    /// <param name="NoiseEntryDifference">First stereo frame changed by the noise donor's entry state, or null if none or no donor.</param>
    /// <param name="ChannelEntryDifference">First stereo frame changed by the donor's music-voice fields, or null if none.</param>
    private sealed record MusicPcmResult(
        AudioBank Bank,
        int Track,
        long RenderedSamples,
        SeamProfile? Seam,
        SeamProfile? EarlySeam,
        string ClipSha256,
        string EarlyClipSha256,
        long? EchoEntryDifference,
        long? NoiseEntryDifference,
        long? ChannelEntryDifference);

    /// <summary>Wall and summed per-item seconds of one phase.</summary>
    /// <param name="Phase">The phase's name.</param>
    /// <param name="WallSeconds">Elapsed seconds with parallel items.</param>
    /// <param name="SummedSeconds">Sum of each item's elapsed seconds, approximating one core.</param>
    private sealed record PhaseTiming(string Phase, double WallSeconds, double SummedSeconds);

    /// <summary>The complete inventory written as JSON.</summary>
    /// <param name="Tracks">Music structure per (bank, track).</param>
    /// <param name="MusicPcm">PCM measurements per periodic track.</param>
    /// <param name="EchoDonor">The track whose loop-point state perturbs echo and channels.</param>
    /// <param name="NoiseDonor">The track whose loop-point state perturbs noise, if any keys noise.</param>
    /// <param name="Programs">Static scans of every sound program.</param>
    /// <param name="SoundEffects">Every effect in every bank context whose sources it maps.</param>
    /// <param name="Unmapped">(effect, context) pairs whose instruments key sources the context does not map.</param>
    /// <param name="Timings">Phase timings.</param>
    private sealed record InventoryDocument(
        IReadOnlyList<MusicTrackAnalysis> Tracks,
        IReadOnlyList<MusicPcmResult> MusicPcm,
        string? EchoDonor,
        string? NoiseDonor,
        IReadOnlyList<SoundEffectProgramScan> Programs,
        IReadOnlyList<SoundEffectAnalysis> SoundEffects,
        IReadOnlyList<UnmappedEffect> Unmapped,
        IReadOnlyList<PhaseTiming> Timings);

    /// <summary>An effect that keys a BRR source its bank context does not map, so it is not rendered there.</summary>
    /// <param name="Context">The music bank uploaded after the common upload, or null for the common upload alone.</param>
    /// <param name="Library">The effect's library.</param>
    /// <param name="Command">The effect's request byte.</param>
    /// <param name="Missing">Each instrument whose source the context lacks.</param>
    private sealed record UnmappedEffect(AudioBank? Context, SoundEffectLibrary Library, byte Command, IReadOnlyList<string> Missing);
}
