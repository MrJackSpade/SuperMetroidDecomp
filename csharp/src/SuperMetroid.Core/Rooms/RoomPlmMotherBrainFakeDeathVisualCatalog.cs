using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Rooms;

/// <summary>Editable tile appearance for one Mother Brain fake-death draw.</summary>
public sealed record RoomPlmMotherBrainFakeDeathVisualEntry(
    string Id, ushort[] Blocks);

/// <summary>
/// Visual-only Mother Brain fake-death wall, room background, door, and tube
/// appearances. Native collision words, draw geometry, and PLM timing remain
/// immutable compiled data, independent of these replacement tile indexes.
/// </summary>
public sealed class RoomPlmMotherBrainFakeDeathVisualCatalog
{
    /// <summary>Canonical identity of the selected visual frames, excluding native mechanics.</summary>
    public string ContentIdentity => SelectedPresentationHash.Create(nameof(RoomPlmMotherBrainFakeDeathVisualCatalog), content =>
    {
        Span<ushort> words = stackalloc ushort[14];
        foreach (var draw in MotherBrainFakeDeathPlmDrawDefinitions.All.OrderBy(draw => draw.Pointer))
        {
            int index = 0;
            for (int run = 0; run < draw.Runs.Length; run++)
            for (int block = 0; block < draw.Runs.Span[run].LevelWords.Length; block++)
                words[index++] = GetWord(draw.Pointer, run, block);
            content.Append("frame", draw.Pointer);
            // Existing installations hash each draw as one flattened visual frame.
            content.Append("runs", 1);
            content.AppendWords("words", words[..index]);
        }
    });

    private readonly Dictionary<ushort, ushort[]>? customWords;

    public RoomPlmMotherBrainFakeDeathVisualCatalog(
        IEnumerable<RoomPlmMotherBrainFakeDeathVisualEntry> entries)
    {
        ArgumentNullException.ThrowIfNull(entries);
        var selected = new Dictionary<ushort, ushort[]>();
        var seen = new HashSet<ushort>();
        foreach (RoomPlmMotherBrainFakeDeathVisualEntry entry in entries)
        {
            if (entry is null || entry.Blocks is null ||
                !MotherBrainFakeDeathPlmDrawDefinitions.TryGetByVisualId(
                    entry.Id, out var draw) ||
                entry.Blocks.Length != draw.Runs.Span.ToArray().Sum(run =>
                    run.LevelWords.Length) ||
                entry.Blocks.Any(word => !RoomLevelWord.IsValidVisualWord(word)))
                throw new InvalidDataException(
                    "Mother Brain fake-death visuals changed a draw identity, shape, or visual word.");
            if (!seen.Add(draw.Pointer))
                throw new InvalidDataException(
                    $"Mother Brain fake-death visuals repeat draw {entry.Id}.");
            bool changed = false;
            int index = 0;
            foreach (var run in draw.Runs.Span)
            foreach (ushort word in run.LevelWords.Span)
                changed |= entry.Blocks[index++] != new RoomLevelWord(word).VisualWord;
            if (changed) selected.Add(draw.Pointer, entry.Blocks.ToArray());
        }
        if (seen.Count != MotherBrainFakeDeathPlmDrawDefinitions.All.Count())
            throw new InvalidDataException(
                "Mother Brain fake-death visuals do not cover all twenty-two draws.");
        if (selected.Count != 0) customWords = selected;
    }

    public ushort GetWord(ushort drawPointer, int runIndex, int blockIndex)
    {
        if (MotherBrainFakeDeathPlmDrawDefinitions.TryDescribeBackground(drawPointer, out var background))
        {
            if (runIndex != 0 || (uint)blockIndex >= 13)
                throw new ArgumentOutOfRangeException(nameof(blockIndex));
            return customWords is not null && customWords.TryGetValue(drawPointer, out var selectedBackground)
                ? selectedBackground[blockIndex] : new RoomLevelWord(background.WordAt(blockIndex)).VisualWord;
        }
        if (MotherBrainFakeDeathPlmDrawDefinitions.TryDescribeRegular(drawPointer, out var regular))
        {
            if ((uint)runIndex >= regular.RunCount || (uint)blockIndex >= regular.Count(runIndex))
                throw new ArgumentOutOfRangeException(nameof(blockIndex));
            return customWords is not null && customWords.TryGetValue(drawPointer, out var selected)
                ? selected[(runIndex == 0 ? 0 : regular.Count(0)) + blockIndex]
                : new RoomLevelWord(regular.WordAt(runIndex, blockIndex)).VisualWord;
        }
        if (!MotherBrainFakeDeathPlmDrawDefinitions.TryDescribeBoundary(drawPointer, out var draw))
            throw new InvalidDataException(
                $"Mother Brain fake-death visuals lack draw ${drawPointer:X4}.");
        if ((uint)runIndex >= 2 ||
            (uint)blockIndex >= (uint)draw.Count(runIndex))
            throw new ArgumentOutOfRangeException(nameof(blockIndex));
        int flatIndex = blockIndex;
        for (int run = 0; run < runIndex; run++)
            flatIndex += draw.Count(run);
        return customWords is not null && customWords.TryGetValue(drawPointer, out var words)
            ? words[flatIndex] : new RoomLevelWord(draw.WordAt(runIndex, blockIndex)).VisualWord;
    }
}
