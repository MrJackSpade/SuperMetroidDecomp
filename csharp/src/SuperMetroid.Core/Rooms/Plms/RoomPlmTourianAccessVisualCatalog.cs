using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Areas.Tourian;
using SuperMetroid.Core.Rooms.LevelData;

namespace SuperMetroid.Core.Rooms.Plms;

/// <summary>
/// Visual-only block selections for the four crumble frames and six-row clear.
/// The compiled draw lists retain every physical level word and run offset.
/// </summary>
public sealed class RoomPlmTourianAccessVisualCatalog
{
    /// <summary>Canonical identity of the selected visual frames, excluding native mechanics.</summary>
    public string ContentIdentity => SelectedPresentationHash.Create(nameof(RoomPlmTourianAccessVisualCatalog), content =>
    {
        Span<ushort> words = stackalloc ushort[24];
        foreach (var draw in TourianAccessPlmDrawDefinitions.All.OrderBy(draw => draw.Pointer))
        {
            TourianAccessPlmDrawDefinitions.TryDescribe(draw.Pointer, out int rows, out _);
            for (int index = 0; index < rows * 4; index++)
                words[index] = GetWord(draw.Pointer, index / 4, index % 4);
            content.Append("frame", draw.Pointer);
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
    public RoomPlmTourianAccessVisualCatalog(
        IEnumerable<RoomPlmVisualEntry> entries)
    {
        customWords = new PlmVisualOverrides(entries, "Tourian access", TourianAccessPlmDrawDefinitions.TryGetByVisualId,
            TourianAccessPlmDrawDefinitions.All.Count());
    }

    /// <summary>Resolves one Tourian access-floor visual cell from edited artwork or its compiled stock layout.</summary>
    /// <param name="drawPointer">Bank-$84 draw pointer: $9297 for an empty row, $92A3/$92AF/$92BB for visible crumble rows, or $92C7 for the six-row clear.</param>
    /// <param name="runIndex">Top-to-bottom row index, zero for single-row frames or zero through five for the six-row clear.</param>
    /// <param name="blockIndex">Zero-based left-to-right column within the four-cell row.</param>
    /// <returns>The twelve-bit metatile index/parent-flip word, with no collision nibble or physical level mutation.</returns>
    /// <exception cref="InvalidDataException"><paramref name="drawPointer"/> is not a compiled Tourian access layout.</exception>
    /// <exception cref="ArgumentOutOfRangeException">The row or column is outside that layout's dimensions; the reported parameter is <paramref name="blockIndex"/>.</exception>
    public ushort GetWord(ushort drawPointer, int runIndex, int blockIndex)
    {
        if (!TourianAccessPlmDrawDefinitions.TryDescribe(drawPointer, out int rows, out ushort stockWord))
            throw new InvalidDataException(
                $"Tourian access visuals lack frame ${drawPointer:X4}.");
        if ((uint)runIndex >= (uint)rows ||
            (uint)blockIndex >= 4)
            throw new ArgumentOutOfRangeException(nameof(blockIndex));
        return customWords.TryGet(drawPointer, out var words)
            ? words[runIndex * 4 + blockIndex] : new RoomLevelWord(stockWord).VisualWord;
    }
}
