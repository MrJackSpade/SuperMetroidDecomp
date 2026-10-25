using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Frontend.GameOver;

/// <summary>Verification access: the GameOverMenuState's pixels through the one published path.</summary>
internal static class GameOverMenuStatePixelsAccess
{
    /// <summary>The scene's published pixels.</summary>
    extension(GameOverMenuState scene)
    {
        /// <summary>Captures the scene and draws its snapshot with the game's renderer.</summary>
        internal RgbaPixel[] Render() => FrontendScenePixelsAccess.Pixels(scene.CaptureRenderSnapshot());
    }
}
