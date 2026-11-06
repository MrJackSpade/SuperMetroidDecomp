namespace SuperMetroid.Core.Rooms.Backgrounds;

/// <summary>
/// The source room's foreground during a door's opening scroll: it keeps the screen it showed when the door began, now moving
/// with the destination's scroll, and the destination shows everywhere else. The window is in the foreground scroll space, so it
/// covers exactly the scrolled positions the source screen occupied.
/// </summary>
/// <param name="SourceRoom">The source room's foreground with its own scroll offset.</param>
/// <param name="WindowOrigin">The scrolled position of the source screen's top-left gameplay pixel.</param>
public sealed record RoomDoorBackground(RoomBackgroundLayer SourceRoom, Position16<PlanePixels> WindowOrigin)
{
    /// <summary>
    /// The window of a door that began with the foreground's camera at <paramref name="sourceCamera"/>: the source screen's
    /// top-left gameplay pixel is the foreground row the screen showed below the HUD, the layers showing one row below the
    /// camera (<see cref="ScreenGeometry.LayerRowsAboveSprites"/>).
    /// </summary>
    /// <param name="sourceRoom">The source room's foreground.</param>
    /// <param name="sourceCamera">The source room's foreground camera when the door began.</param>
    public static RoomDoorBackground Begin(RoomBackgroundLayer sourceRoom, Position16<PlanePixels> sourceCamera) =>
        new(sourceRoom, sourceCamera + new Offset16<PlanePixels>(0,
            ScreenGeometry.LayerRowsAboveSprites + ScreenGeometry.GameplayHudHeightPixels));
}
