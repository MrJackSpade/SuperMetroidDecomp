using SuperMetroid.Core.Rendering;

namespace SuperMetroid.Core.Frontend.Map;

/// <summary>Where the map screens' frame opening lies on the screen (#1514): a map shows only through it, its frame's border around it.</summary>
internal static class MapFrameOpening
{
    /// <summary>The screen pixels of the frame's opening while the frame is scrolled by <paramref name="frameScroll"/>.</summary>
    /// <param name="frameScroll">The frame plane's scroll in pixels.</param>
    /// <returns>A window of the one rectangle the opening covers.</returns>
    internal static ScreenWindow Window(Offset16<PlanePixels> frameScroll)
    {
        int left = MapFrameDefinitions.OpeningCorner.X * ScreenGeometry.BackgroundTileSizePixels - frameScroll.X;
        int top = MapFrameDefinitions.OpeningCorner.Y * ScreenGeometry.BackgroundTileSizePixels - frameScroll.Y;
        return ScreenWindow.Of(WindowRectangle.Spanning(left, top,
            left + MapFrameDefinitions.OpeningSize.X * ScreenGeometry.BackgroundTileSizePixels - 1,
            top + MapFrameDefinitions.OpeningSize.Y * ScreenGeometry.BackgroundTileSizePixels - 1));
    }
}
