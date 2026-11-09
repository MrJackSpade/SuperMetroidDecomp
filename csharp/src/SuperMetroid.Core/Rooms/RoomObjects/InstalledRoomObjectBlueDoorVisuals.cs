using SuperMetroid.Core.Rooms.Doors;
using SuperMetroid.Core.Rooms.LevelData;

namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>
/// The installed pictures of the sixteen blue-door frames, four of which a closed cap shows under a second identity
/// (#1557). The caps' terrain, animation timing and door handoff stay the game's.
/// </summary>
public sealed class InstalledRoomObjectBlueDoorVisuals
{
    /// <summary>The installed pictures of every blue-door frame.</summary>
    private readonly RoomObjectFramePictures pictures;

    /// <summary>Holds the installed pictures of every blue-door frame.</summary>
    /// <param name="pictures">Each blue-door frame's four cell pictures, keyed by frame.</param>
    internal InstalledRoomObjectBlueDoorVisuals(RoomObjectFramePictures pictures) =>
        this.pictures = Ensure.NotNull(pictures);

    /// <summary>The installed picture of one cell of a blue-door frame; a closed cap's second identity shows its frame's picture.</summary>
    /// <param name="draw">The blue-door frame.</param>
    /// <param name="blockIndex">Zero-based block index 0..3 within the cap/clear run, not a room-block coordinate.</param>
    /// <returns>The block picture and its mirroring.</returns>
    /// <exception cref="InvalidDataException">The draw is not a blue-door frame, or the installation lacks it.</exception>
    /// <exception cref="ArgumentOutOfRangeException">The block index is outside 0..3.</exception>
    internal RoomBlockAppearance GetAppearance(RoomObjectDraw draw, int blockIndex)
    {
        if (!BlueDoorCaps.TryDescribe(draw, out _))
            throw new InvalidDataException(
                $"Blue-door visuals lack frame {draw}.");
        if ((uint)blockIndex >= 4)
            throw new ArgumentOutOfRangeException(nameof(blockIndex));
        return pictures.At(BlueDoorCaps.VisualSource(draw), blockIndex);
    }
}
