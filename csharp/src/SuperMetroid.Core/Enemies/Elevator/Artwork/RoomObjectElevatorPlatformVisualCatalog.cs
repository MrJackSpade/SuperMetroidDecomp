using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Rooms.RoomObjects;

namespace SuperMetroid.Core.Enemies.Elevator.Artwork;

/// <summary>
/// The installed pictures of the elevator platform's frames (#1557). The four-step loop, draw geometry and terrain stay
/// the game's.
/// </summary>
public sealed class RoomObjectElevatorPlatformVisualCatalog
{
    /// <summary>The installed pictures of every platform frame.</summary>
    private readonly RoomObjectFramePictures pictures;

    /// <summary>Holds the installed pictures of every platform frame.</summary>
    /// <param name="pictures">Each platform frame's cell pictures in run then cell order, keyed by frame.</param>
    internal RoomObjectElevatorPlatformVisualCatalog(RoomObjectFramePictures pictures) =>
        this.pictures = Ensure.NotNull(pictures);

    /// <summary>The installed picture of one cell of a platform frame.</summary>
    /// <param name="draw">The platform frame.</param>
    /// <param name="runIndex">Zero-based ordinal of the draw run.</param>
    /// <param name="blockIndex">Zero-based block ordinal within that run, not a room coordinate.</param>
    /// <returns>The block picture and its mirroring.</returns>
    /// <exception cref="InvalidDataException">The draw is not a platform frame, or the installation lacks it.</exception>
    /// <exception cref="ArgumentOutOfRangeException">The run or block ordinal is outside the frame's shape.</exception>
    internal RoomBlockAppearance GetAppearance(RoomObjectDraw draw, int runIndex, int blockIndex) =>
        ElevatorPlatformArtwork.Appearance(pictures, draw, runIndex, blockIndex);
}
