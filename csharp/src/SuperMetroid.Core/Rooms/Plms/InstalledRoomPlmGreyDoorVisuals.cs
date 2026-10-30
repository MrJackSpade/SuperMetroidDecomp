using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Rooms.Doors;
using SuperMetroid.Core.Rooms.LevelData;

namespace SuperMetroid.Core.Rooms.Plms;

/// <summary>
/// Presentation-only selection for twenty grey-door and shared clear-cap frames. The compiled
/// level appearances continue to own collision, animation timing, and door handoff.
/// </summary>
public sealed class InstalledRoomPlmGreyDoorVisuals
{
    /// <summary>Canonical identity of the selected visual frames, excluding native mechanics.</summary>
    public string ContentIdentity => SelectedPresentationHash.Create(nameof(InstalledRoomPlmGreyDoorVisuals), content =>
    {
        Span<RoomBlockAppearance> appearances = stackalloc RoomBlockAppearance[4];
        foreach (var frame in GreyDoorCaps.All.OrderBy(item => GreyDoorCaps.VisualId(item.Draw), StringComparer.Ordinal))
        {
            for (int row = 0; row < appearances.Length; row++) appearances[row] = GetAppearance(frame.Draw, row);
            content.AppendName("frame", GreyDoorCaps.VisualId(frame.Draw));
            content.Append("runs", 1);
            content.AppendAppearances("blocks", appearances);
        }
    });

    /// <summary>Copied visual overrides keyed by draw; unchanged frames use calculated stock appearances.</summary>
    private readonly PlmVisualOverrides customBlocks;

    /// <summary>Validates all twenty clear/grey-door frames and copies visual differences while retaining compiled collision, run direction, animation timing, and handoff behavior.</summary>
    /// <param name="entries">Exactly one four-block visual-only entry for each compiled orientation and frame identity.</param>
    /// <exception cref="ArgumentNullException">The entry sequence is null.</exception>
    /// <exception cref="InvalidDataException">An entry is invalid, repeats an identity, changes a draw shape, or leaves coverage incomplete.</exception>
    public InstalledRoomPlmGreyDoorVisuals(IEnumerable<RoomPlmVisualEntry> entries)
    {
        customBlocks = new PlmVisualOverrides(entries, "Grey-door", GreyDoorCaps.TryGetByVisualId,
            GreyDoorCaps.All.Count());
    }

    /// <summary>Resolves one selected cap block without changing its collision or door mechanics.</summary>
    /// <param name="draw">The draw whose appearance is resolved.</param>
    /// <param name="blockIndex">Zero-based block ordinal 0..3 along the frame's native run.</param>
    /// <returns>The authored appearance, or the compiled stock appearance when unchanged.</returns>
    /// <exception cref="InvalidDataException">The draw is not a supported frame.</exception>
    /// <exception cref="ArgumentOutOfRangeException">The block ordinal is outside 0..3.</exception>
    internal RoomBlockAppearance GetAppearance(PlmDraw draw, int blockIndex)
    {
        if (!GreyDoorCaps.TryDescribe(draw, out var shape))
            throw new InvalidDataException(
                $"Grey-door visuals lack frame {draw}.");
        if ((uint)blockIndex >= 4)
            throw new ArgumentOutOfRangeException(nameof(blockIndex));
        return customBlocks.TryGet(draw, out var appearances)
            ? appearances[blockIndex] : shape.BlockAt(blockIndex).Appearance;
    }
}
