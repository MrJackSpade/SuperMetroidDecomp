using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Rooms.Doors;
using SuperMetroid.Core.Rooms.LevelData;

namespace SuperMetroid.Core.Rooms.Plms;

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

    /// <summary>Owned visual-word overrides keyed by canonical frame pointer; null means every frame uses stock art.</summary>
    private readonly PlmVisualOverrides customBlocks;

    /// <summary>Validates complete canonical blue-door frame coverage and retains owned copies only for visual words differing from the compiled draw shapes.</summary>
    /// <param name="entries">Exactly one entry for every editable frame, each retaining the native four-block run geometry.</param>
    /// <exception cref="ArgumentNullException">The entry sequence is null.</exception>
    /// <exception cref="InvalidDataException">An entry is null, changes an identity/shape, has invalid visual words, repeats a frame, or leaves coverage incomplete.</exception>
    public RoomPlmBlueDoorVisualCatalog(IEnumerable<RoomPlmVisualEntry> entries)
    {
        customBlocks = new PlmVisualOverrides(entries, "Blue-door", BlueDoorPlmDrawDefinitions.TryGetByVisualId,
            BlueDoorPlmDrawDefinitions.Editable.Count());
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
        return customBlocks.TryGet(BlueDoorPlmDrawDefinitions.VisualSource(drawPointer), out var words)
            ? words[blockIndex] : new RoomLevelWord(draw.WordAt(blockIndex)).VisualWord;
    }
}
