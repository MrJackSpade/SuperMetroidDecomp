namespace SuperMetroid.Core.Spatial;

/// <summary>
/// Names the plane pixel a gameplay or map position shows at the screen's top-left, for display layers that draw any
/// pictured plane. The coordinates are unchanged; only the space is renamed.
/// </summary>
public static class PlaneConversions
{
    /// <summary>The foreground plane pixel a gameplay foreground scroll shows at the screen's top-left.</summary>
    public static Position16<PlanePixels> AsPlaneScroll(this Position16<RoomPixels> scroll) => new(scroll.X, scroll.Y);

    /// <summary>The background plane pixel a background scroll shows at the screen's top-left.</summary>
    public static Position16<PlanePixels> AsPlaneScroll(this Position16<BackgroundPixels> scroll) => new(scroll.X, scroll.Y);

    /// <summary>
    /// The plane offset an area-map scroll shows at the screen's top-left (pause and file-select map views, #1555): the
    /// map's scroll wraps within the coordinate range, so a scroll past the signed range names a scroll left of or above the map's
    /// origin, and the map, which does not repeat, shows nothing there.
    /// </summary>
    public static Offset16<PlanePixels> AsPlaneOffset(this Position16<MapPixels> scroll) => Offset16<PlanePixels>.Wrapped(scroll.X, scroll.Y);

    /// <summary>
    /// The plane offset a room scroll shows at the screen's top-left, for a pictured plane drawn with the room's camera
    /// (#1555): a scroll past the signed range names a scroll left of or above the plane's origin.
    /// </summary>
    public static Offset16<PlanePixels> AsPlaneOffset(this Position16<RoomPixels> scroll) => Offset16<PlanePixels>.Wrapped(scroll.X, scroll.Y);

}
