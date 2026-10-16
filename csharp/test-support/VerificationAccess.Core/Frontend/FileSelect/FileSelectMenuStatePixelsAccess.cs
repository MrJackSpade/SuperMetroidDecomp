using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rendering;

namespace SuperMetroid.Core.Frontend.FileSelect;

/// <summary>Verification access: the FileSelectMenuState's pixels through the one published path.</summary>
internal static class FileSelectMenuStatePixelsAccess
{
    /// <summary>The scene's published pixels.</summary>
    extension(FileSelectMenuState scene)
    {
        /// <summary>Captures the scene and rasterizes its snapshot in software.</summary>
        internal Rgba32[] Render() => FrontendScenePixelsAccess.Pixels(scene.CaptureRenderSnapshot());
    }
}
