using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rendering;

namespace SuperMetroid.Core.Frontend.Pause;

/// <summary>Verification access: the PauseMenuState's pixels through the one published path.</summary>
internal static class PauseMenuStatePixelsAccess
{
    /// <summary>The scene's published pixels.</summary>
    extension(PauseMenuState scene)
    {
        /// <summary>Captures the scene and rasterizes its snapshot in software.</summary>
        internal Rgba32[] Render() => FrontendScenePixelsAccess.Pixels(scene.CaptureRenderSnapshot());
    }
}
