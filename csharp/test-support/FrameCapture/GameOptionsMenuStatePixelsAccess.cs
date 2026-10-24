using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Frontend.Options;

/// <summary>Verification access: the GameOptionsMenuState's pixels through the one published path.</summary>
internal static class GameOptionsMenuStatePixelsAccess
{
    /// <summary>The scene's published pixels.</summary>
    extension(GameOptionsMenuState scene)
    {
        /// <summary>Captures the scene and draws its snapshot with the game's renderer.</summary>
        internal Rgba32[] Render() => FrontendScenePixelsAccess.Pixels(scene.CaptureRenderSnapshot());
    }
}
