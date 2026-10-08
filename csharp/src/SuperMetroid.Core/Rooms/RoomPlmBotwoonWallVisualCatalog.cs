using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Rooms;

/// <summary>The editable nine-tile appearance of Botwoon's defeated wall.</summary>
public sealed record RoomPlmBotwoonWallVisualEntry(string Id, ushort[] Blocks);

/// <summary>
/// Visual-only wall-clear block selections. The nine physical block words and
/// the crumble program remain compiled cartridge mechanics. Crumble frames
/// share the existing shot-block visual resource.
/// </summary>
public sealed class RoomPlmBotwoonWallVisualCatalog
{
    /// <summary>Canonical identity of the selected visual frames, excluding native mechanics.</summary>
    public string ContentIdentity => SelectedPresentationHash.Create(nameof(RoomPlmBotwoonWallVisualCatalog),
        content =>
        {
            Span<ushort> selected = stackalloc ushort[BotwoonWallPlmDrawDefinitions.BlockCount];
            for (int index = 0; index < selected.Length; index++)
                selected[index] = GetWord(BotwoonWallPlmDrawDefinitions.ClearPointer, 0, index);
            content.AppendWords("blocks", selected);
        });

    private readonly ushort[]? blocks;

    public RoomPlmBotwoonWallVisualCatalog(
        IEnumerable<RoomPlmBotwoonWallVisualEntry> entries)
    {
        ArgumentNullException.ThrowIfNull(entries);
        RoomPlmBotwoonWallVisualEntry[] selected = entries.ToArray();
        if (selected.Length != 1 || selected[0] is null ||
            selected[0].Id != BotwoonWallPlmDrawDefinitions.VisualId(
                BotwoonWallPlmDrawDefinitions.ClearPointer) ||
            selected[0].Blocks is null || selected[0].Blocks.Length != 9 ||
            selected[0].Blocks.Any(word => !RoomLevelWord.IsValidVisualWord(word)))
            throw new InvalidDataException(
                "Botwoon wall visuals must contain exactly one nine-block clear frame.");
        bool stock = true;
        for (int index = 0; index < BotwoonWallPlmDrawDefinitions.BlockCount; index++)
            stock &= selected[0].Blocks[index] == StockWord(index);
        if (!stock) blocks = selected[0].Blocks.ToArray();
    }

    private static ushort StockWord(int index) =>
        new RoomLevelWord(BotwoonWallPlmDrawDefinitions.LevelWordAt(index)).VisualWord;

    public ushort GetWord(ushort drawPointer, int runIndex, int blockIndex)
    {
        if (drawPointer != BotwoonWallPlmDrawDefinitions.ClearPointer ||
            runIndex != 0)
            throw new InvalidDataException(
                $"Botwoon wall visuals lack draw ${drawPointer:X4}, run {runIndex}.");
        if ((uint)blockIndex >= BotwoonWallPlmDrawDefinitions.BlockCount)
            throw new ArgumentOutOfRangeException(nameof(blockIndex));
        return blocks is null ? StockWord(blockIndex) : blocks[blockIndex];
    }
}
