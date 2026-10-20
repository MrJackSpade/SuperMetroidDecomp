using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Frontend.Intro;

/// <summary>Verification access: the IntroCeresFlightState's pixels through the one published path.</summary>
internal static class IntroCeresFlightStatePixelsAccess
{
    /// <summary>The scene's published pixels.</summary>
    extension(IntroCeresFlightState scene)
    {
        /// <summary>Captures the scene and rasterizes its snapshot in software.</summary>
        internal Rgba32[] Render() => FrontendScenePixelsAccess.Pixels(scene.CaptureRenderSnapshot());
    }
}
