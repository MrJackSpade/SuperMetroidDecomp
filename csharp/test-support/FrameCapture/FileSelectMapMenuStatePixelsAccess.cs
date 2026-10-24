using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Frontend.FileSelect;

/// <summary>Verification access: the FileSelectMapMenuState's pixels through the one published path.</summary>
internal static class FileSelectMapMenuStatePixelsAccess
{
    /// <summary>The scene's published pixels.</summary>
    extension(FileSelectMapMenuState scene)
    {
        /// <summary>Captures the scene and draws its snapshot with the game's renderer.</summary>
        internal Rgba32[] Render() => FrontendScenePixelsAccess.Pixels(scene.CaptureRenderSnapshot());
    }
}
