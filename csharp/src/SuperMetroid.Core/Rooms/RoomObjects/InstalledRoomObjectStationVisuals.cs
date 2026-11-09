using SuperMetroid.Core.Rooms.LevelData;

namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>
/// The installed pictures of the station frames (#1557). Station animation, activation, rewards, save behaviour and
/// terrain stay the game's.
/// </summary>
public sealed class InstalledRoomObjectStationVisuals
{
    /// <summary>The installed pictures of every station frame.</summary>
    private readonly RoomObjectFramePictures pictures;

    /// <summary>Holds the installed pictures of every station frame.</summary>
    /// <param name="pictures">Each station frame's cell pictures in run then cell order, keyed by frame.</param>
    internal InstalledRoomObjectStationVisuals(RoomObjectFramePictures pictures) =>
        this.pictures = Ensure.NotNull(pictures);

    /// <summary>The installed picture of one cell of a station frame.</summary>
    /// <param name="draw">The station frame.</param>
    /// <param name="runIndex">Zero-based horizontal draw-run ordinal; geometry is frame-specific, with save-pod runs ordered from floor to cap rather than screen-top downward.</param>
    /// <param name="blockIndex">Zero-based left-to-right cell within the selected run.</param>
    /// <returns>The block picture and its mirroring.</returns>
    /// <exception cref="InvalidDataException"><paramref name="draw"/> is not a station frame, or the installation lacks it.</exception>
    /// <exception cref="ArgumentOutOfRangeException">The run or cell ordinal is outside that frame's shape; the reported parameter is <paramref name="blockIndex"/>.</exception>
    internal RoomBlockAppearance GetAppearance(RoomObjectDraw draw, int runIndex, int blockIndex)
    {
        if (!StationObjectDraws.TryDescribe(draw, out var shape))
            throw new InvalidDataException(
                $"Station visuals lack frame {draw}.");
        if ((uint)runIndex >= (uint)shape.RunCount ||
            (uint)blockIndex >= (uint)shape.BlockCount(runIndex))
            throw new ArgumentOutOfRangeException(nameof(blockIndex));
        int flatIndex = blockIndex;
        for (int run = 0; run < runIndex; run++)
            flatIndex += shape.BlockCount(run);
        return pictures.At(draw, flatIndex);
    }
}
