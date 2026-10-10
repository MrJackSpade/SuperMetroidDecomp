using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Rooms;

/// <summary>Visual-only blocks for one colored-door orientation and frame.</summary>
/// <param name="Id">Compiled yellow, green, or red door-cap frame identity, including its orientation.</param>
/// <param name="Blocks">Four metatile/flip words in native run order; changed arrays are copied by the catalog.</param>
public sealed record RoomPlmColoredDoorVisualEntry(string Id, ushort[] Blocks);

/// <summary>
/// Presentation-only selection for forty-eight yellow, green and red door-cap frames. The compiled
/// level words continue to own collision, animation timing, and door handoff.
/// </summary>
public sealed class RoomPlmColoredDoorVisualCatalog
{
    /// <summary>Canonical identity of the selected visual frames, excluding native mechanics.</summary>
    public string ContentIdentity => SelectedPresentationHash.Create(nameof(RoomPlmColoredDoorVisualCatalog), content =>
    {
        Span<ushort> words = stackalloc ushort[4];
        foreach (var frame in ColoredDoorPlmDrawDefinitions.All.OrderBy(frame => frame.Pointer))
        {
            for (int row = 0; row < words.Length; row++) words[row] = GetWord(frame.Pointer, row);
            content.Append("frame", frame.Pointer);
            content.Append("runs", 1);
            content.AppendWords("words", words);
        }
    });

    /// <summary>Owned visual replacements keyed by compiled frame pointer; null when all selected frames retain their stock words.</summary>
    private readonly Dictionary<ushort, ushort[]>? customBlocks;

    /// <summary>Validates complete coverage of the forty-eight colored-door frames and copies visual differences while preserving compiled cap geometry and opening mechanics.</summary>
    /// <param name="entries">Exactly one four-word, visual-only entry for each compiled color, orientation, and frame identity.</param>
    /// <exception cref="ArgumentNullException">The entry sequence is null.</exception>
    /// <exception cref="InvalidDataException">An entry is invalid, repeats an identity, changes a draw shape, or leaves frame coverage incomplete.</exception>
    public RoomPlmColoredDoorVisualCatalog(IEnumerable<RoomPlmColoredDoorVisualEntry> entries)
    {
        ArgumentNullException.ThrowIfNull(entries);
        var selected = new Dictionary<ushort, ushort[]>();
        var seen = new HashSet<ushort>();
        foreach (RoomPlmColoredDoorVisualEntry entry in entries)
        {
            if (entry is null || entry.Blocks is null ||
                !ColoredDoorPlmDrawDefinitions.TryGetByVisualId(entry.Id, out var draw) ||
                draw.Runs.Length != 1 ||
                entry.Blocks.Length != draw.Runs.Span[0].LevelWords.Length ||
                entry.Blocks.Any(word => !RoomLevelWord.IsValidVisualWord(word)))
                throw new InvalidDataException(
                    "Colored-door visuals changed a compiled frame identity, draw shape, or visual word.");
            if (!seen.Add(draw.Pointer))
                throw new InvalidDataException($"Colored-door visuals repeat frame {entry.Id}.");
            ColoredDoorPlmDrawDefinitions.TryDescribe(draw.Pointer, out var shape);
            bool changed = false;
            for (int row = 0; row < entry.Blocks.Length; row++)
                changed |= entry.Blocks[row] != new RoomLevelWord(shape.WordAt(row)).VisualWord;
            if (changed) selected.Add(draw.Pointer, entry.Blocks.ToArray());
        }
        if (seen.Count != ColoredDoorPlmDrawDefinitions.All.Count())
            throw new InvalidDataException(
                "Colored-door visuals do not cover all compiled frames.");
        if (selected.Count != 0) customBlocks = selected;
    }

    /// <summary>Resolves one selected cap block without changing the physical collision word or door-opening rules.</summary>
    /// <param name="drawPointer">Bank-$84 identity of the compiled colored-door draw frame.</param>
    /// <param name="blockIndex">Zero-based word ordinal, zero through three, along the frame's horizontal or vertical run.</param>
    /// <returns>The authored metatile/flip word, or the compiled stock word's visual bits when unchanged.</returns>
    /// <exception cref="InvalidDataException">The pointer does not identify a supported colored-door frame.</exception>
    /// <exception cref="ArgumentOutOfRangeException">The block ordinal is outside the four-word run.</exception>
    public ushort GetWord(ushort drawPointer, int blockIndex)
    {
        if (!ColoredDoorPlmDrawDefinitions.TryDescribe(drawPointer, out var draw))
            throw new InvalidDataException(
                $"Colored-door visuals lack frame ${drawPointer:X4}.");
        if ((uint)blockIndex >= 4)
            throw new ArgumentOutOfRangeException(nameof(blockIndex));
        return customBlocks is not null && customBlocks.TryGetValue(drawPointer, out var words)
            ? words[blockIndex] : new RoomLevelWord(draw.WordAt(blockIndex)).VisualWord;
    }
}
