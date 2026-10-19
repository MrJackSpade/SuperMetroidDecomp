using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rendering;

namespace SuperMetroid.Core.Frontend.Ending;

/// <summary>Verification access: the EndingCreditsPresentation's pixels through the one published path.</summary>
internal static class EndingCreditsPresentationPixelsAccess
{
    /// <summary>The scene's published pixels.</summary>
    extension(EndingCreditsPresentation scene)
    {
        /// <summary>Captures the scene and rasterizes its snapshot in software.</summary>
        internal Rgba32[] Render() => FrontendScenePixelsAccess.Pixels(scene.CaptureRenderSnapshot());
    }
}
