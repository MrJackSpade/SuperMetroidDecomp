using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Rooms.Plms;

namespace SuperMetroid.Core.Areas.Tourian.Artwork;

/// <summary>
/// Visual-only block selections for the four crumble frames and six-row clear.
/// The compiled draws retain every block's collision and run offset.
/// </summary>
public sealed class RoomPlmTourianAccessVisuals
{
    /// <summary>
    /// Canonical identity of the selected visual frames, excluding native mechanics. The identity keeps the name it was
    /// introduced under, so installed content identities do not change.
    /// </summary>
    public string ContentIdentity => SelectedPresentationHash.Create("RoomPlmTourianAccessVisualCatalog", content =>
    {
        Span<RoomBlockAppearance> appearances = stackalloc RoomBlockAppearance[24];
        foreach (TourianAccessDraw draw in Enum.GetValues<TourianAccessDraw>().OrderBy(TourianAccessDraws.VisualId, StringComparer.Ordinal))
        {
            TourianAccessDraws.Describe(draw, out int rows, out _);
            for (int index = 0; index < rows * 4; index++)
                appearances[index] = GetAppearance(PlmDraw.Of(draw), index / 4, index % 4);
            content.AppendName("frame", TourianAccessDraws.VisualId(draw));
            // Preserve the original flattened-frame hash, including the six-row clear.
            content.Append("runs", 1);
            content.AppendAppearances("blocks", appearances[..(rows * 4)]);
        }
    });

    /// <summary>Copied visual-only appearances for Tourian access layouts that differ from compiled stock.</summary>
    private readonly PlmVisualOverrides customBlocks;

    /// <summary>Validates all five Tourian access-floor layouts and captures their selected appearance independently of compiled physical appearances and row offsets.</summary>
    /// <param name="entries">The empty row, three visible crumble rows, and six-row clear exactly once, each with its compiled block count and appearances.</param>
    /// <remarks>Edited arrays are copied; stock-identical layouts use compiled appearance, so caller-array edits after construction cannot alter the catalog.</remarks>
    /// <exception cref="ArgumentNullException"><paramref name="entries"/> is null.</exception>
    /// <exception cref="InvalidDataException">Coverage, uniqueness, layout identity, or block-array dimensions are invalid.</exception>
    public RoomPlmTourianAccessVisuals(
        IEnumerable<RoomPlmVisualEntry> entries)
    {
        customBlocks = new PlmVisualOverrides(entries, "Tourian access", TourianAccessDraws.TryGetByVisualId,
            TourianAccessDraws.All.Count());
    }

    /// <summary>Resolves one Tourian access-floor visual cell from edited artwork or its compiled stock layout.</summary>
    /// <param name="draw">The empty row, a visible crumble row, or the six-row clear.</param>
    /// <param name="runIndex">Top-to-bottom row index, zero for single-row frames or zero through five for the six-row clear.</param>
    /// <param name="blockIndex">Zero-based left-to-right column within the four-cell row.</param>
    /// <returns>The block's appearance, with no collision or terrain change.</returns>
    /// <exception cref="ArgumentOutOfRangeException">The row or column is outside that layout's dimensions; the reported parameter is <paramref name="blockIndex"/>.</exception>
    /// <exception cref="InvalidDataException"><paramref name="draw"/> is not a Tourian access draw.</exception>
    internal RoomBlockAppearance GetAppearance(PlmDraw draw, int runIndex, int blockIndex)
    {
        TourianAccessDraws.Describe(PlmDraw.FrameOf<TourianAccessDraw>(draw, "Tourian access"), out int rows, out RoomTerrainBlock stock);
        if ((uint)runIndex >= (uint)rows ||
            (uint)blockIndex >= 4)
            throw new ArgumentOutOfRangeException(nameof(blockIndex));
        return customBlocks.TryGet(draw, out var appearances)
            ? appearances[runIndex * 4 + blockIndex] : stock.Appearance;
    }
}
