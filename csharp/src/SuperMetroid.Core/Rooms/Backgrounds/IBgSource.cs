namespace SuperMetroid.Core.Rooms.Backgrounds;

/// <summary>A background as a renderer shows it (#1392): what, if anything, shows at a scrolled pixel.</summary>
public interface IBgSource
{
    /// <summary>What shows at a scrolled position; false where the background shows nothing.</summary>
    /// <param name="scrolled">The scroll register plus the screen pixel, wrapped to sixteen bits.</param>
    /// <param name="sample">The tile or block picture pixel shown.</param>
    bool TrySample(Position16<BgPlanePixels> scrolled, out BgSample sample);
}
