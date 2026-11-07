using SuperMetroid.Core.Rooms.Doors;
using SuperMetroid.Core.Rooms.LevelData;

namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>
/// Presentation-only choices for the mirrored eye, middle and bottom components.
/// Door collision, attack logic, hit counters, timers and persistence stay compiled.
/// </summary>
public sealed class InstalledRoomObjectEyeDoorVisuals
{
    /// <summary>
    /// Copied visual overrides keyed by the eye-door frame's shared source draw;
    /// null keeps the compiled appearance for every component and clearing frame.
    /// </summary>
    private readonly RoomObjectVisualOverrides customBlocks;

    /// <summary>Holds the import-validated appearances of the editable frames that differ from the compiled components.</summary>
    /// <param name="customBlocks">The changed frames' appearances, keyed by each frame's shared source draw.</param>
    internal InstalledRoomObjectEyeDoorVisuals(RoomObjectVisualOverrides customBlocks) =>
        this.customBlocks = Ensure.NotNull(customBlocks);

    /// <summary>Resolves an eye-door visual block, sharing and horizontally mirroring the selected clearing artwork for the opening-clear alias.</summary>
    /// <param name="draw">The draw whose appearance is resolved.</param>
    /// <param name="blockIndex">Zero-based block ordinal within the component's single run: two eye blocks, one middle or bottom block, or four clearing blocks.</param>
    /// <returns>The selected appearance with the horizontal flip toggled for the opening-clear alias; unchanged art derives from compiled stock appearances.</returns>
    /// <exception cref="InvalidDataException">The draw is not a supported eye-door frame or clearing alias.</exception>
    /// <exception cref="ArgumentOutOfRangeException">The block ordinal is outside the selected component's draw shape.</exception>
    internal RoomBlockAppearance GetAppearance(RoomObjectDraw draw, int blockIndex)
    {
        RoomObjectDraw visualSource = EyeDoorDraws.VisualSource(draw);
        if (!EyeDoorDraws.TryDescribe(visualSource, out var shape))
            throw new InvalidDataException($"Eye-door visuals lack frame {draw}.");
        if ((uint)blockIndex >= (uint)shape.BlockCount)
            throw new ArgumentOutOfRangeException(nameof(blockIndex));
        RoomBlockAppearance appearance = customBlocks.TryGet(visualSource, out var appearances)
            ? appearances[blockIndex] : shape.BlockAt(blockIndex).Appearance;
        return draw == visualSource ? appearance : appearance.WithMirroring(appearance.Mirroring ^ BlockMirroring.Horizontal);
    }
}
