namespace SuperMetroid.Core.Spatial;

/// <summary>
/// Names the scroll-register words a gameplay layer position programs, for generic render layers
/// that sample any background plane. The words are unchanged; only the space is re-branded.
/// </summary>
public static class BgPlaneConversions
{
    /// <summary>The foreground's scroll words of a gameplay foreground scroll.</summary>
    public static Position16<TilePlanePixels> AsBgPlaneScroll(this Position16<RoomPixels> scroll) => new(scroll.X, scroll.Y);

    /// <summary>The background's scroll words of a background scroll.</summary>
    public static Position16<TilePlanePixels> AsBgPlaneScroll(this Position16<BackgroundPixels> scroll) => new(scroll.X, scroll.Y);

    /// <summary>A foreground scroll offset in scroll-register space.</summary>
    public static Offset16<TilePlanePixels> AsBgPlaneOffset(this Offset16<RoomPixels> offset) => new(offset.X, offset.Y);

    /// <summary>A background scroll offset in scroll-register space.</summary>
    public static Offset16<TilePlanePixels> AsBgPlaneOffset(this Offset16<BackgroundPixels> offset) => new(offset.X, offset.Y);

    /// <summary>The foreground's scroll words of an area-map scroll (pause and file-select map views).</summary>
    public static Position16<TilePlanePixels> AsBgPlaneScroll(this Position16<MapPixels> scroll) => new(scroll.X, scroll.Y);
}
