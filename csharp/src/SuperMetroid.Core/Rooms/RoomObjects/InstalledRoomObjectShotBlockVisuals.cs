using SuperMetroid.Core.Rooms.LevelData;

namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>The installed pictures of the 19 ordinary shot-block frames (#1557); their terrain stays the game's.</summary>
public sealed class InstalledRoomObjectShotBlockVisuals
{
    /// <summary>The installed pictures of every shot-block frame.</summary>
    private readonly RoomObjectFramePictures pictures;

    /// <summary>Holds the installed pictures of every shot-block frame.</summary>
    /// <param name="pictures">Each shot-block frame's cell pictures in run then cell order, keyed by frame.</param>
    internal InstalledRoomObjectShotBlockVisuals(RoomObjectFramePictures pictures) =>
        this.pictures = Ensure.NotNull(pictures);

    /// <summary>The installed picture of one cell of a shot-block frame.</summary>
    /// <exception cref="InvalidDataException"><paramref name="draw"/> is not a shot-block frame, or the installation lacks it.</exception>
    /// <exception cref="ArgumentOutOfRangeException">The run or cell ordinal is outside the frame's shape.</exception>
    internal RoomBlockAppearance GetAppearance(RoomObjectDraw draw, int runIndex, int blockIndex)
    {
        if (draw is not RoomObjectFrameDraw<ShotBlockObjectDraws.Draw> { Frame: var shape })
            throw new InvalidDataException($"Shot-block visuals lack draw {draw}.");
        if ((uint)runIndex >= (uint)shape.RunCount ||
            (uint)blockIndex >= (uint)shape.BlocksPerRun)
            throw new ArgumentOutOfRangeException(nameof(blockIndex));
        return pictures.At(draw, runIndex * shape.BlocksPerRun + blockIndex);
    }
}
