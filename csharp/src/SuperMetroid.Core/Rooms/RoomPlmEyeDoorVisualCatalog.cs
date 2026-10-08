using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Rooms;

/// <summary>Editable visual blocks for one eye-door draw frame.</summary>
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
