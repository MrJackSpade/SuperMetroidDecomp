namespace SuperMetroid.Core.Enemies.Platforms;

/// <summary>
/// The Tripper's and Kamer's vertically moving and still platform loops as typed programs (#641), one per animation a spawn, branch or resumed sleep enters, in authored order.
/// Their steps are <see cref="PlatformAnimations"/>.
/// </summary>
internal enum PlatformProgram
{
    /// <summary>The kamer moving left animation.</summary>
    KamerMovingLeft,
    /// <summary>The loop.</summary>
    KamerMovingLeftLoop,
    /// <summary>The kamer moving right animation.</summary>
    KamerMovingRight,
    /// <summary>The loop.</summary>
    KamerMovingRightLoop,
    /// <summary>The kamer still left animation.</summary>
    KamerStillLeft,
    /// <summary>The loop.</summary>
    KamerStillLeftLoop,
    /// <summary>The kamer still right animation.</summary>
    KamerStillRight,
    /// <summary>The loop.</summary>
    KamerStillRightLoop,
    /// <summary>The tripper moving left animation.</summary>
    TripperMovingLeft,
    /// <summary>The loop.</summary>
    TripperMovingLeftLoop,
    /// <summary>The tripper moving right animation.</summary>
    TripperMovingRight,
    /// <summary>The loop.</summary>
    TripperMovingRightLoop,
    /// <summary>The tripper still moving left animation.</summary>
    TripperStillMovingLeft,
    /// <summary>The loop.</summary>
    TripperStillMovingLeftLoop,
    /// <summary>The tripper still moving right animation.</summary>
    TripperStillMovingRight,
    /// <summary>The loop.</summary>
    TripperStillMovingRightLoop,
}
