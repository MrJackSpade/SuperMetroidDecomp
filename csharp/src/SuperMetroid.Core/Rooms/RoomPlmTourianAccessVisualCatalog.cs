using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Rooms;

/// <summary>One editable Tourian access-floor frame in cartridge draw-run order.</summary>
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

    private RoomPlmTourianAccessVisualCatalog() { }

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

    /// <summary>Calculate stock row appearance from physical draw words; retain only custom artwork.</summary>
    public static RoomPlmTourianAccessVisualCatalog Stock() => new();

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
