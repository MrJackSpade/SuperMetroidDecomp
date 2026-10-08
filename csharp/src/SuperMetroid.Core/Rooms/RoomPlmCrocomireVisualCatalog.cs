using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Rooms;

/// <summary>Editable tile appearances for one Crocomire arena draw layout.</summary>
public sealed record RoomPlmCrocomireVisualEntry(string Id, ushort[] Blocks);

/// <summary>
/// Visual-only Crocomire bridge and invisible-wall blocks. The compiled PLM
/// programs, complete physical level words, and draw geometry remain immutable.
/// </summary>
public sealed class RoomPlmCrocomireVisualCatalog
{
    /// <summary>Canonical identity of the selected visual frames, excluding native mechanics.</summary>
    public string ContentIdentity => SelectedPresentationHash.Create(nameof(RoomPlmCrocomireVisualCatalog), content =>
    {
        Span<ushort> words = stackalloc ushort[24];
        foreach (var draw in CrocomireArenaPlmDrawDefinitions.All.OrderBy(draw => draw.Pointer))
        {
            CrocomireArenaPlmDrawDefinitions.TryDescribe(draw.Pointer, out var shape);
            int count = shape.RunCount * shape.WordsPerRun;
            for (int index = 0; index < count; index++)
                words[index] = GetWord(draw.Pointer, index / shape.WordsPerRun, index % shape.WordsPerRun);
            content.Append("frame", draw.Pointer);
            // Preserve the flattened-frame identity used by existing visual installations.
            content.Append("runs", 1);
            content.AppendWords("words", words[..count]);
        }
    });

    private readonly Dictionary<ushort, ushort[]>? customWords;

    public RoomPlmCrocomireVisualCatalog(
        IEnumerable<RoomPlmCrocomireVisualEntry> entries)
    {
        ArgumentNullException.ThrowIfNull(entries);
        var selected = new Dictionary<ushort, ushort[]>();
        var seen = new HashSet<ushort>();
        foreach (RoomPlmCrocomireVisualEntry entry in entries)
        {
            if (entry is null || entry.Blocks is null ||
                !CrocomireArenaPlmDrawDefinitions.TryGetByVisualId(
                    entry.Id, out var draw) ||
                !CrocomireArenaPlmDrawDefinitions.TryDescribe(draw.Pointer, out var shape) ||
                entry.Blocks.Length != shape.RunCount * shape.WordsPerRun ||
                entry.Blocks.Any(word => !RoomLevelWord.IsValidVisualWord(word)))
                throw new InvalidDataException(
                    "Crocomire visuals changed a draw identity, shape, or visual word.");
            if (!seen.Add(draw.Pointer))
                throw new InvalidDataException(
                    $"Crocomire visuals repeat draw {entry.Id}.");
            bool changed = false;
            for (int index = 0; index < entry.Blocks.Length; index++)
                changed |= entry.Blocks[index] != new RoomLevelWord(
                    shape.WordAt(index / shape.WordsPerRun, index % shape.WordsPerRun)).VisualWord;
            if (changed) selected.Add(draw.Pointer, entry.Blocks.ToArray());
        }
        if (seen.Count != CrocomireArenaPlmDrawDefinitions.All.Count())
            throw new InvalidDataException(
                "Crocomire visuals do not cover all five compiled draws.");
        if (selected.Count != 0) customWords = selected;
    }

    public ushort GetWord(ushort drawPointer, int runIndex, int blockIndex)
    {
        if (!CrocomireArenaPlmDrawDefinitions.TryDescribe(drawPointer, out var draw))
            throw new InvalidDataException(
                $"Crocomire visuals lack draw ${drawPointer:X4}.");
        if ((uint)runIndex >= (uint)draw.RunCount ||
            (uint)blockIndex >= (uint)draw.WordsPerRun)
            throw new ArgumentOutOfRangeException(nameof(blockIndex));
        int flatIndex = runIndex * draw.WordsPerRun + blockIndex;
        return customWords is not null && customWords.TryGetValue(drawPointer, out var words)
            ? words[flatIndex] : new RoomLevelWord(draw.WordAt(runIndex, blockIndex)).VisualWord;
    }
}
