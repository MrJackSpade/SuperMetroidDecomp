namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>
/// The installed pictures of the small reveals and walls room objects draw (#1557): the three bombed reveals, the
/// cleared passage of Crateria's main street escape, the opened wall of the old Tourian escape shaft and the four frames
/// of the escape animals' rescue wall. Their terrain stays the game's; only what each cell shows comes from here.
/// </summary>
public sealed class InstalledRoomObjectRevealAndWallVisuals
{
    /// <summary>Holds the pictures the import made of every reveal and wall frame.</summary>
    /// <param name="images">Every reveal and wall frame's picture, keyed by its draw.</param>
    internal InstalledRoomObjectRevealAndWallVisuals(RoomObjectFrameImages<RoomObjectDraw> images) => Images = Ensure.NotNull(images);

    /// <summary>Every reveal and wall frame's picture, keyed by its draw.</summary>
    internal RoomObjectFrameImages<RoomObjectDraw> Images { get; }

    /// <summary>The picture <paramref name="draw"/> shows.</summary>
    /// <exception cref="InvalidDataException">The installation holds no picture of the draw.</exception>
    internal RoomObjectFrameImage Image(RoomObjectDraw draw) => Images.Image(draw);
}
