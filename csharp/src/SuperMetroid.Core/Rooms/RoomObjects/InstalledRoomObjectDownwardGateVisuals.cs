using SuperMetroid.Core.Rooms.LevelData;

namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>
/// The installed pictures of the downward gate's frames and shot triggers (#1557). Gate terrain, shot filters and
/// animation timing stay the game's.
/// </summary>
public sealed class InstalledRoomObjectDownwardGateVisuals
{
    /// <summary>The installed pictures of every gate frame.</summary>
    private readonly RoomObjectFramePictures pictures;

    /// <summary>Holds the installed pictures of every gate frame.</summary>
    /// <param name="pictures">Each gate frame's cell pictures in run then cell order, keyed by frame.</param>
    internal InstalledRoomObjectDownwardGateVisuals(RoomObjectFramePictures pictures) =>
        this.pictures = Ensure.NotNull(pictures);

    /// <summary>The installed picture of one cell of a gate or shot-trigger frame.</summary>
    /// <param name="draw">The gate frame.</param>
    /// <param name="runIndex">Zero-based ordinal of the draw run.</param>
    /// <param name="blockIndex">Zero-based block ordinal within that run, not a room coordinate.</param>
    /// <returns>The block picture and its mirroring.</returns>
    /// <exception cref="InvalidDataException">The draw is not a gate or shot-trigger frame, or the installation lacks it.</exception>
    /// <exception cref="ArgumentOutOfRangeException">The run or block ordinal is outside the frame's shape.</exception>
    internal RoomBlockAppearance GetAppearance(RoomObjectDraw draw, int runIndex, int blockIndex)
    {
        if (!DownwardGateObjectDraws.TryDescribe(draw, out var shape))
            throw new InvalidDataException($"Downward gate visuals lack frame {draw}.");
        if ((uint)runIndex >= (uint)shape.RunCount ||
            (uint)blockIndex >= (uint)shape.BlockCount(runIndex))
            throw new ArgumentOutOfRangeException(nameof(blockIndex));
        int flatIndex = blockIndex;
        for (int run = 0; run < runIndex; run++)
            flatIndex += shape.BlockCount(run);
        return pictures.At(draw, flatIndex);
    }
}
