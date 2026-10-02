using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Rooms;

/// <summary>One replaceable Samus Eater block appearance in native run order.</summary>
public sealed record RoomPlmSamusEaterVisualEntry(string Id, ushort[] Blocks);

/// <summary>
/// Visual-only level words for all four floor and four ceiling plant poses.
/// The compiled draw definitions retain their physical collision nibble and
/// three-run geometry even when a player replaces these visible block IDs.
/// </summary>
public sealed class RoomPlmSamusEaterVisualCatalog
{
    /// <summary>Canonical identity of the selected visual frames, excluding native mechanics.</summary>
    public string ContentIdentity => SelectedPresentationHash.Create(nameof(RoomPlmSamusEaterVisualCatalog), content =>
    {
        Span<ushort> words = stackalloc ushort[8];
        foreach (var draw in SamusEaterPlmDrawDefinitions.All.OrderBy(draw => draw.Pointer))
        {
            int index = 0;
            for (int run = 0; run < 3; run++)
            for (int block = 0; block < SamusEaterPlmDrawDefinitions.Draw.Count(run); block++)
                words[index++] = GetWord(draw.Pointer, run, block);
            content.Append("frame", draw.Pointer);
            // Preserve the original flattened-frame hash, not the three physical runs.
            content.Append("runs", 1);
            content.AppendWords("words", words);
        }
    });

    private readonly Dictionary<ushort, ushort[]>? customWords;

    private RoomPlmSamusEaterVisualCatalog() { }

    public RoomPlmSamusEaterVisualCatalog(
        IEnumerable<RoomPlmSamusEaterVisualEntry> entries)
    {
        ArgumentNullException.ThrowIfNull(entries);
        var selected = new Dictionary<ushort, ushort[]>();
        var seen = new HashSet<ushort>();
        foreach (RoomPlmSamusEaterVisualEntry entry in entries)
        {
            if (entry is null || entry.Blocks is null ||
                !SamusEaterPlmDrawDefinitions.TryGetByVisualId(entry.Id, out var draw) ||
                entry.Blocks.Length != draw.Runs.Span.ToArray().Sum(run =>
                    run.LevelWords.Length) ||
                entry.Blocks.Any(word => !RoomLevelWord.IsValidVisualWord(word)))
                throw new InvalidDataException(
                    "Samus Eater visuals changed a frame identity, draw shape, or visual word.");
            if (!seen.Add(draw.Pointer))
                throw new InvalidDataException(
                    $"Samus Eater visuals repeat frame {entry.Id}.");
            bool differs = false;
            int index = 0;
            foreach (var run in draw.Runs.Span)
            foreach (ushort word in run.LevelWords.Span)
                differs |= entry.Blocks[index++] != new RoomLevelWord(word).VisualWord;
            if (differs) selected.Add(draw.Pointer, entry.Blocks.ToArray());
        }
        if (seen.Count != SamusEaterPlmDrawDefinitions.All.Count())
            throw new InvalidDataException(
                "Samus Eater visuals do not cover all eight compiled frames.");
        if (selected.Count != 0) customWords = selected;
    }

    /// <summary>Stock appearance is calculated from the physical draw's visual bits without a cache.</summary>
    public static RoomPlmSamusEaterVisualCatalog Stock() => new();

    public ushort GetWord(ushort drawPointer, int runIndex, int blockIndex)
    {
        if (!SamusEaterPlmDrawDefinitions.TryDescribe(drawPointer, out var draw))
            throw new InvalidDataException(
                $"Samus Eater visuals lack frame ${drawPointer:X4}.");
        if ((uint)runIndex >= 3 ||
            (uint)blockIndex >= (uint)SamusEaterPlmDrawDefinitions.Draw.Count(runIndex))
            throw new ArgumentOutOfRangeException(nameof(blockIndex));
        int flatIndex = blockIndex;
        for (int run = 0; run < runIndex; run++)
            flatIndex += SamusEaterPlmDrawDefinitions.Draw.Count(run);
        return customWords is not null && customWords.TryGetValue(drawPointer, out var words)
            ? words[flatIndex] : new RoomLevelWord(draw.WordAt(runIndex, blockIndex)).VisualWord;
    }
}
