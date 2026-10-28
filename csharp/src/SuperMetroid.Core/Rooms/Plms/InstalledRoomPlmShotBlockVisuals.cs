using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Rooms.LevelData;

namespace SuperMetroid.Core.Rooms.Plms;

/// <summary>Complete, immutable presentation selection for the 19 ordinary shot-block lists.</summary>
public sealed class InstalledRoomPlmShotBlockVisuals
{
    /// <summary>Canonical identity of the selected visual frames, excluding native mechanics.</summary>
    public string ContentIdentity => SelectedPresentationHash.Create(nameof(InstalledRoomPlmShotBlockVisuals), content =>
    {
        Span<ushort> row = stackalloc ushort[2];
        foreach (var draw in ShotBlockPlmDraws.Calculated.OrderBy(ShotBlockPlmDraws.VisualId, StringComparer.Ordinal))
        {
            content.AppendName("frame", ShotBlockPlmDraws.VisualId(draw));
            content.Append("runs", draw.RunCount);
            for (int run = 0; run < draw.RunCount; run++)
            {
                for (int block = 0; block < draw.WordsPerRun; block++)
                    row[block] = GetWord(PlmDraw.Of(draw), run, block);
                content.AppendWords("words", row[..draw.WordsPerRun]);
            }
        }
    });

    /// <summary>Deep-copied edited run words; stock-identical draw lists stay calculated.</summary>
    private readonly Dictionary<ShotBlockPlmDraws.Draw, ushort[][]>? customWords;

    /// <summary>Validates visual references for all nineteen compiled breakup/restoration draw lists, deep-copying edited runs while retaining calculated stock visuals where supplied words match; native collision words and run placement are never replaced.</summary>
    /// <param name="entries">Complete, duplicate-free draw identity set with unchanged native run counts/lengths and low-twelve-bit visual words.</param>
    /// <exception cref="ArgumentNullException"><paramref name="entries"/> is null.</exception>
    /// <exception cref="InvalidDataException">An entry/run is null, a draw identity is unknown/duplicated/missing, the run shape differs, or a word sets a collision/type bit.</exception>
    public InstalledRoomPlmShotBlockVisuals(IEnumerable<RoomPlmVisualRunEntry> entries)
    {
        Ensure.NotNull(entries);
        var selected = new Dictionary<ShotBlockPlmDraws.Draw, ushort[][]>();
        var seen = new HashSet<ShotBlockPlmDraws.Draw>();
        foreach (RoomPlmVisualRunEntry entry in entries)
        {
            if (entry is null || entry.Runs is null ||
                !ShotBlockPlmDraws.TryGetByVisualId(entry.Id, out var definition) ||
                entry.Runs.Length != definition.RunCount)
                throw new InvalidDataException("Shot-block visuals changed a compiled draw-list identity or shape.");
            for (int run = 0; run < entry.Runs.Length; run++)
            {
                ushort[] visualWords = entry.Runs[run];
                if (visualWords is null ||
                    visualWords.Length != definition.WordsPerRun ||
                    visualWords.Any(word => !RoomLevelWord.IsValidVisualWord(word)))
                    throw new InvalidDataException(
                        $"Shot-block visuals {entry.Id} contain an invalid visual word.");
            }

            if (!seen.Add(definition))
                throw new InvalidDataException($"Shot-block visuals repeat draw {entry.Id}.");
            bool stock = true;
            for (int run = 0; run < definition.RunCount; run++)
                for (int block = 0; block < definition.WordsPerRun; block++)
                    stock &= entry.Runs[run][block] == new RoomLevelWord(definition.WordAt(run, block)).VisualWord;
            if (!stock) selected.Add(definition, entry.Runs.Select(run => run.ToArray()).ToArray());
        }

        if (seen.Count != RoomPlmShotBlockDrawDefinitions.DrawCount)
            throw new InvalidDataException("Shot-block visuals do not cover all compiled draw lists.");
        if (selected.Count != 0) customWords = selected;
    }

    /// <summary>Returns one visual reference without exposing the catalog's mutable backing arrays.</summary>
    /// <exception cref="InvalidDataException"><paramref name="draw"/> is not a shot-block draw.</exception>
    internal ushort GetWord(PlmDraw draw, int runIndex, int wordIndex)
    {
        if (draw is not PlmFrameDraw<ShotBlockPlmDraws.Draw> { Frame: var shape })
            throw new InvalidDataException($"Shot-block visuals lack draw {draw}.");
        if ((uint)runIndex >= (uint)shape.RunCount ||
            (uint)wordIndex >= (uint)shape.WordsPerRun)
            throw new ArgumentOutOfRangeException(nameof(wordIndex));
        return customWords is not null && customWords.TryGetValue(shape, out var runs)
            ? runs[runIndex][wordIndex] : new RoomLevelWord(shape.WordAt(runIndex, wordIndex)).VisualWord;
    }
}
