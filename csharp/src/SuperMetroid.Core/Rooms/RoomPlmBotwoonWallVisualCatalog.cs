using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Rooms;

/// <summary>The editable nine-tile appearance of Botwoon's defeated wall.</summary>
/// <param name="Id">Compiled clear-frame visual identity; no crumble-frame identities are owned by this entry.</param>
/// <param name="Blocks">Nine visual metatile/flip words in the native clear run's order; construction copies a changed array.</param>
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

    /// <summary>Owned clear-frame overrides, or <see langword="null"/> when every selected word matches compiled stock visuals.</summary>
    private readonly ushort[]? blocks;

    /// <summary>Validates the single nine-block wall-clear appearance and retains an owned copy only when it differs from compiled stock visuals.</summary>
    /// <param name="entries">Exactly one known clear-frame entry containing nine visual-only words; crumble frames belong to the separate shot-block catalog.</param>
    /// <exception cref="ArgumentNullException">The entry sequence is null.</exception>
    /// <exception cref="InvalidDataException">The entry count, identity, block count, or visual-only word payload is invalid.</exception>
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

    /// <summary>Gets a clear-frame word from compiled level data with physical collision bits removed.</summary>
    /// <param name="index">Zero-based position within the nine-block wall-clear run.</param>
    /// <returns>The stock visual metatile and flip bits for that position.</returns>
    private static ushort StockWord(int index) =>
        new RoomLevelWord(BotwoonWallPlmDrawDefinitions.LevelWordAt(index)).VisualWord;

    /// <summary>Resolves one selected wall-clear visual word without altering the compiled clear geometry, collision words, or crumble sequence.</summary>
    /// <param name="drawPointer">The compiled bank-$84 Botwoon wall-clear draw identity.</param>
    /// <param name="runIndex">The sole clear-run ordinal, zero.</param>
    /// <param name="blockIndex">Zero-based word ordinal 0..8 within the nine-block clear run.</param>
    /// <returns>The metatile reference and parent flips, excluding physical collision bits.</returns>
    /// <exception cref="InvalidDataException">The pointer is not the clear draw or the run ordinal is not zero.</exception>
    /// <exception cref="ArgumentOutOfRangeException">The block ordinal is outside 0..8.</exception>
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
