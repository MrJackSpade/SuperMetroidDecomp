using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Rooms.RoomObjects;

namespace SuperMetroid.Core.Areas.Tourian.Artwork;

/// <summary>
/// Visual-only block selections for the four crumble frames and six-row clear.
/// The compiled draws retain every block's collision and run offset.
/// </summary>
public sealed class RoomObjectTourianAccessVisuals
{
    /// <summary>Copied visual-only appearances for Tourian access layouts that differ from compiled stock.</summary>
    private readonly RoomObjectVisualOverrides customBlocks;

    /// <summary>Holds the import-validated appearances of the layouts that differ from the compiled layouts.</summary>
    /// <param name="customBlocks">The changed layouts' flattened appearances, keyed by draw.</param>
    internal RoomObjectTourianAccessVisuals(RoomObjectVisualOverrides customBlocks) =>
        this.customBlocks = Ensure.NotNull(customBlocks);

    /// <summary>Resolves one Tourian access-floor visual cell from edited artwork or its compiled stock layout.</summary>
    /// <param name="draw">The empty row, a visible crumble row, or the six-row clear.</param>
    /// <param name="runIndex">Top-to-bottom row index, zero for single-row frames or zero through five for the six-row clear.</param>
    /// <param name="blockIndex">Zero-based left-to-right column within the four-cell row.</param>
    /// <returns>The block's appearance, with no collision or terrain change.</returns>
    /// <exception cref="ArgumentOutOfRangeException">The row or column is outside that layout's dimensions; the reported parameter is <paramref name="blockIndex"/>.</exception>
    /// <exception cref="InvalidDataException"><paramref name="draw"/> is not a Tourian access draw.</exception>
    internal RoomBlockAppearance GetAppearance(RoomObjectDraw draw, int runIndex, int blockIndex)
    {
        TourianAccessDraws.Describe(RoomObjectDraw.FrameOf<TourianAccessDraw>(draw, "Tourian access"), out int rows, out RoomTerrainBlock stock);
        if ((uint)runIndex >= (uint)rows ||
            (uint)blockIndex >= 4)
            throw new ArgumentOutOfRangeException(nameof(blockIndex));
        return customBlocks.TryGet(draw, out var appearances)
            ? appearances[runIndex * 4 + blockIndex] : stock.Appearance;
    }
}
