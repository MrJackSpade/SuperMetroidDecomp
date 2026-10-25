using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Frontend.Intro;

/// <summary>Verification access: the IntroCeresFlightState's pixels through the one published path.</summary>
internal static class IntroCeresFlightStatePixelsAccess
{
    /// <summary>The scene's published pixels.</summary>
    extension(IntroCeresFlightState scene)
    {
        /// <summary>Captures the scene and draws its snapshot with the game's renderer.</summary>
        internal RgbaPixel[] Render() => FrontendScenePixelsAccess.Pixels(scene.CaptureRenderSnapshot());
    }
}
