using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Rooms.Doors;
using SuperMetroid.Core.Rooms.LevelData;

namespace SuperMetroid.Core.Rooms.Plms;

/// <summary>
/// Presentation-only selection for sixteen blue-door frames with four physical aliases. The compiled
/// level appearances continue to own collision, animation timing, and door handoff.
/// </summary>
public sealed class InstalledRoomPlmBlueDoorVisuals
{
    /// <summary>Canonical identity of the selected visual frames, excluding native mechanics.</summary>
    public string ContentIdentity => SelectedPresentationHash.Create(nameof(InstalledRoomPlmBlueDoorVisuals), content =>
    {
        Span<RoomBlockAppearance> appearances = stackalloc RoomBlockAppearance[4];
        foreach (var frame in BlueDoorCaps.Editable.OrderBy(item => BlueDoorCaps.VisualId(item.Draw), StringComparer.Ordinal))
        {
            for (int row = 0; row < appearances.Length; row++) appearances[row] = GetAppearance(frame.Draw, row);
            content.AppendName("frame", BlueDoorCaps.VisualId(frame.Draw));
            content.Append("runs", 1);
            content.AppendAppearances("blocks", appearances);
        }
    });

    /// <summary>Owned appearance overrides keyed by draw; null means every frame uses stock art.</summary>
    private readonly PlmVisualOverrides customBlocks;

    /// <summary>Validates complete canonical blue-door frame coverage and retains owned copies only for visual appearances differing from the compiled draw shapes.</summary>
    /// <param name="entries">Exactly one entry for every editable frame, each retaining the native four-block run geometry.</param>
    /// <exception cref="ArgumentNullException">The entry sequence is null.</exception>
    /// <exception cref="InvalidDataException">An entry is null, changes an identity/shape, has invalid visual appearances, repeats a frame, or leaves coverage incomplete.</exception>
    public InstalledRoomPlmBlueDoorVisuals(IEnumerable<RoomPlmVisualEntry> entries)
    {
        customBlocks = new PlmVisualOverrides(entries, "Blue-door", BlueDoorCaps.TryGetByVisualId,
            BlueDoorCaps.Editable.Count());
    }

    /// <summary>Resolves a selected blue-door appearance, normalizing physical alias pointers to their shared editable frame without changing collision or door timing.</summary>
    /// <param name="draw">The draw whose appearance is resolved.</param>
    /// <param name="blockIndex">Zero-based block index 0..3 within the native cap/clear run, not a room-block coordinate.</param>
    /// <returns>The visual metatile reference and flips, with physical collision bits excluded.</returns>
    /// <exception cref="InvalidDataException">The draw is not a supported blue-door frame.</exception>
    /// <exception cref="ArgumentOutOfRangeException">The block index is outside 0..3.</exception>
    internal RoomBlockAppearance GetAppearance(PlmDraw draw, int blockIndex)
    {
        if (!BlueDoorCaps.TryDescribe(draw, out var shape))
            throw new InvalidDataException(
                $"Blue-door visuals lack frame {draw}.");
        if ((uint)blockIndex >= 4)
            throw new ArgumentOutOfRangeException(nameof(blockIndex));
        return customBlocks.TryGet(BlueDoorCaps.VisualSource(draw), out var appearances)
            ? appearances[blockIndex] : shape.BlockAt(blockIndex).Appearance;
    }
}
