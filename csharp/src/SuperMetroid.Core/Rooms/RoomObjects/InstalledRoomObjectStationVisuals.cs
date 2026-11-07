using SuperMetroid.Core.Rooms.LevelData;

namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>
/// Complete station presentation selection. Station
/// animation, activation, rewards, save behavior and collision stay compiled.
/// </summary>
public sealed class InstalledRoomObjectStationVisuals
{
    /// <summary>Deep-copied visual overrides keyed by draw; absent frames use stock artwork.</summary>
    private readonly IReadOnlyDictionary<RoomObjectDraw, RoomBlockAppearance[][]> customBlocks;

    /// <summary>Holds the import-validated run appearances of the frames that differ from the compiled frames.</summary>
    /// <param name="customBlocks">The changed frames' appearances run by run, keyed by draw.</param>
    internal InstalledRoomObjectStationVisuals(IReadOnlyDictionary<RoomObjectDraw, RoomBlockAppearance[][]> customBlocks) =>
        this.customBlocks = Ensure.NotNull(customBlocks);

    /// <summary>Resolves a station cell's selected appearance, using compiled appearance for an unchanged frame.</summary>
    /// <param name="draw">The draw whose appearance is resolved.</param>
    /// <param name="runIndex">Zero-based horizontal draw-run ordinal; geometry is frame-specific, with save-pod runs ordered from floor to cap rather than screen-top downward.</param>
    /// <param name="blockIndex">Zero-based left-to-right cell within the selected run's compiled length.</param>
    /// <returns>The twelve-bit metatile/parent-flip reference, without the separately applied physical collision nibble.</returns>
    /// <exception cref="InvalidDataException"><paramref name="draw"/> does not identify a compiled station frame.</exception>
    /// <exception cref="ArgumentOutOfRangeException">The run or cell ordinal is outside that frame's shape; the reported parameter is <paramref name="blockIndex"/>.</exception>
    internal RoomBlockAppearance GetAppearance(RoomObjectDraw draw, int runIndex, int blockIndex)
    {
        if (!StationObjectDraws.TryDescribe(draw, out var shape))
            throw new InvalidDataException(
                $"Station visuals lack frame {draw}.");
        if ((uint)runIndex >= (uint)shape.RunCount ||
            (uint)blockIndex >= (uint)shape.BlockCount(runIndex))
            throw new ArgumentOutOfRangeException(nameof(blockIndex));
        return customBlocks.TryGetValue(draw, out var runs)
            ? runs[runIndex][blockIndex] : shape.BlockAt(runIndex, blockIndex).Appearance;
    }
}
