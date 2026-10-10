using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Rooms;

/// <summary>Visual-only blocks for a grey-door cap or shared door-clear frame.</summary>
/// <param name="Id">Compiled clear or grey-door frame identity, including orientation.</param>
/// <param name="Blocks">Four metatile/flip words in native run order; changed arrays are copied by the catalog.</param>
public sealed record RoomPlmGreyDoorVisualEntry(string Id, ushort[] Blocks);

/// <summary>
/// Presentation-only selection for twenty grey-door and shared clear-cap frames. The compiled
/// level words continue to own collision, animation timing, and door handoff.
/// </summary>
public sealed class RoomPlmGreyDoorVisualCatalog
{
    /// <summary>Canonical identity of the selected visual frames, excluding native mechanics.</summary>
    public string ContentIdentity => SelectedPresentationHash.Create(nameof(RoomPlmGreyDoorVisualCatalog), content =>
    {
        Span<ushort> words = stackalloc ushort[4];
        foreach (var frame in GreyDoorPlmDrawDefinitions.All.OrderBy(frame => frame.Pointer))
        {
            for (int row = 0; row < words.Length; row++) words[row] = GetWord(frame.Pointer, row);
            content.Append("frame", frame.Pointer);
            content.Append("runs", 1);
            content.AppendWords("words", words);
        }
    });

    /// <summary>Visual overrides keyed by draw pointer; null means every frame uses its compiled stock words.</summary>
    private readonly Dictionary<ushort, ushort[]>? customBlocks;

    /// <summary>Validates all twenty clear/grey-door frames and copies visual differences while retaining compiled collision, run direction, animation timing, and handoff behavior.</summary>
    /// <param name="entries">Exactly one four-word visual-only entry for each compiled orientation and frame identity.</param>
    /// <exception cref="ArgumentNullException">The entry sequence is null.</exception>
    /// <exception cref="InvalidDataException">An entry is invalid, repeats an identity, changes a draw shape, or leaves coverage incomplete.</exception>
    public RoomPlmGreyDoorVisualCatalog(IEnumerable<RoomPlmGreyDoorVisualEntry> entries)
    {
        ArgumentNullException.ThrowIfNull(entries);
        var selected = new Dictionary<ushort, ushort[]>();
        var seen = new HashSet<ushort>();
        foreach (RoomPlmGreyDoorVisualEntry entry in entries)
        {
            if (entry is null || entry.Blocks is null ||
                !GreyDoorPlmDrawDefinitions.TryGetByVisualId(entry.Id, out var draw) ||
                draw.Runs.Length != 1 ||
                entry.Blocks.Length != draw.Runs.Span[0].LevelWords.Length ||
                entry.Blocks.Any(word => !RoomLevelWord.IsValidVisualWord(word)))
                throw new InvalidDataException(
                    "Grey-door visuals changed a compiled frame identity, draw shape, or visual word.");
            if (!seen.Add(draw.Pointer))
                throw new InvalidDataException($"Grey-door visuals repeat frame {entry.Id}.");
            GreyDoorPlmDrawDefinitions.TryDescribe(draw.Pointer, out var shape);
            bool changed = false;
            for (int row = 0; row < entry.Blocks.Length; row++)
                changed |= entry.Blocks[row] != new RoomLevelWord(shape.WordAt(row)).VisualWord;
            if (changed) selected.Add(draw.Pointer, entry.Blocks.ToArray());
        }
        if (seen.Count != GreyDoorPlmDrawDefinitions.All.Count())
            throw new InvalidDataException(
                "Grey-door visuals do not cover all compiled frames.");
        if (selected.Count != 0) customBlocks = selected;
    }

    /// <summary>Resolves one selected cap block without changing its physical collision word or door mechanics.</summary>
    /// <param name="drawPointer">Bank-$84 identity of a supported clear or grey-door draw frame.</param>
    /// <param name="blockIndex">Zero-based word ordinal 0..3 along the frame's native run.</param>
    /// <returns>The authored metatile/flip word, or the compiled stock word's visual bits when unchanged.</returns>
    /// <exception cref="InvalidDataException">The pointer does not identify a supported frame.</exception>
    /// <exception cref="ArgumentOutOfRangeException">The block ordinal is outside 0..3.</exception>
    public ushort GetWord(ushort drawPointer, int blockIndex)
    {
        if (!GreyDoorPlmDrawDefinitions.TryDescribe(drawPointer, out var draw))
            throw new InvalidDataException(
                $"Grey-door visuals lack frame ${drawPointer:X4}.");
        if ((uint)blockIndex >= 4)
            throw new ArgumentOutOfRangeException(nameof(blockIndex));
        return customBlocks is not null && customBlocks.TryGetValue(drawPointer, out var words)
            ? words[blockIndex] : new RoomLevelWord(draw.WordAt(blockIndex)).VisualWord;
    }
}
