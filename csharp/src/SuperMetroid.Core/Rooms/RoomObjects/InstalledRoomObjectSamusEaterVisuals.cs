using SuperMetroid.Core.Rooms.LevelData;

namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>The installed pictures of the Samus Eater plant's frames (#1557); the three-run terrain layout stays the game's.</summary>
public sealed class InstalledRoomObjectSamusEaterVisuals
{
    /// <summary>The installed pictures of every plant frame.</summary>
    private readonly RoomObjectFramePictures pictures;

    /// <summary>Holds the installed pictures of every plant frame.</summary>
    /// <param name="pictures">Each plant frame's cell pictures in run then cell order, keyed by frame.</param>
    internal InstalledRoomObjectSamusEaterVisuals(RoomObjectFramePictures pictures) =>
        this.pictures = Ensure.NotNull(pictures);

    /// <summary>The installed picture of one cell of a plant frame.</summary>
    /// <param name="draw">The plant frame.</param>
    /// <param name="runIndex">Zero-based run ordinal, zero through two.</param>
    /// <param name="blockIndex">Zero-based cell ordinal within that run.</param>
    /// <returns>The block picture and its mirroring.</returns>
    /// <exception cref="InvalidDataException">The draw is not a plant frame, or the installation lacks it.</exception>
    /// <exception cref="ArgumentOutOfRangeException">The run or cell ordinal is outside the frame's shape.</exception>
    internal RoomBlockAppearance GetAppearance(RoomObjectDraw draw, int runIndex, int blockIndex)
    {
        if (!SamusEaterObjectDraws.TryDescribe(draw, out _))
            throw new InvalidDataException(
                $"Samus Eater visuals lack frame {draw}.");
        if ((uint)runIndex >= 3 ||
            (uint)blockIndex >= (uint)SamusEaterObjectDraws.Draw.Count(runIndex))
            throw new ArgumentOutOfRangeException(nameof(blockIndex));
        int flatIndex = blockIndex;
        for (int run = 0; run < runIndex; run++)
            flatIndex += SamusEaterObjectDraws.Draw.Count(run);
        return pictures.At(draw, flatIndex);
    }
}
