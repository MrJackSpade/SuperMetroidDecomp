using SuperMetroid.Core.Animation;

namespace SuperMetroid.Core.Areas.Ceres;

/// <summary>The animation a Ceres door or wall plays (#1953), shared by every one of them.</summary>
internal sealed class CeresDoorAnimationState
{
    /// <summary>The animation and where it stands in it (#1600), set when the door or wall starts.</summary>
    internal AnimationPlayback<CeresDoorSegment> Playback { get; set; }

    /// <summary>
    /// How the door shows: its actor frames, or its overlay picture from its animation's showing it until it shows its actor
    /// again. Only the door of Ceres Ridley's room plays an animation that shows its overlay (#2545, #2573).
    /// </summary>
    internal CeresRidleyRoomDoorLook Look { get; set; }
}
