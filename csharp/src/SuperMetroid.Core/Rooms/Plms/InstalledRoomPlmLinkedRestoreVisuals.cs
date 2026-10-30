using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Rooms.LevelData;

namespace SuperMetroid.Core.Rooms.Plms;

/// <summary>
/// Editable visual block selection for bomb and contact-crumble restoration.
/// The compiled level appearances retain linked parent/child collision ownership.
/// </summary>
public sealed class InstalledRoomPlmLinkedRestoreVisuals
{
    /// <summary>Canonical identity of the selected visual frames, excluding native mechanics.</summary>
    public string ContentIdentity => SelectedPresentationHash.Create(nameof(InstalledRoomPlmLinkedRestoreVisuals), content =>
    {
        Span<RoomBlockAppearance> appearances = stackalloc RoomBlockAppearance[4];
        foreach (var draw in RoomPlmLinkedRestoreDrawDefinitions.All.OrderBy(item => LinkedRestorePlmDraws.VisualId(item.Draw), StringComparer.Ordinal))
        {
            Describe(draw.Draw, out int runs, out _);
            for (int run = 0; run < runs; run++)
                for (int block = 0; block < 2; block++)
                    appearances[run * 2 + block] = GetAppearance(draw.Draw, run, block);
            content.AppendName("frame", LinkedRestorePlmDraws.VisualId(draw.Draw));
            // The original catalog hashes each flattened frame as one run.
            content.Append("runs", 1);
            content.AppendAppearances("blocks", appearances[..(runs * 2)]);
        }
    });

    /// <summary>Only layouts whose visual appearances differ from the compiled common appearance need stored overrides.</summary>
    private readonly PlmVisualOverrides customBlocks;

    /// <summary>Validates all six linked restoration layouts and copies visual differences without changing linked parent/child collision or restoration programs.</summary>
    /// <param name="entries">One visual-only entry per compiled layout with two appearances per native run and complete identity coverage.</param>
    /// <exception cref="ArgumentNullException">The entry sequence is null.</exception>
    /// <exception cref="InvalidDataException">An entry is invalid, repeats an identity, changes a draw shape, or leaves compiled coverage incomplete.</exception>
    public InstalledRoomPlmLinkedRestoreVisuals(IEnumerable<RoomPlmVisualEntry> entries)
    {
        customBlocks = new PlmVisualOverrides(entries, "Linked restoration", LinkedRestorePlmDraws.TryGetByVisualId,
            RoomPlmLinkedRestoreDrawDefinitions.All.Count());
    }

    /// <summary>Resolves one linked restoration block by native run and block ordinal, leaving its physical collision link compiled.</summary>
    /// <param name="draw">The draw whose appearance is resolved.</param>
    /// <param name="runIndex">Zero-based native run ordinal; square layouts have two runs and the other layouts one.</param>
    /// <param name="blockIndex">Block ordinal within the run, zero or one, not a flattened entry index.</param>
    /// <returns>The authored appearance, or the layout's common compiled appearance when unchanged.</returns>
    /// <exception cref="InvalidDataException">The draw is not a supported linked restoration layout.</exception>
    /// <exception cref="ArgumentOutOfRangeException">The run or block ordinal is outside the compiled draw shape.</exception>
    internal RoomBlockAppearance GetAppearance(PlmDraw draw, int runIndex, int blockIndex)
    {
        if (!Describe(draw, out int runs, out RoomBlockAppearance stock))
            throw new InvalidDataException($"Linked restoration visuals lack frame {draw}.");
        if ((uint)runIndex >= runs || (uint)blockIndex >= 2)
            throw new ArgumentOutOfRangeException(nameof(blockIndex));
        return customBlocks.TryGet(draw, out var appearances)
            ? appearances[runIndex * 2 + blockIndex] : stock;
    }

    /// <summary>
    /// Resolves a bomb or contact-crumble draw's geometry and shared visual tile.
    /// Every cell uses that tile while collision links remain owned by the draw definitions.
    /// </summary>
    private static bool Describe(PlmDraw draw, out int runs, out RoomBlockAppearance stock)
    {
        if (LinkedRestorePlmFamily.TryDescribeAny(draw, out var shape))
        {
            runs = shape.RunCount;
            stock = shape.BlockAt(0, 0).Appearance;
            return true;
        }
        runs = 0;
        stock = default;
        return false;
    }
}
