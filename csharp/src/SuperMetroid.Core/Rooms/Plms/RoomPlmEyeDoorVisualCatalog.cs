using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Rooms.Doors;
using SuperMetroid.Core.Rooms.LevelData;

namespace SuperMetroid.Core.Rooms.Plms;

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
        foreach (var frame in EyeDoorPlmDrawDefinitions.Editable.OrderBy(item => EyeDoorPlmDrawDefinitions.VisualId(item.Draw), StringComparer.Ordinal))
        {
            EyeDoorPlmDrawDefinitions.TryDescribe(frame.Draw, out var shape);
            content.AppendName("frame", EyeDoorPlmDrawDefinitions.VisualId(frame.Draw));
            content.Append("runs", 1);
            for (int cell = 0; cell < shape.WordCount; cell++) buffer[cell] = GetWord(frame.Draw, cell);
            content.AppendWords("words", buffer[..shape.WordCount]);
        }
    });

    /// <summary>
    /// Copied visual overrides keyed by the eye-door frame's shared source draw;
    /// null keeps the compiled appearance for every component and clearing frame.
    /// </summary>
    private readonly PlmVisualOverrides customBlocks;

    /// <summary>Validates all twenty-three editable eye-door frames and copies visual differences without changing collision, attacks, hit counters, timing, or persistence.</summary>
    /// <remarks>The mirrored opening clear shares the clearing frame's artwork rather than requiring a separate authored entry.</remarks>
    /// <param name="entries">One visual-only entry per compiled editable identity, retaining each eye or component frame's draw shape.</param>
    /// <exception cref="ArgumentNullException">The entry sequence is null.</exception>
    /// <exception cref="InvalidDataException">An entry is invalid, repeats an identity, changes a draw shape, or leaves editable frame coverage incomplete.</exception>
    public RoomPlmEyeDoorVisualCatalog(IEnumerable<RoomPlmVisualEntry> entries)
    {
        customBlocks = new PlmVisualOverrides(entries, "Eye-door", EyeDoorPlmDrawDefinitions.TryGetByVisualId,
            EyeDoorPlmDrawDefinitions.Editable.Count());
    }

    /// <summary>Resolves an eye-door visual block, sharing and horizontally mirroring the selected clearing artwork for the native opening-clear alias.</summary>
    /// <param name="draw">The draw whose visual word is resolved.</param>
    /// <param name="blockIndex">Zero-based word ordinal within the component's single native run: two eye blocks, one middle or bottom block, or four clearing blocks.</param>
    /// <returns>The selected metatile/flip word with the horizontal flip toggled for the opening-clear alias; unchanged art derives from compiled stock words.</returns>
    /// <exception cref="InvalidDataException">The draw is not a supported eye-door frame or clearing alias.</exception>
    /// <exception cref="ArgumentOutOfRangeException">The block ordinal is outside the selected component's draw shape.</exception>
    internal ushort GetWord(PlmDraw draw, int blockIndex)
    {
        PlmDraw visualSource = EyeDoorPlmDrawDefinitions.VisualSource(draw);
        if (!EyeDoorPlmDrawDefinitions.TryDescribe(visualSource, out var shape))
            throw new InvalidDataException($"Eye-door visuals lack frame {draw}.");
        if ((uint)blockIndex >= (uint)shape.WordCount)
            throw new ArgumentOutOfRangeException(nameof(blockIndex));
        int word = customBlocks.TryGet(visualSource, out var words)
            ? words[blockIndex] : new RoomLevelWord(shape.WordAt(blockIndex)).VisualWord;
        return (ushort)(word ^ (draw != visualSource ? LevelBlockFlipFlags.Horizontal.ToLevelWordBits() : 0));
    }
}
