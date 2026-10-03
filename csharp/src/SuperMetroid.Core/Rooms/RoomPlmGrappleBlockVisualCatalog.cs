using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Rooms;

/// <summary>One editable visual reference for a compiled breakable-Grapple draw list.</summary>
public sealed record RoomPlmGrappleBlockVisualEntry(ushort DrawPointer, ushort VisualWord);

/// <summary>
/// Complete visual selection for the five Grapple-block frames. Collision, timing,
/// draw shape, and the original full level word remain in compiled definitions.
/// </summary>
public sealed class RoomPlmGrappleBlockVisualCatalog
{
    /// <summary>Canonical identity of the selected visual frames, excluding native mechanics.</summary>
    public string ContentIdentity => SelectedPresentationHash.Create(nameof(RoomPlmGrappleBlockVisualCatalog), content =>
    {
        Span<ushort> word = stackalloc ushort[1];
        foreach (var draw in RoomPlmGrappleBlockDrawDefinitions.All.OrderBy(draw => draw.Pointer))
        {
            content.Append("frame", draw.Pointer);
            content.Append("runs", 1);
            word[0] = GetWord(draw.Pointer);
            content.AppendWords("words", word);
        }
    });

    private readonly Dictionary<ushort, ushort>? customWords;

    private RoomPlmGrappleBlockVisualCatalog() { }

    public RoomPlmGrappleBlockVisualCatalog(IEnumerable<RoomPlmGrappleBlockVisualEntry> entries)
    {
        ArgumentNullException.ThrowIfNull(entries);
        var selected = new Dictionary<ushort, ushort>();
        var seen = new HashSet<ushort>();
        foreach (RoomPlmGrappleBlockVisualEntry entry in entries)
        {
            if (entry is null ||
                !RoomPlmGrappleBlockDrawDefinitions.TryGet(entry.DrawPointer, out var draw) ||
                !RoomLevelWord.IsValidVisualWord(entry.VisualWord))
                throw new InvalidDataException(
                    "Grapple-block visuals changed a compiled draw identity or collision bits.");
            if (!seen.Add(entry.DrawPointer))
                throw new InvalidDataException(
                    $"Grapple-block visuals repeat draw list ${entry.DrawPointer:X4}.");
            if (entry.VisualWord != new RoomLevelWord(draw.LevelWord).VisualWord)
                selected.Add(entry.DrawPointer, entry.VisualWord);
        }

        if (seen.Count != RoomPlmGrappleBlockDrawDefinitions.All.Count())
            throw new InvalidDataException(
                "Grapple-block visuals do not cover all compiled draw lists.");
        if (selected.Count != 0) customWords = selected;
    }

    /// <summary>Stock visuals use the calculated draw word without storing derived entries.</summary>
    public static RoomPlmGrappleBlockVisualCatalog Stock() => new();

    public ushort GetWord(ushort drawPointer)
    {
        if (!RoomPlmGrappleBlockDrawDefinitions.TryGet(drawPointer, out var draw))
            throw new InvalidDataException($"Grapple-block visuals lack draw list ${drawPointer:X4}.");
        return customWords is not null && customWords.TryGetValue(drawPointer, out ushort word)
            ? word : new RoomLevelWord(draw.LevelWord).VisualWord;
    }
}
