namespace SuperMetroid.Core.Enemies.Platforms;

/// <summary>
/// The Tripper's and Kamer's vertically moving and still platform loops as typed programs (#641), one per animation a spawn, branch or resumed sleep enters, in authored order.
/// Their steps are <see cref="PlatformAnimations"/>.
/// </summary>
internal enum PlatformSegment
{
    /// <summary>The Kamer moving left animation.</summary>
    KamerMovingLeft,
    /// <summary>The loop.</summary>
    KamerMovingLeftLoop,
    /// <summary>The Kamer moving right animation.</summary>
    KamerMovingRight,
    /// <summary>The loop.</summary>
    KamerMovingRightLoop,
    /// <summary>The Kamer still left animation.</summary>
    KamerStillLeft,
    /// <summary>The loop.</summary>
    KamerStillLeftLoop,
    /// <summary>The Kamer still right animation.</summary>
    KamerStillRight,
    /// <summary>The loop.</summary>
    KamerStillRightLoop,
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
