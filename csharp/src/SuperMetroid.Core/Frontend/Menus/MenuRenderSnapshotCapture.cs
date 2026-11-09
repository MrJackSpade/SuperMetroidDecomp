using SuperMetroid.Core.Rendering;

namespace SuperMetroid.Core.Frontend.Menus;

/// <summary>Captures the shared menu compositor without retaining menu state.</summary>
internal static class MenuRenderSnapshotCapture
{
    /// <summary>
    /// Captures a menu screen showing pictures (#1467): its backdrop page, its page scrolled over it, then its sprites, with
    /// the screen's palette, sprite entries and brightness. The page scrolls by an offset in screen pixels and does not
    /// repeat (#1555).
    /// </summary>
    /// <param name="screen">The menu screen captured.</param>
    /// <param name="backdrop">The pictures the backdrop page shows.</param>
    /// <param name="page">The pictures the page shows.</param>
    /// <param name="pageVerticalScroll">The page line shown on the screen's top line.</param>
    /// <param name="brightness">The screen brightness.</param>
    internal static LayeredScene Capture(MenuScreen screen, PicturedPlane backdrop, PicturedPlane page, int pageVerticalScroll, int brightness)
    {
        DisplayLayer[] layers =
        [
            new PicturedPlaneLayer(backdrop, Offset16<PlanePixels>.Zero),
            new PicturedPlaneLayer(page, new Offset16<PlanePixels>(0, pageVerticalScroll)),
            screen.Sprites(),
        ];
        return new(screen.Paint(), layers, brightness);
    }
}
