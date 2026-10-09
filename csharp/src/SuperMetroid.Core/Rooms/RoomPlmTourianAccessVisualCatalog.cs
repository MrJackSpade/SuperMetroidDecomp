using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Rooms;

/// <summary>One editable Tourian access-floor frame in cartridge draw-run order.</summary>
/// <param name="Id">Case-sensitive compiled identity: <c>crumble-empty-row</c>, <c>crumble-frame-0</c> through <c>crumble-frame-2</c>, or <c>clear-six-rows</c>.</param>
/// <param name="Blocks">Mutable row-major array of twelve-bit metatile/parent-flip words: four 16-by-16 cells for a single-row frame, or twenty-four for the six-row clear; collision bits are excluded.</param>
public sealed record RoomPlmTourianAccessVisualEntry(string Id, ushort[] Blocks);

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

    private readonly Dictionary<ushort, ushort[]>? customWords;

    /// <summary>Validates all five Tourian access-floor layouts and captures their selected appearance independently of compiled physical words and row offsets.</summary>
    /// <param name="entries">The empty row, three visible crumble rows, and six-row clear exactly once, each with its compiled block count and valid visual-only words.</param>
    /// <remarks>Edited arrays are copied; stock-identical layouts use compiled visual bits, so caller-array edits after construction cannot alter the catalog.</remarks>
    /// <exception cref="ArgumentNullException"><paramref name="entries"/> is null.</exception>
    /// <exception cref="InvalidDataException">Coverage, uniqueness, layout identity, block-array dimensions, or presentation-only word bits are invalid.</exception>
    public RoomPlmTourianAccessVisualCatalog(
        IEnumerable<RoomPlmTourianAccessVisualEntry> entries)
    {
        ArgumentNullException.ThrowIfNull(entries);
        var selected = new Dictionary<ushort, ushort[]>();
        var seen = new HashSet<ushort>();
        foreach (RoomPlmTourianAccessVisualEntry entry in entries)
        {
            if (entry is null || entry.Blocks is null ||
                !TourianAccessPlmDrawDefinitions.TryGetByVisualId(entry.Id,
                    out var draw) ||
                !TourianAccessPlmDrawDefinitions.TryDescribe(draw.Pointer, out _, out ushort stockWord) ||
                entry.Blocks.Length != draw.Runs.Span.ToArray().Sum(run =>
                    run.LevelWords.Length) ||
                entry.Blocks.Any(word => !RoomLevelWord.IsValidVisualWord(word)))
                throw new InvalidDataException(
                    "Tourian access visuals changed a frame identity, draw shape, or visual word.");
            if (!seen.Add(draw.Pointer))
                throw new InvalidDataException(
                    $"Tourian access visuals repeat frame {entry.Id}.");
            if (entry.Blocks.Any(word => word != new RoomLevelWord(stockWord).VisualWord))
                selected.Add(draw.Pointer, entry.Blocks.ToArray());
        }
        if (seen.Count != TourianAccessPlmDrawDefinitions.All.Count())
            throw new InvalidDataException(
                "Tourian access visuals do not cover all five compiled layouts.");
        if (selected.Count != 0) customWords = selected;
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
        return customWords is not null && customWords.TryGetValue(drawPointer, out var words)
            ? words[runIndex * 4 + blockIndex] : new RoomLevelWord(stockWord).VisualWord;
    }
}
