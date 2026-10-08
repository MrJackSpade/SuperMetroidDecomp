using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Rooms;

/// <summary>One linked-block restoration image in cartridge run order.</summary>
public sealed record RoomPlmLinkedRestoreVisualEntry(string Id, ushort[] Blocks);

/// <summary>
/// Editable visual block selection for bomb and contact-crumble restoration.
/// The compiled level words retain linked parent/child collision ownership.
/// </summary>
public sealed class RoomPlmLinkedRestoreVisualCatalog
{
    /// <summary>Canonical identity of the selected visual frames, excluding native mechanics.</summary>
    public string ContentIdentity => SelectedPresentationHash.Create(nameof(RoomPlmLinkedRestoreVisualCatalog), content =>
    {
        Span<ushort> words = stackalloc ushort[4];
        foreach (var draw in RoomPlmLinkedRestoreDrawDefinitions.All.OrderBy(draw => draw.Pointer))
        {
            Describe(draw.Pointer, out int runs, out _);
            for (int run = 0; run < runs; run++)
            for (int block = 0; block < 2; block++)
                words[run * 2 + block] = GetWord(draw.Pointer, run, block);
            content.Append("frame", draw.Pointer);
            // The original catalog hashes each flattened frame as one run.
            content.Append("runs", 1);
            content.AppendWords("words", words[..(runs * 2)]);
        }
    });

    private readonly Dictionary<ushort, ushort[]>? customWords;

    public RoomPlmLinkedRestoreVisualCatalog(IEnumerable<RoomPlmLinkedRestoreVisualEntry> entries)
    {
        ArgumentNullException.ThrowIfNull(entries);
        var selected = new Dictionary<ushort, ushort[]>();
        var seen = new HashSet<ushort>();
        foreach (RoomPlmLinkedRestoreVisualEntry entry in entries)
        {
            if (entry is null || entry.Blocks is null ||
                !RoomPlmLinkedRestoreDrawDefinitions.TryGetByVisualId(entry.Id, out var draw) ||
                !Describe(draw.Pointer, out int runs, out ushort stockWord) ||
                entry.Blocks.Length != runs * 2 ||
                entry.Blocks.Any(word => !RoomLevelWord.IsValidVisualWord(word)))
                throw new InvalidDataException(
                    "Linked restoration visuals changed a frame identity, draw shape, or visual word.");
            if (!seen.Add(draw.Pointer))
                throw new InvalidDataException($"Linked restoration visuals repeat frame {entry.Id}.");
            if (entry.Blocks.Any(word => word != stockWord))
                selected.Add(draw.Pointer, entry.Blocks.ToArray());
        }
        if (seen.Count != RoomPlmLinkedRestoreDrawDefinitions.All.Count())
            throw new InvalidDataException("Linked restoration visuals do not cover all six compiled layouts.");
        if (selected.Count != 0) customWords = selected;
    }

    public ushort GetWord(ushort drawPointer, int runIndex, int blockIndex)
    {
        if (!Describe(drawPointer, out int runs, out ushort stockWord))
            throw new InvalidDataException($"Linked restoration visuals lack frame ${drawPointer:X4}.");
        if ((uint)runIndex >= runs || (uint)blockIndex >= 2)
            throw new ArgumentOutOfRangeException(nameof(blockIndex));
        return customWords is not null && customWords.TryGetValue(drawPointer, out var words)
            ? words[runIndex * 2 + blockIndex] : stockWord;
    }

    // Every cell in a linked restoration uses the same visual tile; only its
    // collision link changes. Keep domain identity and geometry in the draw owners.
    private static bool Describe(ushort pointer, out int runs, out ushort stockWord)
    {
        if (RoomPlmBombBlockRestoreDrawDefinitions.TryDescribe(pointer, out var bomb))
        {
            runs = bomb.RunCount;
            stockWord = new RoomLevelWord(bomb.WordAt(0, 0)).VisualWord;
            return true;
        }
        if (RoomPlmContactCrumbleRestoreDrawDefinitions.TryDescribe(pointer, out var crumble))
        {
            runs = crumble.RunCount;
            stockWord = new RoomLevelWord(crumble.WordAt(0, 0)).VisualWord;
            return true;
        }
        runs = 0;
        stockWord = 0;
        return false;
    }
}
