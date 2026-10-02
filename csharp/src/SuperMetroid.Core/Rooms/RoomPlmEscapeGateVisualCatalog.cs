using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Rooms;

/// <summary>Editable visual block references for one Mother Brain escape-gate frame.</summary>
public sealed record RoomPlmEscapeGateVisualEntry(string Id, ushort[] Blocks);

/// <summary>
/// Presentation-only selection for the three escape-gate frames. The compiled
/// level words continue to own collision, animation timing, and door handoff.
/// </summary>
public sealed class RoomPlmEscapeGateVisualCatalog
{
    /// <summary>Canonical identity of the selected visual frames, excluding native mechanics.</summary>
    public string ContentIdentity => SelectedPresentationHash.Create(nameof(RoomPlmEscapeGateVisualCatalog), content =>
    {
        Span<ushort> words = stackalloc ushort[4];
        foreach (var frame in MotherBrainEscapeGatePlmDrawDefinitions.All.OrderBy(frame => frame.Pointer))
        {
            for (int row = 0; row < words.Length; row++) words[row] = GetWord(frame.Pointer, row);
            content.Append("frame", frame.Pointer);
            content.Append("runs", 1);
            content.AppendWords("words", words);
        }
    });

    private readonly Dictionary<ushort, ushort[]>? customBlocks;

    private RoomPlmEscapeGateVisualCatalog() { }

    public RoomPlmEscapeGateVisualCatalog(IEnumerable<RoomPlmEscapeGateVisualEntry> entries)
    {
        ArgumentNullException.ThrowIfNull(entries);
        var selected = new Dictionary<ushort, ushort[]>();
        var seen = new HashSet<ushort>();
        foreach (RoomPlmEscapeGateVisualEntry entry in entries)
        {
            if (entry is null || entry.Blocks is null ||
                !MotherBrainEscapeGatePlmDrawDefinitions.TryGetByVisualId(entry.Id, out var draw) ||
                draw.Runs.Length != 1 ||
                entry.Blocks.Length != draw.Runs.Span[0].LevelWords.Length ||
                entry.Blocks.Any(word => !RoomLevelWord.IsValidVisualWord(word)))
                throw new InvalidDataException(
                    "Escape-gate visuals changed a compiled frame identity, draw shape, or visual word.");
            if (!seen.Add(draw.Pointer))
                throw new InvalidDataException($"Escape-gate visuals repeat frame {entry.Id}.");
            MotherBrainEscapeGatePlmDrawDefinitions.TryDescribe(draw.Pointer, out var shape);
            bool changed = false;
            for (int row = 0; row < entry.Blocks.Length; row++)
                changed |= entry.Blocks[row] != new RoomLevelWord(shape.WordAt(row)).VisualWord;
            if (changed) selected.Add(draw.Pointer, entry.Blocks.ToArray());
        }
        if (seen.Count != MotherBrainEscapeGatePlmDrawDefinitions.All.Count())
            throw new InvalidDataException(
                "Escape-gate visuals do not cover all compiled frames.");
        if (selected.Count != 0) customBlocks = selected;
    }

    /// <summary>Calculate stock visuals directly; only selected custom frames need storage.</summary>
    public static RoomPlmEscapeGateVisualCatalog Stock() => new();

    public ushort GetWord(ushort drawPointer, int blockIndex)
    {
        if (!MotherBrainEscapeGatePlmDrawDefinitions.TryDescribe(drawPointer, out var draw))
            throw new InvalidDataException(
                $"Escape-gate visuals lack frame ${drawPointer:X4}.");
        if ((uint)blockIndex >= 4)
            throw new ArgumentOutOfRangeException(nameof(blockIndex));
        return customBlocks is not null && customBlocks.TryGetValue(drawPointer, out var words)
            ? words[blockIndex] : new RoomLevelWord(draw.WordAt(blockIndex)).VisualWord;
    }
}
