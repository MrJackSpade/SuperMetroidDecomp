using SuperMetroid.Core.Rooms.LevelData;

namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>The installed pictures of the n00b tube's frames (#1557); their terrain and crack sequence stay the game's.</summary>
public sealed class InstalledRoomObjectNoobTubeVisuals
{
    /// <summary>The installed pictures of every tube frame.</summary>
    private readonly RoomObjectFramePictures pictures;

    /// <summary>Holds the installed pictures of every tube frame.</summary>
    /// <param name="pictures">Each tube frame's cell pictures in run then cell order, keyed by frame.</param>
    internal InstalledRoomObjectNoobTubeVisuals(RoomObjectFramePictures pictures) =>
        this.pictures = Ensure.NotNull(pictures);

    /// <summary>The installed picture of one cell of a tube frame.</summary>
    /// <param name="draw">The tube frame.</param>
    /// <param name="runIndex">Zero-based draw-run ordinal.</param>
    /// <param name="blockIndex">Zero-based cell ordinal within that run.</param>
    /// <returns>The block picture and its mirroring.</returns>
    /// <exception cref="InvalidDataException">The draw is not a tube frame, or the installation lacks it.</exception>
    /// <exception cref="ArgumentOutOfRangeException">The run or cell ordinal is outside the frame's shape.</exception>
    internal RoomBlockAppearance GetAppearance(RoomObjectDraw draw, int runIndex, int blockIndex)
    {
        if (!NoobTubeObjectDraws.TryDescribe(draw, out var shape))
            throw new InvalidDataException(
                $"N00b tube visuals lack frame {draw}.");
        if ((uint)runIndex >= (uint)shape.RunCount ||
            (uint)blockIndex >= (uint)shape.BlockCount(runIndex))
            throw new ArgumentOutOfRangeException(nameof(blockIndex));
        int flatIndex = blockIndex;
        for (int run = 0; run < runIndex; run++)
            flatIndex += shape.BlockCount(run);
        return pictures.At(draw, flatIndex);
    }
}
