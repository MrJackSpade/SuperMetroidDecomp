using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Frontend.FileSelect;

/// <summary>Verification access: the FileSelectMenuState's pixels through the one published path.</summary>
internal static class FileSelectMenuStatePixelsAccess
{
    /// <summary>The scene's published pixels.</summary>
    extension(FileSelectMenuState scene)
    {
        /// <summary>Captures the scene and draws its snapshot with the game's renderer.</summary>
        internal Rgba32[] Render() => FrontendScenePixelsAccess.Pixels(scene.CaptureRenderSnapshot());
    }
}
