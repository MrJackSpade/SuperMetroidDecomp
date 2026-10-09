using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Rooms;

/// <summary>Editable visual blocks for one eye-door draw frame.</summary>
/// <param name="Id">Compiled editable identity for an eye, middle, bottom, or shared clearing frame.</param>
/// <param name="Blocks">Metatile/flip words in native single-run order, with the component's exact block count; changed arrays are copied by the catalog.</param>
public sealed record RoomPlmEyeDoorVisualEntry(string Id, ushort[] Blocks);

/// <summary>
/// Presentation-only choices for the mirrored eye, middle and bottom components.
/// Door collision, attack logic, hit counters, timers and persistence stay compiled.
/// </summary>
public sealed class RoomPlmEyeDoorVisualCatalog
{
    /// <summary>Canonical identity of the selected visual frames, excluding native mechanics.</summary>
    public string ContentIdentity => SelectedPresentationHash.Create(nameof(RoomPlmEyeDoorVisualCatalog), content =>
    {
        Span<ushort> buffer = stackalloc ushort[4];
        foreach (var frame in EyeDoorPlmDrawDefinitions.Editable.OrderBy(frame => frame.Pointer))
        {
            EyeDoorPlmDrawDefinitions.TryDescribe(frame.Pointer, out var shape);
            content.Append("frame", frame.Pointer);
            content.Append("runs", 1);
            for (int cell = 0; cell < shape.WordCount; cell++) buffer[cell] = GetWord(frame.Pointer, cell);
            content.AppendWords("words", buffer[..shape.WordCount]);
        }
    });

    private readonly Dictionary<ushort, ushort[]>? customBlocks;

    /// <summary>Validates all twenty-three editable eye-door frames and copies visual differences without changing collision, attacks, hit counters, timing, or persistence.</summary>
    /// <remarks>The mirrored opening clear shares the clearing frame's artwork rather than requiring a separate authored entry.</remarks>
    /// <param name="entries">One visual-only entry per compiled editable identity, retaining each eye or component frame's draw shape.</param>
    /// <exception cref="ArgumentNullException">The entry sequence is null.</exception>
    /// <exception cref="InvalidDataException">An entry is invalid, repeats an identity, changes a draw shape, or leaves editable frame coverage incomplete.</exception>
    public RoomPlmEyeDoorVisualCatalog(IEnumerable<RoomPlmEyeDoorVisualEntry> entries)
    {
        ArgumentNullException.ThrowIfNull(entries);
        var selected = new Dictionary<ushort, ushort[]>();
        var seen = new HashSet<ushort>();
        foreach (RoomPlmEyeDoorVisualEntry entry in entries)
        {
            if (entry is null || entry.Blocks is null ||
                !EyeDoorPlmDrawDefinitions.TryGetByVisualId(entry.Id, out var draw) ||
                draw.Runs.Length != 1 ||
                entry.Blocks.Length != draw.Runs.Span[0].LevelWords.Length ||
                entry.Blocks.Any(word => !RoomLevelWord.IsValidVisualWord(word)))
                throw new InvalidDataException(
                    "Eye-door visuals changed a frame identity, draw shape, or visual word.");
            if (!seen.Add(draw.Pointer))
                throw new InvalidDataException(
                    $"Eye-door visuals repeat frame {entry.Id}.");
            EyeDoorPlmDrawDefinitions.TryDescribe(draw.Pointer, out var shape);
            bool changed = false;
            for (int cell = 0; cell < shape.WordCount; cell++)
                changed |= entry.Blocks[cell] != new RoomLevelWord(shape.WordAt(cell)).VisualWord;
            if (changed) selected.Add(draw.Pointer, entry.Blocks.ToArray());
        }
        if (seen.Count != EyeDoorPlmDrawDefinitions.Editable.Count())
            throw new InvalidDataException(
                "Eye-door visuals do not cover all compiled frames.");
        if (selected.Count != 0) customBlocks = selected;
    }

    /// <summary>Resolves an eye-door visual block, sharing and horizontally mirroring the selected clearing artwork for the native opening-clear alias.</summary>
    /// <param name="drawPointer">Bank-$84 eye-door draw identity, including mirrored opening clear $9BF7, which uses clearing frame $9C4F.</param>
    /// <param name="blockIndex">Zero-based word ordinal within the component's single native run: two eye blocks, one middle or bottom block, or four clearing blocks.</param>
    /// <returns>The selected metatile/flip word with the horizontal flip toggled for the opening-clear alias; unchanged art derives from compiled stock words.</returns>
    /// <exception cref="InvalidDataException">The pointer does not identify a supported eye-door frame or clearing alias.</exception>
    /// <exception cref="ArgumentOutOfRangeException">The block ordinal is outside the selected component's draw shape.</exception>
    public ushort GetWord(ushort drawPointer, int blockIndex)
    {
        ushort visualSource = EyeDoorPlmDrawDefinitions.VisualSource(drawPointer);
        if (!EyeDoorPlmDrawDefinitions.TryDescribe(visualSource, out var shape))
            throw new InvalidDataException($"Eye-door visuals lack frame ${drawPointer:X4}.");
        if ((uint)blockIndex >= (uint)shape.WordCount)
            throw new ArgumentOutOfRangeException(nameof(blockIndex));
        ushort word = customBlocks is not null && customBlocks.TryGetValue(visualSource, out var words)
            ? words[blockIndex] : new RoomLevelWord(shape.WordAt(blockIndex)).VisualWord;
        return (ushort)(word ^ (drawPointer != visualSource ? (ushort)LevelBlockFlipFlags.Horizontal : 0));
    }
}
