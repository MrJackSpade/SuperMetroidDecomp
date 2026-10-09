using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Rooms;

/// <summary>Visual-only blocks for a blue-door cap or shared door-clear frame.</summary>
/// <param name="Id">Compiled editable frame identity, shared by any physical aliases of that frame.</param>
/// <param name="Blocks">Four visual metatile words in the native draw run's order; catalog construction copies changed words and rejects collision bits.</param>
public sealed record RoomPlmBlueDoorVisualEntry(string Id, ushort[] Blocks);

/// <summary>
/// Presentation-only selection for sixteen blue-door frames with four physical aliases. The compiled
/// level words continue to own collision, animation timing, and door handoff.
/// </summary>
public sealed class RoomPlmBlueDoorVisualCatalog
{
    /// <summary>Canonical identity of the selected visual frames, excluding native mechanics.</summary>
    public string ContentIdentity => SelectedPresentationHash.Create(nameof(RoomPlmBlueDoorVisualCatalog), content =>
    {
        Span<ushort> words = stackalloc ushort[4];
        foreach (var frame in BlueDoorPlmDrawDefinitions.Editable.OrderBy(frame => frame.Pointer))
        {
            for (int row = 0; row < words.Length; row++) words[row] = GetWord(frame.Pointer, row);
            content.Append("frame", frame.Pointer);
            content.Append("runs", 1);
            content.AppendWords("words", words);
        }
    });

    private readonly Dictionary<ushort, ushort[]>? customBlocks;

    /// <summary>Validates complete canonical blue-door frame coverage and retains owned copies only for visual words differing from the compiled draw shapes.</summary>
    /// <param name="entries">Exactly one entry for every editable frame, each retaining the native four-block run geometry.</param>
    /// <exception cref="ArgumentNullException">The entry sequence is null.</exception>
    /// <exception cref="InvalidDataException">An entry is null, changes an identity/shape, has invalid visual words, repeats a frame, or leaves coverage incomplete.</exception>
    public RoomPlmBlueDoorVisualCatalog(IEnumerable<RoomPlmBlueDoorVisualEntry> entries)
    {
        ArgumentNullException.ThrowIfNull(entries);
        var selected = new Dictionary<ushort, ushort[]>();
        var seen = new HashSet<ushort>();
        foreach (RoomPlmBlueDoorVisualEntry entry in entries)
        {
            if (entry is null || entry.Blocks is null ||
                !BlueDoorPlmDrawDefinitions.TryGetByVisualId(entry.Id, out var draw) ||
                draw.Runs.Length != 1 ||
                entry.Blocks.Length != draw.Runs.Span[0].LevelWords.Length ||
                entry.Blocks.Any(word => !RoomLevelWord.IsValidVisualWord(word)))
                throw new InvalidDataException(
                    "Blue-door visuals changed a compiled frame identity, draw shape, or visual word.");
            if (!seen.Add(draw.Pointer))
                throw new InvalidDataException($"Blue-door visuals repeat frame {entry.Id}.");
            BlueDoorPlmDrawDefinitions.TryDescribe(draw.Pointer, out var shape);
            bool changed = false;
            for (int row = 0; row < entry.Blocks.Length; row++)
                changed |= entry.Blocks[row] != new RoomLevelWord(shape.WordAt(row)).VisualWord;
            if (changed) selected.Add(draw.Pointer, entry.Blocks.ToArray());
        }
        if (seen.Count != BlueDoorPlmDrawDefinitions.Editable.Count())
            throw new InvalidDataException(
                "Blue-door visuals do not cover all compiled frames.");
        if (selected.Count != 0) customBlocks = selected;
    }

    /// <summary>Resolves a selected blue-door visual word, normalizing physical alias pointers to their shared editable frame without changing collision or door timing.</summary>
    /// <param name="drawPointer">Compiled bank-$84 draw-instruction identity, including supported physical aliases.</param>
    /// <param name="blockIndex">Zero-based word index 0..3 within the native cap/clear run, not a room-block coordinate.</param>
    /// <returns>The visual metatile reference and flips, with physical collision bits excluded.</returns>
    /// <exception cref="InvalidDataException">The draw pointer is not a supported blue-door frame.</exception>
    /// <exception cref="ArgumentOutOfRangeException">The block index is outside 0..3.</exception>
    public ushort GetWord(ushort drawPointer, int blockIndex)
    {
        if (!BlueDoorPlmDrawDefinitions.TryDescribe(drawPointer, out var draw))
            throw new InvalidDataException(
                $"Blue-door visuals lack frame ${drawPointer:X4}.");
        if ((uint)blockIndex >= 4)
            throw new ArgumentOutOfRangeException(nameof(blockIndex));
        return customBlocks is not null && customBlocks.TryGetValue(BlueDoorPlmDrawDefinitions.VisualSource(drawPointer), out var words)
            ? words[blockIndex] : new RoomLevelWord(draw.WordAt(blockIndex)).VisualWord;
    }
}
