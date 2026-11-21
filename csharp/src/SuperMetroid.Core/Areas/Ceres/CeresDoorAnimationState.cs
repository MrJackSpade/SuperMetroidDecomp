using SuperMetroid.Core.Animation;

namespace SuperMetroid.Core.Areas.Ceres;

/// <summary>The animation a Ceres door or wall plays (#1953), shared by every one of them.</summary>
internal sealed class CeresDoorAnimationState
{
    /// <summary>The animation and where it stands in it (#1600), set when the door or wall starts.</summary>
    internal AnimationPlayback<CeresDoorSegment> Playback { get; set; }

    /// <summary>
    /// How the door shows (#2545, #2573): its actor frames, or its overlay picture from its animation's showing it until
    /// it shows its actor again. Each door's animation switches its own look, as each door enemy holds its own
    /// drawn-by-Ridley flag ($A6:F69F, $A6:F6B0); only the animations of the door of Ridley's room switch it, and only that
    /// door's overlay is drawn (#2554).
    /// </summary>
    public CeresRidleyRoomDoorLook Look { get; internal set; }
}
