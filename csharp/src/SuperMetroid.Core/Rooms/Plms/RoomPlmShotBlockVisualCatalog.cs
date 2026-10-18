using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Rooms.LevelData;

namespace SuperMetroid.Core.Rooms.Plms;

/// <summary>Complete, immutable presentation selection for the 19 ordinary shot-block lists.</summary>
public sealed class RoomPlmShotBlockVisualCatalog
{
    /// <summary>Canonical identity of the selected visual frames, excluding native mechanics.</summary>
    public string ContentIdentity => SelectedPresentationHash.Create(nameof(RoomPlmShotBlockVisualCatalog), content =>
    {
        Span<ushort> row = stackalloc ushort[2];
        foreach (var draw in RoomPlmShotBlockDrawDefinitions.Calculated.OrderBy(draw => draw.Pointer))
        {
            content.Append("frame", draw.Pointer);
            content.Append("runs", draw.RunCount);
            for (int run = 0; run < draw.RunCount; run++)
            {
                for (int block = 0; block < draw.WordsPerRun; block++)
                    row[block] = GetWord(draw.Pointer, run, block);
                content.AppendWords("words", row[..draw.WordsPerRun]);
            }
        }
    });

    /// <summary>Deep-copied edited run words; stock-identical draw lists stay calculated.</summary>
    private readonly Dictionary<int, ushort[][]>? customWords;

    /// <summary>Validates visual references for all nineteen compiled breakup/restoration draw lists, deep-copying edited runs while retaining calculated stock visuals where supplied words match; native collision words and run placement are never replaced.</summary>
    /// <param name="entries">Complete, duplicate-free draw identity set with unchanged native run counts/lengths and low-twelve-bit visual words.</param>
    /// <exception cref="ArgumentNullException"><paramref name="entries"/> is null.</exception>
    /// <exception cref="InvalidDataException">An entry/run is null, a draw identity is unknown/duplicated/missing, the run shape differs, or a word sets a collision/type bit.</exception>
    public RoomPlmShotBlockVisualCatalog(IEnumerable<RoomPlmShotBlockVisualEntry> entries)
    {
        Ensure.NotNull(entries);
        var selected = new Dictionary<int, ushort[][]>();
        var seen = new HashSet<int>();
        foreach (RoomPlmShotBlockVisualEntry entry in entries)
        {
            if (entry is null || entry.Runs is null ||
                !RoomPlmShotBlockDrawDefinitions.TryGet(checked((ushort)entry.DrawPointer), out var definition) ||
                entry.Runs.Length != definition.RunCount)
                throw new InvalidDataException("Shot-block visuals changed a compiled draw-list identity or shape.");
            for (int run = 0; run < entry.Runs.Length; run++)
            {
                ushort[] visualWords = entry.Runs[run];
                if (visualWords is null ||
                    visualWords.Length != definition.WordsPerRun ||
                    visualWords.Any(word => !RoomLevelWord.IsValidVisualWord(word)))
                    throw new InvalidDataException(
                        $"Shot-block visuals ${entry.DrawPointer:X4} contain an invalid visual word.");
            }

            if (!seen.Add(entry.DrawPointer))
                throw new InvalidDataException(
                    $"Shot-block visuals repeat draw list ${entry.DrawPointer:X4}.");
            bool stock = true;
            for (int run = 0; run < definition.RunCount; run++)
            for (int block = 0; block < definition.WordsPerRun; block++)
                stock &= entry.Runs[run][block] == new RoomLevelWord(definition.WordAt(run, block)).VisualWord;
            if (!stock) selected.Add(entry.DrawPointer, entry.Runs.Select(run => run.ToArray()).ToArray());
        }

        if (seen.Count != RoomPlmShotBlockDrawDefinitions.DrawCount)
            throw new InvalidDataException("Shot-block visuals do not cover all compiled draw lists.");
        if (selected.Count != 0) customWords = selected;
    }

    /// <summary>Returns one visual reference without exposing the catalog's mutable backing arrays.</summary>
    public ushort GetWord(ushort drawPointer, int runIndex, int wordIndex)
    {
        if (!RoomPlmShotBlockDrawDefinitions.TryGet(drawPointer, out var draw))
            throw new InvalidDataException($"Shot-block visuals lack draw list ${drawPointer:X4}.");
        if ((uint)runIndex >= (uint)draw.RunCount ||
            (uint)wordIndex >= (uint)draw.WordsPerRun)
            throw new ArgumentOutOfRangeException(nameof(wordIndex));
        return customWords is not null && customWords.TryGetValue(drawPointer, out var runs)
            ? runs[runIndex][wordIndex] : new RoomLevelWord(draw.WordAt(runIndex, wordIndex)).VisualWord;
    }
}
