using SuperMetroid.Core.Rooms.LevelData;

namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>
/// The installed pictures of the small reveals and walls room objects draw (#1557): the three bombed reveals, the
/// cleared passage of Crateria's main street escape, the opened wall of the old Tourian escape shaft and the four frames
/// of the escape animals' rescue wall. Their terrain stays the game's; only what each cell shows comes from here.
/// </summary>
public sealed class InstalledRoomObjectRevealAndWallVisuals
{
    /// <summary>The installed pictures of every reveal and wall frame.</summary>
    private readonly RoomObjectFramePictures pictures;

    /// <summary>Holds the installed pictures of every reveal and wall frame.</summary>
    /// <param name="pictures">Each frame's cell pictures in run then cell order, keyed by frame.</param>
    internal InstalledRoomObjectRevealAndWallVisuals(RoomObjectFramePictures pictures) =>
        this.pictures = Ensure.NotNull(pictures);

    /// <summary>The installed picture of cell <paramref name="cell"/> of a reveal or wall frame, counted in run then cell order.</summary>
    /// <param name="draw">The reveal or wall frame.</param>
    /// <param name="cell">The cell, counted across the frame's runs in order.</param>
    /// <returns>The block picture and its mirroring.</returns>
    /// <exception cref="InvalidDataException">The installation holds no such frame.</exception>
    /// <exception cref="ArgumentOutOfRangeException">The frame has no such cell.</exception>
    internal RoomBlockAppearance GetAppearance(RoomObjectDraw draw, int cell) => pictures.At(draw, cell);
}
