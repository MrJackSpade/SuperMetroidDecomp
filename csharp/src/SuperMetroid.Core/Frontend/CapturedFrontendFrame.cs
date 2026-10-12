using SuperMetroid.Core.Rendering;

namespace SuperMetroid.Core.Frontend;

/// <summary>
/// Explicit staged output: Snapshot is GPU-ready data for extracted scenes; otherwise
/// Frame.Pixels contains legacy raster output. This distinction must remain visible in
/// backend coverage/telemetry. The normal desktop uses captured output; legacy
/// callers can still explicitly request raster output through Step.
/// </summary>
public readonly record struct CapturedFrontendFrame(FrontendFrame Frame, RenderFrameSnapshot? Snapshot)
{
}
