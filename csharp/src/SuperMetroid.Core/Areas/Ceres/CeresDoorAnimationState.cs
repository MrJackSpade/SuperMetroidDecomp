using SuperMetroid.Core.Animation;

namespace SuperMetroid.Core.Areas.Ceres;

/// <summary>The animation a Ceres door or wall plays (#1953), shared by every one of them.</summary>
internal sealed class CeresDoorAnimationState
{
    /// <summary>The animation and where it stands in it (#1600), set when the door or wall starts.</summary>
    internal AnimationPlayback<CeresDoorSegment> Playback { get; set; }
}
