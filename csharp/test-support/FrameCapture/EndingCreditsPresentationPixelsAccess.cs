using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Frontend.Ending;

/// <summary>Verification access: the EndingCreditsPresentation's pixels through the one published path.</summary>
internal static class EndingCreditsPresentationPixelsAccess
{
    /// <summary>The scene's published pixels.</summary>
    extension(EndingCreditsState scene)
    {
        /// <summary>Captures the scene and draws its snapshot with the game's renderer.</summary>
        internal RgbaPixel[] Render() => FrontendScenePixelsAccess.Pixels(EndingCreditsPresentation.CaptureRenderSnapshot(scene));
    }
}
