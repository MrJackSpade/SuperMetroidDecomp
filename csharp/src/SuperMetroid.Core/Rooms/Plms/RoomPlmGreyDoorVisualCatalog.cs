using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Rooms.Doors;
using SuperMetroid.Core.Rooms.LevelData;

namespace SuperMetroid.Core.Rooms.Plms;

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

    /// <summary>Copied visual overrides keyed by compiled draw pointer; unchanged frames use calculated stock words.</summary>
    private readonly PlmVisualOverrides customBlocks;

    /// <summary>Validates all twenty clear/grey-door frames and copies visual differences while retaining compiled collision, run direction, animation timing, and handoff behavior.</summary>
    /// <param name="entries">Exactly one four-word visual-only entry for each compiled orientation and frame identity.</param>
    /// <exception cref="ArgumentNullException">The entry sequence is null.</exception>
    /// <exception cref="InvalidDataException">An entry is invalid, repeats an identity, changes a draw shape, or leaves coverage incomplete.</exception>
    public RoomPlmGreyDoorVisualCatalog(IEnumerable<RoomPlmVisualEntry> entries)
    {
        customBlocks = new PlmVisualOverrides(entries, "Grey-door", GreyDoorPlmDrawDefinitions.TryGetByVisualId,
            GreyDoorPlmDrawDefinitions.All.Count());
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
        return customBlocks.TryGet(drawPointer, out var words)
            ? words[blockIndex] : new RoomLevelWord(draw.WordAt(blockIndex)).VisualWord;
    }
}
