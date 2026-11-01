using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Runtime;
using SuperMetroid.Core.Rendering;
using SuperMetroid.TestSupport.FrameRendering;

namespace SuperMetroid.Core.Frontend;

/// <summary>
/// Verification access: a scene's or gameplay frame's pixels through the one published path, its render snapshot
/// drawn by the game's renderer (Direct3D 11 on the WARP device), for checks that assert on pixels.
/// </summary>
internal static class FrontendScenePixelsAccess
{
    /// <summary>Rasterizes a layered snapshot.</summary>
    internal static RgbaPixel[] Pixels(LayeredScene snapshot) =>
        DisplayRendering.Render(new DisplayPacket(0, snapshot));

    /// <summary>Rasterizes a published gameplay frame.</summary>
    /// <param name="frame">The published frame.</param>
    internal static RgbaPixel[] Pixels(GameplayDisplayFrame frame) => Pixels(GameplayDisplayCapture.Capture(frame));
}
