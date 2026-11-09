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

    /// <summary>The plane pixel an area-map scroll shows at the screen's top-left (pause and file-select map views).</summary>
    public static Position16<PlanePixels> AsPlaneScroll(this Position16<MapPixels> scroll) => new(scroll.X, scroll.Y);
}
