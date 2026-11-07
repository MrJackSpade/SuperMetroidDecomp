using SuperMetroid.Core.Rooms.Doors;
using SuperMetroid.Core.Rooms.LevelData;

namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>
/// Presentation-only selection for forty-eight yellow, green and red door-cap frames. The compiled
/// level appearances continue to own collision, animation timing, and door handoff.
/// </summary>
public sealed class InstalledRoomObjectColoredDoorVisuals
{
    /// <summary>Copied artwork overrides keyed by draw; omitted frames use stock visual appearances.</summary>
    private readonly RoomObjectVisualOverrides customBlocks;

    /// <summary>Holds the import-validated appearances of the frames that differ from the compiled caps.</summary>
    /// <param name="customBlocks">The changed frames' four appearances, keyed by frame.</param>
    internal InstalledRoomObjectColoredDoorVisuals(RoomObjectVisualOverrides customBlocks) =>
        this.customBlocks = Ensure.NotNull(customBlocks);

    /// <summary>Resolves one selected cap block without changing the collision or door-opening rules.</summary>
    /// <param name="draw">The draw whose appearance is resolved.</param>
    /// <param name="blockIndex">Zero-based block ordinal, zero through three, along the frame's horizontal or vertical run.</param>
    /// <returns>The authored appearance, or the compiled stock appearance when unchanged.</returns>
    /// <exception cref="InvalidDataException">The draw is not a supported colored-door frame.</exception>
    /// <exception cref="ArgumentOutOfRangeException">The block ordinal is outside the four-block run.</exception>
    internal RoomBlockAppearance GetAppearance(RoomObjectDraw draw, int blockIndex)
    {
        if (!ColoredDoorCaps.TryDescribe(draw, out var shape))
            throw new InvalidDataException(
                $"Colored-door visuals lack frame {draw}.");
        if ((uint)blockIndex >= 4)
            throw new ArgumentOutOfRangeException(nameof(blockIndex));
        return customBlocks.TryGet(draw, out var appearances)
            ? appearances[blockIndex] : shape.BlockAt(blockIndex).Appearance;
    }
}
