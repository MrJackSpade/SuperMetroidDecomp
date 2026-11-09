using SuperMetroid.Core.Rooms.Doors;
using SuperMetroid.Core.Rooms.LevelData;

namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>
/// The installed pictures of the grey caps' frames and their shared clear frames (#1557). The caps' terrain, waits and
/// timing stay the game's.
/// </summary>
public sealed class InstalledRoomObjectGreyDoorVisuals
{
    /// <summary>The installed pictures of every grey-cap frame.</summary>
    private readonly RoomObjectFramePictures pictures;

    /// <summary>Holds the installed pictures of every grey-cap frame.</summary>
    /// <param name="pictures">Each grey-cap frame's four cell pictures, keyed by frame.</param>
    internal InstalledRoomObjectGreyDoorVisuals(RoomObjectFramePictures pictures) =>
        this.pictures = Ensure.NotNull(pictures);

    /// <summary>The installed picture of one cell of a grey-cap frame.</summary>
    /// <param name="draw">The grey-cap frame.</param>
    /// <param name="blockIndex">Zero-based block index 0..3 within the cap run, not a room-block coordinate.</param>
    /// <returns>The block picture and its mirroring.</returns>
    /// <exception cref="InvalidDataException">The draw is not a grey-cap frame, or the installation lacks it.</exception>
    /// <exception cref="ArgumentOutOfRangeException">The block index is outside 0..3.</exception>
    internal RoomBlockAppearance GetAppearance(RoomObjectDraw draw, int blockIndex)
    {
        if (!GreyDoorCaps.TryDescribe(draw, out _))
            throw new InvalidDataException(
                $"Grey-door visuals lack frame {draw}.");
        if ((uint)blockIndex >= 4)
            throw new ArgumentOutOfRangeException(nameof(blockIndex));
        return pictures.At(draw, blockIndex);
    }
}
