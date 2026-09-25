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
    private readonly ushort[] blocks;

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
        blocks = selected[0].Blocks.ToArray();
    }

    public static RoomPlmBotwoonWallVisualCatalog Stock() => new(
        [new RoomPlmBotwoonWallVisualEntry("clear-wall",
            Enumerable.Repeat((ushort)0x00ff, 9).ToArray())]);

    public ushort GetWord(ushort drawPointer, int runIndex, int blockIndex)
    {
        if (drawPointer != BotwoonWallPlmDrawDefinitions.ClearPointer ||
            runIndex != 0)
            throw new InvalidDataException(
                $"Botwoon wall visuals lack draw ${drawPointer:X4}, run {runIndex}.");
        if ((uint)blockIndex >= (uint)blocks.Length)
            throw new ArgumentOutOfRangeException(nameof(blockIndex));
        return blocks[blockIndex];
    }
}
