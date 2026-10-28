using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Rooms.Plms;

namespace SuperMetroid.Core.Areas.Tourian.Artwork;

/// <summary>
/// Visual-only block selections for the four crumble frames and six-row clear.
/// The compiled draw lists retain every physical level word and run offset.
/// </summary>
public sealed class RoomPlmTourianAccessVisuals
{
    /// <summary>
    /// Canonical identity of the selected visual frames, excluding native mechanics. The identity keeps the name it was
    /// introduced under, so installed content identities do not change.
    /// </summary>
    public string ContentIdentity => SelectedPresentationHash.Create("RoomPlmTourianAccessVisualCatalog", content =>
    {
        Span<ushort> words = stackalloc ushort[24];
        foreach (TourianAccessDraw draw in Enum.GetValues<TourianAccessDraw>().OrderBy(TourianAccessDraws.VisualId, StringComparer.Ordinal))
        {
            TourianAccessDraws.Describe(draw, out int rows, out _);
            for (int index = 0; index < rows * 4; index++)
                words[index] = GetWord(PlmDraw.Of(draw), index / 4, index % 4);
            content.AppendName("frame", TourianAccessDraws.VisualId(draw));
            // Preserve the original flattened-frame hash, including the six-row clear.
            content.Append("runs", 1);
            content.AppendWords("words", words[..(rows * 4)]);
        }
    });

    /// <summary>Copied visual-only words for Tourian access layouts that differ from compiled stock.</summary>
    private readonly PlmVisualOverrides customWords;

    /// <summary>Validates all five Tourian access-floor layouts and captures their selected appearance independently of compiled physical words and row offsets.</summary>
    /// <param name="entries">The empty row, three visible crumble rows, and six-row clear exactly once, each with its compiled block count and valid visual-only words.</param>
    /// <remarks>Edited arrays are copied; stock-identical layouts use compiled visual bits, so caller-array edits after construction cannot alter the catalog.</remarks>
    /// <exception cref="ArgumentNullException"><paramref name="entries"/> is null.</exception>
    /// <exception cref="InvalidDataException">Coverage, uniqueness, layout identity, block-array dimensions, or presentation-only word bits are invalid.</exception>
    public RoomPlmTourianAccessVisuals(
        IEnumerable<RoomPlmVisualEntry> entries)
    {
        customWords = new PlmVisualOverrides(entries, "Tourian access", TourianAccessDraws.TryGetByVisualId,
            TourianAccessDraws.All.Count());
    }

    /// <summary>Resolves one Tourian access-floor visual cell from edited artwork or its compiled stock layout.</summary>
    /// <param name="draw">The empty row, a visible crumble row, or the six-row clear.</param>
    /// <param name="runIndex">Top-to-bottom row index, zero for single-row frames or zero through five for the six-row clear.</param>
    /// <param name="blockIndex">Zero-based left-to-right column within the four-cell row.</param>
    /// <returns>The twelve-bit metatile index/parent-flip word, with no collision nibble or physical level mutation.</returns>
    /// <exception cref="ArgumentOutOfRangeException">The row or column is outside that layout's dimensions; the reported parameter is <paramref name="blockIndex"/>.</exception>
    /// <exception cref="InvalidDataException"><paramref name="draw"/> is not a Tourian access draw.</exception>
    internal ushort GetWord(PlmDraw draw, int runIndex, int blockIndex)
    {
        TourianAccessDraws.Describe(PlmDraw.FrameOf<TourianAccessDraw>(draw, "Tourian access"), out int rows, out int stockWord);
        if ((uint)runIndex >= (uint)rows ||
            (uint)blockIndex >= 4)
            throw new ArgumentOutOfRangeException(nameof(blockIndex));
        return customWords.TryGet(draw, out var words)
            ? words[runIndex * 4 + blockIndex] : new RoomLevelWord(checked((ushort)stockWord)).VisualWord;
    }
}
