using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Frontend.CeresCinematics;

/// <summary>Verification access: the CeresDestructionCinematicState's pixels through the one published path.</summary>
internal static class CeresDestructionCinematicStatePixelsAccess
{
    /// <summary>The scene's published pixels.</summary>
    extension(CeresDestructionCinematicState scene)
    {
        /// <summary>Captures the scene and rasterizes its snapshot in software.</summary>
        internal Rgba32[] Render() => FrontendScenePixelsAccess.Pixels(scene.CaptureRenderSnapshot());
    }
}
