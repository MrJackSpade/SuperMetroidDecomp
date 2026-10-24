using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rendering;
using SuperMetroid.TestSupport.FrameRendering;

namespace SuperMetroid.Core.Frontend.Title;

/// <summary>Verification access: the TitleSequenceState's pixels through the one published path.</summary>
internal static class TitleSequenceStatePixelsAccess
{
    /// <summary>The scene's published pixels.</summary>
    extension(TitleSequenceState scene)
    {
        /// <summary>Captures the scene and draws its snapshot with the game's renderer.</summary>
        internal Rgba32[] Render() => DisplayRendering.Render(new DisplayPacket(new RenderFrameIdentity(1, 1, 0), scene.CaptureRenderSnapshot()));
    }
}
