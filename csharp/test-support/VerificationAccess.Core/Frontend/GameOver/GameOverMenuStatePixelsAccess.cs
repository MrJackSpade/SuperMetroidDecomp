using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Frontend.GameOver;

/// <summary>Verification access: the GameOverMenuState's pixels through the one published path.</summary>
internal static class GameOverMenuStatePixelsAccess
{
    /// <summary>The scene's published pixels.</summary>
    extension(GameOverMenuState scene)
    {
        /// <summary>Captures the scene and rasterizes its snapshot in software.</summary>
        internal Rgba32[] Render() => FrontendScenePixelsAccess.Pixels(scene.CaptureRenderSnapshot());
    }
}
