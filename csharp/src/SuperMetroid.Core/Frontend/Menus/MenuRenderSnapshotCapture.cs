using SuperMetroid.Core.Rendering;

namespace SuperMetroid.Core.Frontend.Menus;

/// <summary>Captures the shared menu compositor without retaining menu state.</summary>
internal static class MenuRenderSnapshotCapture
{
    /// <summary>
    /// Captures a menu screen showing pictures (#1467): its backdrop page, its page scrolled over it, then its sprites, with
    /// the screen's palette, sprite entries and brightness.
    /// </summary>
    /// <param name="screen">The menu screen captured.</param>
    /// <param name="backdrop">The pictures the backdrop page shows.</param>
    /// <param name="page">The pictures the page shows.</param>
    /// <param name="pageVerticalScroll">The page's vertical scroll; only its low sixteen bits position the page.</param>
    /// <param name="brightness">The screen brightness.</param>
    internal static LayeredScene Capture(MenuScreen screen, PicturedPlane backdrop, PicturedPlane page, int pageVerticalScroll, int brightness)
    {
        DisplayLayer[] layers =
        [
            new PicturedPlaneLayer(backdrop, default, null),
            new PicturedPlaneLayer(page, new Position16<TilePlanePixels>(0, unchecked((ushort)pageVerticalScroll)), null),
            new SpritesLayer(),
        ];
        return new(screen.Paint(), layers, brightness);
    }
}
