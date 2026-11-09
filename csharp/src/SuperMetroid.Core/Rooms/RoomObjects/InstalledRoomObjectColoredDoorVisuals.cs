using SuperMetroid.Core.Rooms.Doors;
using SuperMetroid.Core.Rooms.LevelData;

namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>
/// The installed pictures of the coloured door caps' frames (#1557). The caps' terrain, hits and timing stay the game's.
/// </summary>
public sealed class InstalledRoomObjectColoredDoorVisuals
{
    /// <summary>The installed pictures of every coloured-cap frame.</summary>
    private readonly RoomObjectFramePictures pictures;

    /// <summary>Holds the installed pictures of every coloured-cap frame.</summary>
    /// <param name="pictures">Each coloured-cap frame's four cell pictures, keyed by frame.</param>
    internal InstalledRoomObjectColoredDoorVisuals(RoomObjectFramePictures pictures) =>
        this.pictures = Ensure.NotNull(pictures);

    /// <summary>The installed picture of one cell of a coloured-cap frame.</summary>
    /// <param name="draw">The coloured-cap frame.</param>
    /// <param name="blockIndex">Zero-based block index 0..3 within the cap run, not a room-block coordinate.</param>
    /// <returns>The block picture and its mirroring.</returns>
    /// <exception cref="InvalidDataException">The draw is not a coloured-cap frame, or the installation lacks it.</exception>
    /// <exception cref="ArgumentOutOfRangeException">The block index is outside 0..3.</exception>
    internal RoomBlockAppearance GetAppearance(RoomObjectDraw draw, int blockIndex)
    {
        if (!ColoredDoorCaps.TryDescribe(draw, out _))
            throw new InvalidDataException(
                $"Colored-door visuals lack frame {draw}.");
        if ((uint)blockIndex >= 4)
            throw new ArgumentOutOfRangeException(nameof(blockIndex));
        return pictures.At(draw, blockIndex);
    }
}
