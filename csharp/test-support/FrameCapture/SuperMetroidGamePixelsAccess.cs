using SuperMetroid.TestSupport.FrameRendering;
using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Frontend;

/// <summary>Verification access: the game's published frame as pixels.</summary>
internal static class SuperMetroidGamePixelsAccess
{
    /// <summary>The game's published picture.</summary>
    extension(SuperMetroidGame game)
    {
        /// <summary>Draws the last published frame with the game's renderer into a fresh array; call it between steps.</summary>
        internal RgbaPixel[] PublishedPixels() => DisplayRendering.Render(game.GetRetainedDisplay(1, 1));
    }
}
