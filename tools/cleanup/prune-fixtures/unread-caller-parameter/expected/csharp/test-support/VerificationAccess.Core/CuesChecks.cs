using SuperMetroid.Core;

namespace SuperMetroid.Verification;

/// <summary>The cues the checks read.</summary>
internal static class CuesChecks
{
    /// <summary>Whether the left cue points left.</summary>
    internal static bool PointsLeft => Cues.Left.Direction < 0;
}
