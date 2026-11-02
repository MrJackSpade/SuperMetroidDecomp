using SuperMetroid.Core.Rooms.LevelData;

namespace SuperMetroid.Core.Rooms.Plms;

/// <summary>
/// Editable visual block selection for bomb and contact-crumble restoration.
/// The compiled level appearances retain linked parent/child collision ownership.
/// </summary>
public sealed class InstalledRoomPlmLinkedRestoreVisuals
{
    /// <summary>Only layouts whose visual appearances differ from the compiled common appearance need stored overrides.</summary>
    private readonly PlmVisualOverrides customBlocks;

    /// <summary>Holds the import-validated appearances of the layouts that differ from the compiled layouts.</summary>
    /// <param name="customBlocks">The changed layouts' flattened appearances, keyed by draw.</param>
    internal InstalledRoomPlmLinkedRestoreVisuals(PlmVisualOverrides customBlocks) =>
        this.customBlocks = Ensure.NotNull(customBlocks);

    /// <summary>Resolves one linked restoration block by run and block ordinal, leaving its physical collision link compiled.</summary>
    /// <param name="draw">The draw whose appearance is resolved.</param>
    /// <param name="runIndex">Zero-based run ordinal; square layouts have two runs and the other layouts one.</param>
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
    internal static bool Describe(PlmDraw draw, out int runs, out RoomBlockAppearance stock)
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
