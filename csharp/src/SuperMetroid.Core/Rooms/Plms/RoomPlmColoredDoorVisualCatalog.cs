using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Rooms.Doors;
using SuperMetroid.Core.Rooms.LevelData;

namespace SuperMetroid.Core.Rooms.Plms;

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

    /// <summary>Copied artwork overrides keyed by compiled door-frame pointer; omitted frames use stock visual words.</summary>
    private readonly PlmVisualOverrides customBlocks;

    /// <summary>Validates complete coverage of the forty-eight colored-door frames and copies visual differences while preserving compiled cap geometry and opening mechanics.</summary>
    /// <param name="entries">Exactly one four-word, visual-only entry for each compiled color, orientation, and frame identity.</param>
    /// <exception cref="ArgumentNullException">The entry sequence is null.</exception>
    /// <exception cref="InvalidDataException">An entry is invalid, repeats an identity, changes a draw shape, or leaves frame coverage incomplete.</exception>
    public RoomPlmColoredDoorVisualCatalog(IEnumerable<RoomPlmVisualEntry> entries)
    {
        customBlocks = new PlmVisualOverrides(entries, "Colored-door", ColoredDoorPlmDrawDefinitions.TryGetByVisualId,
            ColoredDoorPlmDrawDefinitions.All.Count());
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
        return customBlocks.TryGet(drawPointer, out var words)
            ? words[blockIndex] : new RoomLevelWord(draw.WordAt(blockIndex)).VisualWord;
    }
}
