using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Frontend.Options;

/// <summary>Verification access: the GameOptionsMenuState's pixels through the one published path.</summary>
internal static class GameOptionsMenuStatePixelsAccess
{
    /// <summary>The scene's published pixels.</summary>
    extension(GameOptionsMenuState scene)
    {
        /// <summary>Captures the scene and rasterizes its snapshot in software.</summary>
        internal Rgba32[] Render() => FrontendScenePixelsAccess.Pixels(scene.CaptureRenderSnapshot());
    }
}
