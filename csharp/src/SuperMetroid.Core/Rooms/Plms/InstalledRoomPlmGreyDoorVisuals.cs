using SuperMetroid.Core.Rooms.Doors;
using SuperMetroid.Core.Rooms.LevelData;

namespace SuperMetroid.Core.Rooms.Plms;

/// <summary>
/// Presentation-only selection for twenty grey-door and shared clear-cap frames. The compiled
/// level appearances continue to own collision, animation timing, and door handoff.
/// </summary>
public sealed class InstalledRoomPlmGreyDoorVisuals
{
    /// <summary>Copied visual overrides keyed by draw; unchanged frames use calculated stock appearances.</summary>
    private readonly PlmVisualOverrides customBlocks;

    /// <summary>Holds the import-validated appearances of the frames that differ from the compiled caps.</summary>
    /// <param name="customBlocks">The changed frames' four appearances, keyed by frame.</param>
    internal InstalledRoomPlmGreyDoorVisuals(PlmVisualOverrides customBlocks) =>
        this.customBlocks = Ensure.NotNull(customBlocks);

    /// <summary>Resolves one selected cap block without changing its collision or door mechanics.</summary>
    /// <param name="draw">The draw whose appearance is resolved.</param>
    /// <param name="blockIndex">Zero-based block ordinal 0..3 along the frame's run.</param>
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
