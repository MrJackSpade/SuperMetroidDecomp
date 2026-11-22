using SuperMetroid.Core.Title;

namespace SuperMetroid.Verification.Title;

/// <summary>The scenes the checks read.</summary>
internal static class ScenesChecks
{
    /// <summary>Whether the first pan zooms in.</summary>
    internal static bool FirstPanZooms => Scenes.FirstPan.Zoom > 0;
}
