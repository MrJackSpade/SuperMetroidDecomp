namespace SuperMetroid.Core.Areas.Tourian;

/// <summary>
/// Where the Tourian statue room's releases stand (#2545): the statues release one at a time, and the descent latches that
/// all four have released, possibly while the last release still plays.
/// </summary>
public enum TourianStatueReleasePhase
{
    /// <summary>No statue is releasing, and not all four have released.</summary>
    NoneReleasing,

    /// <summary>One statue plays its release; the others keep idling until it finishes.</summary>
    OneReleasing,

    /// <summary>All four have released, the descent has latched it, and the last release still plays.</summary>
    AllReleasedWhileLastReleases,

    /// <summary>All four have released and none is releasing: the descent goes on to its hold.</summary>
    AllReleased,
}
