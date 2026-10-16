using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rendering;

namespace SuperMetroid.Core.Frontend.Intro;

/// <summary>Verification access: the IntroCinematicState's pixels through the one published path.</summary>
internal static class IntroCinematicStatePixelsAccess
{
    /// <summary>The scene's published pixels.</summary>
    extension(IntroCinematicState scene)
    {
        /// <summary>Captures the scene and rasterizes its snapshot in software.</summary>
        internal Rgba32[] Render() => FrontendScenePixelsAccess.Pixels(scene.CaptureTranslatedRenderSnapshot());
    }
}
