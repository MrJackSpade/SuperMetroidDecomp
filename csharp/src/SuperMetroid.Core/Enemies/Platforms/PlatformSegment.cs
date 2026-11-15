namespace SuperMetroid.Core.Enemies.Platforms;

/// <summary>
/// The Tripper's and the suspensor platform's vertically moving and still platform loops (#641): one segment per animation a spawn, branch or resumed sleep enters, in authored order.
/// Their steps are <see cref="PlatformAnimations"/>.
/// </summary>
internal enum PlatformSegment
{
    /// <summary>The suspensor platform heading left, moving or still: it moves left and plays its loop (#1842).</summary>
    SuspensorPlatformHeadingLeft,
    /// <summary>The suspensor platform's loop, the same whichever way it heads and whether it moves or stays still (#1842).</summary>
    SuspensorPlatformLoop,
    /// <summary>The suspensor platform heading right, moving or still: it moves right and plays its loop (#1842).</summary>
    SuspensorPlatformHeadingRight,
    /// <summary>The Tripper moving left animation.</summary>
    TripperMovingLeft,
    /// <summary>The loop.</summary>
    TripperMovingLeftLoop,
    /// <summary>The Tripper moving right animation.</summary>
    TripperMovingRight,
    /// <summary>The loop.</summary>
    TripperMovingRightLoop,
    /// <summary>The Tripper still moving left animation.</summary>
    TripperStillMovingLeft,
    /// <summary>The loop.</summary>
    TripperStillMovingLeftLoop,
    /// <summary>The Tripper still moving right animation.</summary>
    TripperStillMovingRight,
    /// <summary>The loop.</summary>
    TripperStillMovingRightLoop,
}
