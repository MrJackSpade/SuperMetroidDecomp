namespace SuperMetroid.Core.Rooms.Backgrounds;

/// <summary>A background's tilemap words by scroll position: what tile, if any, covers a scrolled pixel.</summary>
public interface IBgTileSource
{
    /// <summary>The tile at a scrolled position; false where the background shows nothing.</summary>
    /// <param name="scrolled">The scroll register plus the screen pixel, wrapped to sixteen bits.</param>
    /// <param name="tile">The tilemap word there.</param>
    bool TryTileAt(Position16<BgPlanePixels> scrolled, out SnesBgTilemapWord tile);
}
