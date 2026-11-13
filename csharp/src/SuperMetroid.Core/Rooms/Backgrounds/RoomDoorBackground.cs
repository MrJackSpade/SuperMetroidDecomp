namespace SuperMetroid.Core.Rooms.Backgrounds;

/// <summary>
/// The source room's foreground during a door's opening scroll (#1555): it keeps the screen it showed when the door began,
/// lying at a fixed place of the destination's foreground and so moving with the destination's scroll, and the destination
/// shows everywhere else.
/// </summary>
/// <param name="SourceRoom">The source room's foreground.</param>
/// <param name="WindowOrigin">The destination's foreground pixel the source screen's top-left gameplay pixel lies on.</param>
/// <param name="SourceOrigin">The source room's foreground pixel the source screen's top-left gameplay pixel shows.</param>
public sealed record RoomDoorBackground(RoomBackgroundLayer SourceRoom, Position16<PlanePixels> WindowOrigin, Position16<PlanePixels> SourceOrigin)
{
    /// <summary>
    /// The source screen of a door that began with the source room's foreground view at <paramref name="sourceView"/>,
    /// lying where the destination's foreground view begins the door's scroll, <paramref name="destinationView"/>: the
    /// screen's top-left gameplay pixel is the foreground row the screen showed below the HUD, below the view row the
    /// screen hides (<see cref="ScreenGeometry.LayerRowsHiddenAboveScreen"/>). A downward door's setup keeps the source's
    /// view one row lower than the destination's, so the source screen lies one row higher on the destination.
    /// </summary>
    /// <param name="sourceRoom">The source room's foreground.</param>
    /// <param name="sourceView">The source room's foreground view when the door began.</param>
    /// <param name="destinationView">The destination's foreground view when the door's scroll begins.</param>
    /// <param name="downward">Whether the door leads down.</param>
    public static RoomDoorBackground Begin(RoomBackgroundLayer sourceRoom, Position16<PlanePixels> sourceView,
        Position16<PlanePixels> destinationView, bool downward)
    {
        var belowHud = new Offset16<PlanePixels>(0, ScreenGeometry.LayerRowsHiddenAboveScreen +ScreenGeometry.GameplayHudHeightPixels);
        return new(sourceRoom, destinationView + belowHud - new Offset16<PlanePixels>(0, downward ? 1 : 0), sourceView + belowHud);
    }
}
