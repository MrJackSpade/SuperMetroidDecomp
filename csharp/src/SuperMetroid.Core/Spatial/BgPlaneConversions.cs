namespace SuperMetroid.Core.Spatial;

/// <summary>
/// Names the scroll-register words a gameplay layer position programs, for generic render layers
/// that sample any background plane. The words are unchanged; only the space is re-branded.
/// </summary>
public static class BgPlaneConversions
{
    /// <summary>The BG1HOFS/BG1VOFS words of a gameplay BG1 scroll.</summary>
    public static Position16<BgPlanePixels> AsBgPlaneScroll(this Position16<RoomPixels> scroll) => new(scroll.X, scroll.Y);

    /// <summary>The BG2HOFS/BG2VOFS words of a BG2 scroll.</summary>
    public static Position16<BgPlanePixels> AsBgPlaneScroll(this Position16<Bg2Pixels> scroll) => new(scroll.X, scroll.Y);
}
