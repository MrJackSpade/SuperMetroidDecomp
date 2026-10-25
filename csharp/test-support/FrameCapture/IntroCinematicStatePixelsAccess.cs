using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rendering;

namespace SuperMetroid.Core.Frontend.Intro;

/// <summary>Verification access: the IntroCinematicState's pixels through the one published path.</summary>
internal static class IntroCinematicStatePixelsAccess
{
    /// <summary>The scene's published pixels.</summary>
    extension(IntroCinematicState scene)
    {
        /// <summary>Captures the scene and draws its snapshot with the game's renderer.</summary>
        internal RgbaPixel[] Render() => FrontendScenePixelsAccess.Pixels(scene.CaptureTranslatedRenderSnapshot());

        /// <summary>
        /// The scene's art at full brightness: what its layers and sprites show, whatever fade the scene is in, so an
        /// artwork check compares art even while the scene fades in from black.
        /// </summary>
        internal RgbaPixel[] RenderArtwork()
        {
            LayeredScene shown = scene.CaptureTranslatedRenderSnapshot();
            return FrontendScenePixelsAccess.Pixels(new LayeredScene(shown.Paint, shown.Layers, FadeLevel.Full));
        }
    }
}
