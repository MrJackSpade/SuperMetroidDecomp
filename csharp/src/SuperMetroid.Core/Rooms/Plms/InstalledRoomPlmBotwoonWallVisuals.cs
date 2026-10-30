using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Rooms.LevelData;

namespace SuperMetroid.Core.Rooms.Plms;

/// <summary>
/// Visual-only wall-clear block selections. The nine physical block appearances and
/// the crumble program remain compiled cartridge mechanics. Crumble frames
/// share the existing shot-block visual resource.
/// </summary>
public sealed class InstalledRoomPlmBotwoonWallVisuals
{
    /// <summary>Canonical identity of the selected visual frames, excluding native mechanics.</summary>
    public string ContentIdentity => SelectedPresentationHash.Create(nameof(InstalledRoomPlmBotwoonWallVisuals),
        content =>
        {
            Span<RoomBlockAppearance> selected = stackalloc RoomBlockAppearance[BotwoonWallPlmDrawDefinitions.BlockCount];
            for (int index = 0; index < selected.Length; index++)
                selected[index] = GetAppearance(BotwoonWallPlmDrawDefinitions.Clear, 0, index);
            content.AppendAppearances("blocks", selected);
        });

    /// <summary>Owned nine-block clear appearance retained only when it differs from compiled stock visuals.</summary>
    private readonly RoomBlockAppearance[]? blocks;

    /// <summary>Validates the single nine-block wall-clear appearance and retains an owned copy only when it differs from compiled stock visuals.</summary>
    /// <param name="entries">Exactly one known clear-frame entry containing nine visual-only appearances; crumble frames belong to the separate shot-block catalog.</param>
    /// <exception cref="ArgumentNullException">The entry sequence is null.</exception>
    /// <exception cref="InvalidDataException">The entry count, identity, or block count is invalid.</exception>
    public InstalledRoomPlmBotwoonWallVisuals(
        IEnumerable<RoomPlmVisualEntry> entries)
    {
        Ensure.NotNull(entries);
        RoomPlmVisualEntry[] selected = entries.ToArray();
        if (selected.Length != 1 || selected[0] is null ||
            selected[0].Id != BotwoonWallPlmDraws.VisualId(BotwoonWallPlmDrawDefinitions.Clear) ||
            selected[0].Blocks is null || selected[0].Blocks.Length != 9)
            throw new InvalidDataException(
                "Botwoon wall visuals must contain exactly one nine-block clear frame.");
        bool stock = true;
        for (int index = 0; index < BotwoonWallPlmDrawDefinitions.BlockCount; index++)
            stock &= selected[0].Blocks[index] == StockAppearance(index);
        if (!stock) blocks = selected[0].Blocks.ToArray();
    }

    /// <summary>The appearance of one compiled wall-clear block.</summary>
    private static RoomBlockAppearance StockAppearance(int index) =>
        BotwoonWallPlmDraws.BlockAt(index).Appearance;

    /// <summary>Resolves one selected wall-clear appearance without altering the compiled clear geometry, collision appearances, or crumble sequence.</summary>
    /// <param name="draw">The Botwoon wall-clear draw.</param>
    /// <param name="runIndex">The sole clear-run ordinal, zero.</param>
    /// <param name="blockIndex">Zero-based block ordinal 0..8 within the nine-block clear run.</param>
    /// <returns>The block's appearance, excluding its collision.</returns>
    /// <exception cref="InvalidDataException">The draw is not the clear draw or the run ordinal is not zero.</exception>
    /// <exception cref="ArgumentOutOfRangeException">The block ordinal is outside 0..8.</exception>
    internal RoomBlockAppearance GetAppearance(PlmDraw draw, int runIndex, int blockIndex)
    {
        if (draw != BotwoonWallPlmDrawDefinitions.Clear ||
            runIndex != 0)
            throw new InvalidDataException(
                $"Botwoon wall visuals lack draw {draw}, run {runIndex}.");
        if ((uint)blockIndex >= BotwoonWallPlmDrawDefinitions.BlockCount)
            throw new ArgumentOutOfRangeException(nameof(blockIndex));
        return blocks is null ? StockAppearance(blockIndex) : blocks[blockIndex];
    }
}
