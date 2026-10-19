namespace SuperMetroid.Core.Rooms.Backgrounds;

/// <summary>A background a renderer samples by scroll position: what tile, if any, shows at a scrolled pixel.</summary>
public interface IBgTileSource
{
    /// <summary>The tile shown at a scrolled position; false where the background shows nothing.</summary>
    /// <param name="scrolled">The scroll register plus the screen pixel, wrapped to sixteen bits.</param>
    /// <param name="sample">The tile and the pixel within it.</param>
    bool TrySample(Position16<BgPlanePixels> scrolled, out BgTileSample sample);
}
