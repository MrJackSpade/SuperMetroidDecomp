using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Rooms.Doors;
using SuperMetroid.Core.Rooms.LevelData;

namespace SuperMetroid.Core.Rooms.Plms;

/// <summary>
/// Presentation-only choices for the mirrored eye, middle and bottom components.
/// Door collision, attack logic, hit counters, timers and persistence stay compiled.
/// </summary>
public sealed class InstalledRoomPlmEyeDoorVisuals
{
    /// <summary>Canonical identity of the selected visual frames, excluding gameplay mechanics.</summary>
    public string ContentIdentity => SelectedPresentationHash.Create(nameof(InstalledRoomPlmEyeDoorVisuals), content =>
    {
        Span<RoomBlockAppearance> buffer = stackalloc RoomBlockAppearance[4];
        foreach (var frame in EyeDoorDraws.Editable.OrderBy(item => EyeDoorDraws.VisualId(item.Draw), StringComparer.Ordinal))
        {
            EyeDoorDraws.TryDescribe(frame.Draw, out var shape);
            content.AppendName("frame", EyeDoorDraws.VisualId(frame.Draw));
            content.Append("runs", 1);
            for (int cell = 0; cell < shape.BlockCount; cell++) buffer[cell] = GetAppearance(frame.Draw, cell);
            content.AppendAppearances("blocks", buffer[..shape.BlockCount]);
        }
    });

    /// <summary>
    /// Copied visual overrides keyed by the eye-door frame's shared source draw;
    /// null keeps the compiled appearance for every component and clearing frame.
    /// </summary>
    private readonly PlmVisualOverrides customBlocks;

    /// <summary>Holds the import-validated appearances of the editable frames that differ from the compiled components.</summary>
    /// <param name="customBlocks">The changed frames' appearances, keyed by each frame's shared source draw.</param>
    internal InstalledRoomPlmEyeDoorVisuals(PlmVisualOverrides customBlocks) =>
        this.customBlocks = Ensure.NotNull(customBlocks);

    /// <summary>Resolves an eye-door visual block, sharing and horizontally mirroring the selected clearing artwork for the opening-clear alias.</summary>
    /// <param name="draw">The draw whose appearance is resolved.</param>
    /// <param name="blockIndex">Zero-based block ordinal within the component's single run: two eye blocks, one middle or bottom block, or four clearing blocks.</param>
    /// <returns>The selected appearance with the horizontal flip toggled for the opening-clear alias; unchanged art derives from compiled stock appearances.</returns>
    /// <exception cref="InvalidDataException">The draw is not a supported eye-door frame or clearing alias.</exception>
    /// <exception cref="ArgumentOutOfRangeException">The block ordinal is outside the selected component's draw shape.</exception>
    internal RoomBlockAppearance GetAppearance(PlmDraw draw, int blockIndex)
    {
        PlmDraw visualSource = EyeDoorDraws.VisualSource(draw);
        if (!EyeDoorDraws.TryDescribe(visualSource, out var shape))
            throw new InvalidDataException($"Eye-door visuals lack frame {draw}.");
        if ((uint)blockIndex >= (uint)shape.BlockCount)
            throw new ArgumentOutOfRangeException(nameof(blockIndex));
        RoomBlockAppearance appearance = customBlocks.TryGet(visualSource, out var appearances)
            ? appearances[blockIndex] : shape.BlockAt(blockIndex).Appearance;
        return draw == visualSource ? appearance : appearance.WithMirroring(appearance.Mirroring ^ BlockMirroring.Horizontal);
    }
}
