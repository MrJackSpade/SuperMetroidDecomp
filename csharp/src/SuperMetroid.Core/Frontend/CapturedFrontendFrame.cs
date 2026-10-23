using SuperMetroid.Core.Rendering;

namespace SuperMetroid.Core.Frontend;

/// <summary>
/// One captured frame: the frontend frame's state and its published render snapshot, which every backend draws.
/// </summary>
public readonly record struct CapturedFrontendFrame(FrontendFrame Frame, DisplayPacket Snapshot)
{
}
