namespace SuperMetroid.Core.Rooms;

/// <summary>One editable visual reference for a compiled breakable-Grapple draw list.</summary>
public sealed record RoomPlmGrappleBlockVisualEntry(ushort DrawPointer, ushort VisualWord);

/// <summary>
/// Complete visual selection for the five Grapple-block frames. Collision, timing,
/// draw shape, and the original full level word remain in compiled definitions.
/// </summary>
public sealed class RoomPlmGrappleBlockVisualCatalog
{
    private readonly IReadOnlyDictionary<ushort, ushort> words;

    public RoomPlmGrappleBlockVisualCatalog(IEnumerable<RoomPlmGrappleBlockVisualEntry> entries)
    {
        ArgumentNullException.ThrowIfNull(entries);
        var selected = new Dictionary<ushort, ushort>();
        foreach (RoomPlmGrappleBlockVisualEntry entry in entries)
        {
            if (entry is null ||
                !RoomPlmGrappleBlockDrawDefinitions.TryGet(entry.DrawPointer, out _) ||
                !RoomLevelWord.IsValidVisualWord(entry.VisualWord))
                throw new InvalidDataException(
                    "Grapple-block visuals changed a compiled draw identity or collision bits.");
            if (!selected.TryAdd(entry.DrawPointer, entry.VisualWord))
                throw new InvalidDataException(
                    $"Grapple-block visuals repeat draw list ${entry.DrawPointer:X4}.");
        }

        if (selected.Count != RoomPlmGrappleBlockDrawDefinitions.All.Count())
            throw new InvalidDataException(
                "Grapple-block visuals do not cover all compiled draw lists.");
        words = selected;
    }

    public static RoomPlmGrappleBlockVisualCatalog Stock() => new(
        RoomPlmGrappleBlockDrawDefinitions.All.Select(definition =>
            new RoomPlmGrappleBlockVisualEntry(definition.Pointer,
                new RoomLevelWord(definition.LevelWord).VisualWord)));

    public ushort GetWord(ushort drawPointer) => words.TryGetValue(drawPointer, out ushort word)
        ? word
        : throw new InvalidDataException(
            $"Grapple-block visuals lack draw list ${drawPointer:X4}.");
}
