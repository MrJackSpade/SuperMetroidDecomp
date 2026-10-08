using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Rooms;

/// <summary>One Chozo statue PLM layout's visual blocks in native run order.</summary>
public sealed record RoomPlmChozoStatueVisualEntry(string Id, ushort[] Blocks);

/// <summary>
/// Editable Chozo hand and slope-access appearances. The calculated draw geometry and full
/// level words remain independent of installed visual block references.
/// </summary>
public sealed class RoomPlmChozoStatueVisualCatalog
{
    /// <summary>Canonical identity of the selected visual frames, excluding native mechanics.</summary>
    public string ContentIdentity => SelectedPresentationHash.Create(nameof(RoomPlmChozoStatueVisualCatalog), content =>
    {
        Span<ushort> words = stackalloc ushort[27];
        foreach (var frame in ChozoStatuePlmDrawDefinitions.All.OrderBy(frame => frame.Pointer))
        {
            ChozoStatuePlmDrawDefinitions.TryDescribe(frame.Pointer, out var shape);
            int count = 0;
            for (int run = 0; run < shape.RunCount; run++)
            for (int word = 0; word < shape.WordCount(run); word++)
                words[count++] = GetWord(frame.Pointer, run, word);
            content.Append("frame", frame.Pointer);
            // Preserve the flattened single-run framing of installed Chozo artwork.
            content.Append("runs", 1);
            content.AppendWords("words", words[..count]);
        }
    });

    private readonly Dictionary<ushort, ushort[]>? customBlocks;

    public RoomPlmChozoStatueVisualCatalog(
        IEnumerable<RoomPlmChozoStatueVisualEntry> entries)
    {
        ArgumentNullException.ThrowIfNull(entries);
        var selected = new Dictionary<ushort, ushort[]>();
        var seen = new HashSet<ushort>();
        foreach (RoomPlmChozoStatueVisualEntry entry in entries)
        {
            if (entry is null || entry.Blocks is null ||
                !ChozoStatuePlmDrawDefinitions.TryGetByVisualId(entry.Id,
                    out var draw) ||
                entry.Blocks.Length != draw.Runs.Span.ToArray().Sum(run =>
                    run.LevelWords.Length) ||
                entry.Blocks.Any(word => !RoomLevelWord.IsValidVisualWord(word)))
                throw new InvalidDataException(
                    "Chozo statue visuals changed a frame identity, draw shape, or visual word.");
            if (!seen.Add(draw.Pointer))
                throw new InvalidDataException(
                    $"Chozo statue visuals repeat frame {entry.Id}.");
            ChozoStatuePlmDrawDefinitions.TryDescribe(draw.Pointer, out var shape);
            int index = 0;
            bool changed = false;
            for (int run = 0; run < shape.RunCount; run++)
            for (int word = 0; word < shape.WordCount(run); word++)
                changed |= entry.Blocks[index++] != new RoomLevelWord(shape.WordAt(run, word)).VisualWord;
            if (changed) selected.Add(draw.Pointer, entry.Blocks.ToArray());
        }
        if (seen.Count != ChozoStatuePlmDrawDefinitions.All.Count())
            throw new InvalidDataException(
                "Chozo statue visuals do not cover all compiled frames.");
        if (selected.Count != 0) customBlocks = selected;
    }

    public ushort GetWord(ushort drawPointer, int runIndex, int blockIndex)
    {
        if (!ChozoStatuePlmDrawDefinitions.TryDescribe(drawPointer, out var draw))
            throw new InvalidDataException(
                $"Chozo statue visuals lack frame ${drawPointer:X4}.");
        if ((uint)runIndex >= (uint)draw.RunCount ||
            (uint)blockIndex >= (uint)draw.WordCount(runIndex))
            throw new ArgumentOutOfRangeException(nameof(blockIndex));
        int flatIndex = blockIndex;
        for (int run = 0; run < runIndex; run++)
            flatIndex += draw.WordCount(run);
        return customBlocks is not null && customBlocks.TryGetValue(drawPointer, out var words)
            ? words[flatIndex] : new RoomLevelWord(draw.WordAt(runIndex, blockIndex)).VisualWord;
    }
}
